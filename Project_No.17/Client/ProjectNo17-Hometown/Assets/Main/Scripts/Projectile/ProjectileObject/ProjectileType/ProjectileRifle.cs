using BC.ODCC;
using BC.OdccBase;

using UnityEngine;

namespace BC.Projectile
{
	public partial class ProjectileRifle : ProjectileObject, IOdccUpdate.Late
	{
		private ProjectileOrderData orderData;
		private ProjectileLifeData lifeData;
		private ProjectileMoveData moveData;
		private Rigidbody thisRigidbody;
		private bool isFire = false;

		protected override void BaseDestroy()
		{
			orderData= null;
			lifeData= null;
			moveData= null;
			thisRigidbody = null;
		}

		protected override void Fire()
		{
			if(ThisContainer.TryGetData<ProjectileOrderData>(out orderData))
			{
			}
			if(ThisContainer.TryGetData<ProjectileLifeData>(out lifeData))
			{
				lifeData.OnDead = OnDead;
			}

			if(ThisContainer.TryGetData<ProjectileMoveData>(out moveData))
			{
				ThisTransform.position = moveData.startPosition;
				ThisTransform.rotation = Quaternion.LookRotation(moveData.Forward, Vector3.up);
			}

			if(thisRigidbody == null)
			{
				thisRigidbody = GetComponentInChildren<Rigidbody>(true);
			}
			isFire = true;
		}

		void IOdccUpdate.Late.BaseLateUpdate()
		{
			if(!isFire) return;
			if(lifeData != null && lifeData.IsAlive())
			{
				lifeData.LifeTime -= Time.deltaTime;
			}
		}
		protected override void MovementUpdate(in Vector3 velocity, in float deltaTime)
		{
			if(thisRigidbody == null)
			{
				ThisTransform.forward = velocity.normalized;
				ThisTransform.position += velocity * deltaTime;
			}
			else
			{
				ThisTransform.forward = velocity.normalized;
				thisRigidbody.linearVelocity = velocity;
			}
		}

		protected override void SendHit(Collision collision, IReactionTarget reactionTarget)
		{
			ProjectileManager.ProjectileHit_Reaction(this, orderData, reactionTarget);
		}
		protected override void SendHitNoReaction(Collision collision)
		{
			ProjectileManager.ProjectileHit_NoReaction(this, orderData, collision);
		}
		protected override void SendHitBlocking(Collision something)
		{
			ProjectileManager.ProjectileHit_Blocking(this, orderData, something);
		}

		protected override void Dead()
		{
			ProjectileManager.Manager.DestroyProjectile(this);
		}

	}
}