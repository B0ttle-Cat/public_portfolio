using System.Collections.Generic;

using BC.Base;
using BC.ODCC;
using BC.OdccBase;

using UnityEngine;
namespace BC.Projectile
{
	public class ProjectileHitEventReceiver : ComponentBehaviour, IProjectileLifecycle
	{
		private ProjectileObject projectileObject;
		private Rigidbody thisRigidbody;
		private Collider thisCollider;


		private bool isAlive;

		private Vector3 prevPosition;

		private ProjectileOrderData orderData;
		private ProjectileHitData hitData;
		private ProjectileLifeData lifeData;

		protected HashSet<Collider> hitCollider;

		protected override void BaseDestroy()
		{
			projectileObject = null;
			thisRigidbody = null;
			orderData = null;
			hitData = null;
			lifeData = null;
			hitCollider = null;
		}


		private bool IsValid() => isAlive
			&& projectileObject != null && orderData!=null && hitData != null && lifeData != null;

		private void OnCollisionEnter(Collision collision)
		{
			if(!IsValid()) return;
			if(!hitCollider.Add(collision.collider)) return;
			Physics.IgnoreCollision(thisCollider, collision.collider, true);

			int hitLayer = collision.gameObject.layer;
			LayerMask blockingLayerMask = hitData.blockingLayer;
			if(blockingLayerMask.HasLayer(hitLayer))
			{
				projectileObject.OnHitBlocking(collision);
				return;
			}
			LayerMask hitEffectLayerMask = hitData.hitEffectLayer;
			if(hitEffectLayerMask.HasLayer(hitLayer))
			{
				if(collision.collider.TryGetComponent<IReactionTarget>(out var hitTarget))
				{
					hitTarget.ThisContainer.TryGetComponent<IProjectileIFF>(out var iffProvider);
					var unitRelationType = orderData.GetHitTargetRelation(iffProvider);
					if(hitData.hitUnitRelationType.HasFlag(unitRelationType))
					{
						projectileObject.OnHit(collision, hitTarget);
					}
					else
					{
						// Pass Hit (피격 판정 대상이 아님)
					}
				}
				else
				{
					projectileObject.OnHitNoReactionTarget(collision);
				}
			}
		}

		void IProjectileLifecycle.Fire()
		{
			isAlive = true;
			if(thisRigidbody == null)
			{
				thisRigidbody = GameObject.GetComponentInChildren<Rigidbody>(true) ?? GameObject.AddComponent<Rigidbody>();

				thisRigidbody.useGravity = false;
				thisRigidbody.isKinematic = false;
				thisRigidbody.linearDamping = 0f;
				thisRigidbody.angularDamping = 0f;
				thisRigidbody.mass = 1f;
				thisRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
				thisRigidbody.angularVelocity = Vector3.zero;
				thisRigidbody.linearVelocity = Vector3.zero;
				thisRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			}
			if(thisCollider == null)
			{
				thisCollider = GameObject.GetComponentInChildren<Collider>(true);
			}
			if(thisCollider == null) return;

			projectileObject = ThisContainer.GetObject<ProjectileObject>();
			ThisContainer.TryGetData<ProjectileOrderData>(out orderData);
			if(ThisContainer.TryGetData<ProjectileHitData>(out hitData))
			{
				thisCollider.gameObject.layer = hitData.projectileLayer.ToLayer();
			}
			ThisContainer.TryGetData<ProjectileLifeData>(out lifeData);

			if(hitCollider != null && hitCollider.Count > 0)
			{
				foreach(var item in hitCollider)
				{
					Physics.IgnoreCollision(thisCollider, item, false);
				}
			}
			hitCollider = new HashSet<Collider>();

			thisRigidbody.WakeUp();
		}

		void IProjectileLifecycle.Update(float deltaTime)
		{
			prevPosition = thisRigidbody != null ? thisRigidbody.position : transform.position;
		}
		void IProjectileLifecycle.Hit(Collision collision, IReactionTarget hitTarget)
		{
			lifeData.HitCount--;
			if(lifeData.IsDead())
			{
				HitDead(collision);
			}
		}
		void IProjectileLifecycle.HitNoReactionTarget(Collision collision)
		{
			lifeData.HitCount--;
			if(lifeData.IsDead())
			{
				HitDead(collision);
			}
		}
		void IProjectileLifecycle.HitBlocking(Collision something)
		{
			lifeData.HitCount = 0;
			if(lifeData.IsDead())
			{
				HitDead(something);
			}
		}

		async Awaitable IProjectileLifecycle.Dead()
		{
			isAlive = false;
		}

		private void HitDead(Collision lastHitCollision)
		{
			Vector3 directionA = (thisRigidbody.position - prevPosition).normalized;
			ContactPoint contactPoint = lastHitCollision.GetContact(0);
			Vector3 directionB = (contactPoint.point - prevPosition);
			//contactPoint.point
			directionA = Vector3.Project(directionB, directionA);
			prevPosition += directionA;
			thisRigidbody.position = prevPosition;
			ThisTransform.position = prevPosition;
			thisRigidbody.Sleep();
		}
	}
}