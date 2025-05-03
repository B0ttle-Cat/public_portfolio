using BC.Base;
using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;
namespace BC.Projectile
{
	public interface IProjectileLifecycle : IOdccComponent
	{
		void Fire();
		void Update(float deltaTime);
		void Hit(Collision collision, IReactionTarget hitTarget) { }
		void HitNoReactionTarget(Collision collision) { }
		void HitBlocking(Collision something) { }
		Awaitable Dead();
	}
	public abstract class ProjectileObject : ObjectBehaviour, IProjectileObject
	{
		[ShowInInspector, ReadOnly]
		internal string poolingKey { get; set; }
		public virtual bool Init(ObjectBehaviour order, IActuatorModule actuatorModule, IInstanceObject effectorObject, ReactionTargetInfo targetValue)
		{
			if(!ThisContainer.TryGetData<ProjectileInitData>(out var initData)) return false;
			if(!order.ThisContainer.TryGetComponent<TransformMapping>(out var transformMap)) return false;
			if(!transformMap.TryFindTransform("ProjectileStartPoint", out var projectileStartPoint)) return false;

			Vector3 startPosition = projectileStartPoint.position;

			var container = ThisContainer;

			ThisContainer.InitData<ProjectileOrderData>((data) => {
				data.orderObject = order;
				data.orderIFFProvider = order.ThisContainer.GetComponent<IProjectileIFF>();
				data.abilityStruct = order.ThisContainer.GetComponent<IAbilityProvider>();
				data.actuatorModule = actuatorModule;
				data.effectorObject = effectorObject;
			});

			ThisContainer.InitData<ProjectileLifeData>((data) => {
				float lifeTime = initData.maxLifeTime;
				int hitCount = initData.maxHitCount;
				if(lifeTime <=0 || lifeTime > ProjectileLifeData.LimitLifeTime) lifeTime = ProjectileLifeData.LimitLifeTime;
				if(hitCount <=0 || hitCount > ProjectileLifeData.LimitHitCount) hitCount = ProjectileLifeData.LimitHitCount;
				data.InitLife(lifeTime, hitCount);
				data.OnDead = null;
			});

			ThisContainer.InitData<ProjectileHitData>((data) => {
				data.projectileLayer = initData.projectileLayer;
				data.blockingLayer = initData.blockingLayer;
				data.hitEffectLayer = initData.hitEffectLayer;
				data.hitUnitRelationType = initData.hitUnitRelationType;
			});
			ThisContainer.RemoveData<ProjectileMoveData>();
			if(targetValue.IsTracking)
			{
				ThisContainer.InitData<ProjectileMoveTrackingData>((data) => {
					data.startPosition = startPosition;
					data.reactionTargetValue = targetValue;
					data.moveSpeed = initData.moveSpeed;
				});
			}
			else
			{
				ThisContainer.InitData<ProjectileMoveTowardsData>((data) => {
					data.startPosition = startPosition;
					data.reactionTargetValue = targetValue;
					data.moveSpeed = initData.moveSpeed;
				});
			}

			ThisContainer.RemoveData<ProjectileInitData>();
			return true;
		}
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
		public void OnHit(Collision collision, IReactionTarget hitTarget)
		{
			SendHit(collision, hitTarget);
			ThisContainer.CallActionAllComponent<IProjectileLifecycle>(call => call.Hit(collision, hitTarget));
		}
		public void OnHitNoReactionTarget(Collision collision)
		{
			SendHitNoReaction(collision);
			ThisContainer.CallActionAllComponent<IProjectileLifecycle>(call => call.HitNoReactionTarget(collision));
		}
		public void OnHitBlocking(Collision something)
		{
			SendHitBlocking(something);
			ThisContainer.CallActionAllComponent<IProjectileLifecycle>(call => call.HitBlocking(something));
		}
		public async void OnDead()
		{
			int waitCount = 0;
			ThisContainer.CallActionAllComponent<IProjectileLifecycle>(async call => {
				waitCount++;
				await call.Dead();
				waitCount--;
			});
			await AwaitableUtility.WaitTrue(() => waitCount == 0, DestroyCancelToken);
			Dead();
		}
		protected abstract void Fire();
		protected abstract void MovementUpdate(in Vector3 velocity, in float deltaTime);
		protected abstract void SendHit(Collision collision, IReactionTarget hitTarget);
		protected abstract void SendHitNoReaction(Collision collision);
		protected abstract void SendHitBlocking(Collision something);
		protected abstract void Dead();
	}
}