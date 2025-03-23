using BC.AIProvider;
using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public interface IProjectileLifecycle : IOdccComponent
	{
		void Fire();
		void Update(float deltaTime);
		void Hit(Collision collision, ReactionTarget reactionTarget) { }
		void Hit(Collision something, bool isBlocker) { }
		void Dead();
	}
	public abstract class ProjectileObject : ObjectBehaviour
	{
		public void OnFire()
		{
			GameObject.SetActive(true);
			Fire();
			ThisContainer.CallActionAllComponent<IProjectileLifecycle>(call => call.Fire());
		}
		public void OnUpdate(in Vector3 velocity, float deltaTime)
		{
			MovementUpdate(in velocity, in deltaTime);
			ThisContainer.CallActionAllComponent<IProjectileLifecycle>(call => call.Update(deltaTime));
		}
		public void OnHit(Collision collision, ReactionTarget reactionTarget)
		{
			Hit(collision, reactionTarget);
			ThisContainer.CallActionAllComponent<IProjectileLifecycle>(call => call.Hit(collision, reactionTarget));
		}
		public void OnHit(Collision something, bool isBlocker)
		{
			Hit(something, isBlocker);
			ThisContainer.CallActionAllComponent<IProjectileLifecycle>(call => call.Hit(something, isBlocker));
		}
		public void OnDead()
		{
			Dead();
			ThisContainer.CallActionAllComponent<IProjectileLifecycle>(call => call.Dead());
		}
		protected abstract void Fire();
		protected abstract void MovementUpdate(in Vector3 velocity, in float deltaTime);
		protected abstract void Hit(Collision collision, ReactionTarget reactionTarget);
		protected abstract void Hit(Collision something, bool isBlocker);
		protected abstract void Dead();
	}
}