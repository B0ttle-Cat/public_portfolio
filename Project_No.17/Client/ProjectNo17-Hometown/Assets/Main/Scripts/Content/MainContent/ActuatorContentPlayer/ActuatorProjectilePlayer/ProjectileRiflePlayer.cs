using BC.AIProvider;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFContent
{
	[CreateAssetMenu(fileName = nameof(ProjectileRiflePlayer), menuName = "BC/ActuatorModule/Player/"+nameof(ProjectileRiflePlayer))]
	public class ProjectileRiflePlayer : ProjectileActuatorPlayer<ProjectileRifle>
	{
		[Title("Projectile Instance Option")]
		[MinValue(1)]
		public int instanceCount = 1;
		[MinValue(0)]
		public float instanceDelay = 0.1f;
		//private ProjectileInstantiateData instantiateData;

		private ProjectileRifle[] instanceProjectile;
		private AbilityProvider orderAbility;

		protected override async Awaitable<bool> Start()
		{
			if(!SetPoolingProjectile())
			{
				return false;
			}
			return actuator.HasEffect && order.ThisContainer.TryGetComponent<AbilityProvider>(out orderAbility);
		}

		protected override async Awaitable<bool> Casting()
		{
			if(!order.ThisContainer.TryGetComponent<AnimatorComponent>(out var animatorController)) return true;
			return true;
		}

		protected override async Awaitable<bool> Action()
		{
			if(!order.ThisContainer.TryGetComponent<TransformMapping>(out var transformMap)) return false;
			if(!transformMap.TryFindTransform("ProjectileStartPoint", out var projectileStartPoint)) return false;

			instanceProjectile =  await InstanceProjectile(instanceCount, false);
			int length = instanceProjectile == null ? 0 : instanceProjectile.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var projectile = instanceProjectile[i];
				if(projectile == null) continue;
				if(i > 0)
				{
					await Awaitable.WaitForSecondsAsync(instanceDelay);
				}

				Vector3 startPosition = projectileStartPoint.position;

				var container = projectile.ThisContainer;
				container.InitData<ProjectileOrderData>((data) => {
					data.orderObject = order;
					data.orderIFFProvider = order.ThisContainer.GetComponent<IFFProvider>();
					data.actuatorModule = actuator;
					data.actuatorPlayer = this;
				});

				container.InitData<ProjectileLifeData>((data) => {
					float lifeTime = maxLifeTime;
					int hitCount = maxHitCount;
					if(lifeTime <=0 || lifeTime > ProjectileLifeData.LimitLifeTime) lifeTime = ProjectileLifeData.LimitLifeTime;
					if(hitCount <=0 || hitCount > ProjectileLifeData.LimitHitCount) hitCount = ProjectileLifeData.LimitHitCount;
					data.InitLife(lifeTime, hitCount);
					data.OnDead = null;
				});

				container.InitData<ProjectileHitData>((data) => {
					data.projectileLayer = projectileLayer;
					data.blockingLayer = blockingLayer;
					data.hitEffectLayer = hitEffectLayer;
					data.hitUnitRelationType = hitUnitRelationType;
				});

				container.RemoveData<ProjectileMoveData>();
				if(hasTarget)
				{
					container.InitData<ProjectileMoveTrackingData>((data) => {
						data.startPosition = startPosition;
						data.trackingObject = targetObject;
						data.targetPosition = data.GetTrackingPosition();
						data.moveSpeed = moveSpeed;
					});
				}
				else
				{
					container.InitData<ProjectileMoveTowardsData>((data) => {
						data.startPosition = startPosition;
						data.targetPosition = targetPosition;
						data.moveSpeed = moveSpeed;
					});
				}
				projectile.OnFire();
			}

			return true;
		}


		protected override async Awaitable<bool> Cooldown()
		{
			await Awaitable.NextFrameAsync();
			return true;
		}
		protected override async Awaitable OnCancelActuator()
		{
			await Awaitable.NextFrameAsync();
		}

		protected override async Awaitable OnEndedActuator()
		{
			await Awaitable.NextFrameAsync();
		}
		protected override bool OnCheckIdDestroy()
		{
			if(instanceProjectile == null) return true;
			int length = instanceProjectile.Length;
			for(int i = 0 ; i < length ; i++)
			{
				if(instanceProjectile[i] != null) return false;
			}
			return true;
		}

		public override void ProjectileHit(ProjectileRifle projectileObject, ReactionTarget reactionTarget)
		{
			AbilityProvider order = orderAbility;
			AbilityProvider target = reactionTarget == null ? null : reactionTarget.ThisContainer.GetComponent<AbilityProvider>();
			if(order == null || target == null) return;

			if(actuator.ComputeActuatorEffect(order, target, ActuatorEffectType.Chance_Hit))
			{
				ActuatorEffectType damageType = actuator.ComputeActuatorEffect(order, target, ActuatorEffectType.Chance_Critical) ? ActuatorEffectType.Damage_Shield_Critical : ActuatorEffectType.Damage_Shield;

				actuator.ComputeActuatorEffect(order, target, damageType, out float damage_Shield);

				if(reactionTarget.ThisContainer.TryGetComponent<IActuatorEffectListener>(out var unitCommand))
				{
					unitCommand.OnEffectValue(damageType, damage_Shield);
				}
			}
		}

		public override void ShapeEffectHit(ReactionTarget[] reactionTarget)
		{
		}

		public override void OnProjectileDead(ProjectileRifle deadProjectile)
		{
			if(instanceProjectile != null)
			{
				int length = instanceProjectile.Length;
				for(int i = 0 ; i < length ; i++)
				{
					if(instanceProjectile[i] == deadProjectile)
					{
						DestroyProjectile(deadProjectile);
						instanceProjectile[i] = null;
						return;
					}
				}
			}
		}


		protected override void OnDestroy()
		{
			if(instanceProjectile != null)
			{
				int length = instanceProjectile.Length;
				for(int i = 0 ; i < length ; i++)
				{
					if(instanceProjectile[i] != null)
					{
						instanceProjectile[i].DestroyThis(true);
					}
				}
			}
			instanceProjectile = null;
			base.OnDestroy();
		}
	}
}
