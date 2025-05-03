using System.Linq;

using BC.ODCC;
using BC.OdccBase;

using UnityEngine;
namespace BC.Projectile
{
	public class ProjectileManager : ObjectBehaviour, IOdccUpdate.Late, IProjectileManager
	{
		public static ProjectileManager Manager { get; private set; }
		private ProjectilePooling ProjectilePooling { get; set; }

		protected override void BaseAwake()
		{
			Manager = this;
		}

		protected override void BaseStart()
		{
			ProjectilePooling = ThisContainer.GetComponent<ProjectilePooling>();
		}

		void IOdccUpdate.Late.BaseLateUpdate()
		{

		}
		public async Awaitable<GameObject> InstanceProjectile(string projectileType, bool isActive = true)
		{
			ProjectileObject projectile = ProjectilePooling == null ? null : await ProjectilePooling.PopProjectile(projectileType, isActive);
			return projectile.GameObject;
		}
		public async Awaitable<GameObject[]> InstanceProjectile(string projectileType, int count, bool isActive = true)
		{
			ProjectileObject[] projectiles = ProjectilePooling == null ? (new ProjectileObject[0]) : await ProjectilePooling.PopProjectile(projectileType, count, isActive);
			if(projectiles == null) return null;
			return projectiles.Select((item) => item.GameObject).ToArray();
		}
		public async Awaitable<TProjectile> InstanceProjectile<TProjectile>(string projectileType, bool isActive = true) where TProjectile : class, IProjectileObject
		{
			ProjectileObject projectile = ProjectilePooling == null ? null : await ProjectilePooling.PopProjectile(projectileType, isActive);
			if(projectile == null || projectile is not TProjectile tProjectile) return null;
			return tProjectile;
		}
		public async Awaitable<TProjectile[]> InstanceProjectile<TProjectile>(string projectileType, int count, bool isActive = true) where TProjectile : class, IProjectileObject
		{
			ProjectileObject[] projectiles = ProjectilePooling == null ? (new ProjectileObject[0]) : await ProjectilePooling.PopProjectile(projectileType, count, isActive);
			if(projectiles == null) return null;
			return projectiles.Select((item) => item as TProjectile).ToArray();
		}
		public void DestroyProjectile(ProjectileObject projectileObject)
		{
			if(projectileObject == null) return;
			if(ProjectilePooling == null)
			{
				projectileObject.DestroyThis();
				return;
			}

			ProjectilePooling.PushProjectile(projectileObject);
		}
		public void DestroyProjectile(ProjectileObject[] projectileObjects)
		{
			if(projectileObjects == null) return;
			if(ProjectilePooling == null)
			{
				foreach(var item in projectileObjects)
				{
					item.DestroyThis();
				}
				return;
			}

			ProjectilePooling.PushProjectile(projectileObjects);
		}

		public bool SetPoolingProjectile(GameObject prefab)
		{
			if(ProjectilePooling == null) return false;
			if(prefab == null || !prefab.TryGetComponent<ProjectileObject>(out var projectilePrefab)) return false;
			return ProjectilePooling.SetPoolingProjectile(projectilePrefab);
		}

		public static void ProjectileHit(IProjectileObject projectileObject, IAbilityProvider order, IAbilityProvider target, IActuatorModule actuator)
		{
			if(projectileObject == null || order == null || target == null || actuator == null) return;

			if(!actuator.CalculateBooleanEffect(order, target, ActuatorEffectType.Chance_AttackHit, false)) return;

			ActuatorEffectType damageType = ComputeAttackChance(actuator, order, target);
			ComputeAttackDamage(actuator, order, target, ref damageType, out float damagePoint);

			if(target.ThisContainer.TryGetComponent<IActuatorEffectListener>(out var unitCommand))
			{
				unitCommand.OnEffectValue(projectileObject, damageType, damagePoint);
			}

			static ActuatorEffectType ComputeAttackChance(IActuatorModule actuator, IAbilityProvider order, IAbilityProvider target)
			{
				if(!actuator.CalculateBooleanEffect(order, target, ActuatorEffectType.Chance_AttackHit, false))
				{
					return ActuatorEffectType.Damage_Miss;
				}
				else if(!actuator.CalculateBooleanEffect(order, target, ActuatorEffectType.Chance_BlockingPass, true))
				{
					return actuator.HasCalculateEffectType(ActuatorEffectType.Damage_Shield_Blocking)
						? ActuatorEffectType.Damage_Shield_Blocking
						: ActuatorEffectType.Damage_Shield_Attack;
				}
				else if(actuator.CalculateBooleanEffect(order, target, ActuatorEffectType.Chance_CriticalHit, false))
				{
					return actuator.HasCalculateEffectType(ActuatorEffectType.Damage_Shield_Critical)
						? ActuatorEffectType.Damage_Shield_Critical
						: ActuatorEffectType.Damage_Shield_Attack;
				}
				else
				{
					return ActuatorEffectType.Damage_Shield_Attack;
				}
			}
			static void ComputeAttackDamage(IActuatorModule actuator, IAbilityProvider order, IAbilityProvider target, ref ActuatorEffectType type, out float damage)
			{
				if(type == ActuatorEffectType.Damage_Miss)
				{
					damage = 0f;
					return;
				}
				else if(!actuator.TryCalculateFloatEffect(order, target, type, out damage))
				{
					damage = 0f;
					type = ActuatorEffectType.Damage_Miss;
				}
			}
		}
		public static void ProjectileHit_Reaction(ProjectileObject projectileObject, ProjectileOrderData projectileOrderData, IReactionTarget reactionTarget)
		{
			IAbilityProvider order = projectileOrderData.orderObject.ThisContainer.GetComponent<IAbilityProvider>();
			IAbilityProvider target = reactionTarget == null ? null : reactionTarget.ThisContainer.GetComponent<IAbilityProvider>();
			IActuatorModule actuator = projectileOrderData.actuatorModule;
			ProjectileHit(projectileObject, order, target, actuator);
		}
		public static void ProjectileHit_NoReaction(ProjectileObject projectileObject, ProjectileOrderData projectileOrderData, Collision reactionTarget)
		{
			var order = projectileOrderData.orderObject.ThisContainer.GetComponent<IAbilityProvider>();
			if(order == null) return;
			var targetObjectBehaviour = reactionTarget.collider.gameObject.GetComponentInParent<ObjectBehaviour>();
			if(targetObjectBehaviour == null) return;
			if(!targetObjectBehaviour.TryGetComponent<IAbilityProvider>(out var target)) return;
			IActuatorModule actuator = projectileOrderData.actuatorModule;

			ProjectileHit(projectileObject, order, target, actuator);
		}
		public static void ProjectileHit_Blocking(ProjectileObject projectileObject, ProjectileOrderData projectileOrderData, Collision reactionTarget)
		{
			var order = projectileOrderData.orderObject.ThisContainer.GetComponent<IAbilityProvider>();
			if(order == null) return;
			var targetObjectBehaviour = reactionTarget.collider.gameObject.GetComponentInParent<ObjectBehaviour>();
			if(targetObjectBehaviour == null) return;
			if(!targetObjectBehaviour.TryGetComponent<IAbilityProvider>(out var target)) return;
			IActuatorModule actuator = projectileOrderData.actuatorModule;

			ProjectileHit(projectileObject, order, target, actuator);
		}
	}
}