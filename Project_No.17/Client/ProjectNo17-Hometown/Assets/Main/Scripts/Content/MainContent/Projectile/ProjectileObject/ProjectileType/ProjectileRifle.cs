
using BC.AIProvider;
using BC.ODCC;

using UnityEngine;

namespace TFContent
{
	public class ProjectileRifle : ProjectileObject, IOdccUpdate.Late
	{
		private ProjectileActuatorPlayer<ProjectileRifle> actuatorPlayer;
		private ProjectileOrderData orderData;
		private ProjectileLifeData lifeData;
		private ProjectileMoveData moveData;
		private Rigidbody thisRigidbody;
		private bool isFire = false;

		protected override void BaseDestroy()
		{
			actuatorPlayer = null;
			orderData= null;
			lifeData= null;
			moveData= null;
			thisRigidbody = null;
		}



		protected override void Fire()
		{
			actuatorPlayer = null;
			if(ThisContainer.TryGetData<ProjectileOrderData>(out orderData))
			{
				if(orderData.actuatorPlayer is ProjectileActuatorPlayer<ProjectileRifle> player)
				{
					actuatorPlayer = player;
				}
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

		protected override void Hit(Collision collision, ReactionTarget reactionTarget)
		{
			if(actuatorPlayer != null) actuatorPlayer.ProjectileHit(this, reactionTarget);
		}

		protected override void Hit(Collision something, bool isBlocker)
		{

		}

		protected override async void Dead()
		{
			await Awaitable.WaitForSecondsAsync(1f);
			if(actuatorPlayer != null) actuatorPlayer.OnProjectileDead(this);
		}
	}
}