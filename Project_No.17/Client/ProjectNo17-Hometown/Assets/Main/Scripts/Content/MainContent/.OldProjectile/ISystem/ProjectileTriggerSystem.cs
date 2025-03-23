namespace TFContent
{
	using System;

	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Physics;

	using UnityEngine;

	[BurstCompile]
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(ProjectileUpdateSystem))]
	public partial struct PhysicsTriggerSystem : ISystem
	{
		// ✅ HashSet을 위한 Key 구조체
		private struct ProjectileTriggerResult : IEquatable<ProjectileTriggerResult>
		{
			public Entity Projectile;
			public Entity Hit;
			public TriggerState State;
			public enum TriggerState
			{
				Enter, Stay, Exit
			}


			public ProjectileTriggerResult(Entity projectile, Entity hit, TriggerState state)
			{
				Projectile = projectile;
				Hit = hit;
				State = state;
			}

			public bool Equals(ProjectileTriggerResult other)
			{
				return Projectile.Equals(other.Projectile) && Hit.Equals(other.Hit);
			}

			public override int GetHashCode()
			{
				return Projectile.GetHashCode() ^ Hit.GetHashCode();
			}
		}

		[BurstCompile]
		private partial struct TriggerEventJob : ITriggerEventsJob
		{
			public EntityCommandBuffer.ParallelWriter Ecb;
			public ComponentLookup<ProjectileLifeHit> ProjectileLookup;
			//public ComponentLookup<ProjectileTriggerState> TriggerStateLookup;
			public NativeList<ProjectileTriggerResult> ResultList;

			public void Execute(TriggerEvent triggerEvent)
			{
				Entity entityA = triggerEvent.EntityA;
				Entity entityB = triggerEvent.EntityB;
				bool isProjectileA = ProjectileLookup.HasComponent(entityA);
				bool isProjectileB = ProjectileLookup.HasComponent(entityB);

				if(isProjectileA != isProjectileB && isProjectileA || isProjectileB)
				{
					Entity projectileEntity = isProjectileA ? entityA : entityB;
					Entity targetEntity = isProjectileA ? entityB : entityA;

					if(!ProjectileLookup.TryGetComponent(projectileEntity, out ProjectileLifeHit projectileLifeHit))
					{
						Debug.Log("No ProjectileLookup");
						return;
					}
					ProjectileTriggerResult triggerResult = new ProjectileTriggerResult(projectileEntity, targetEntity, ProjectileTriggerResult.TriggerState.Enter);

					int length = ResultList.Length;
					for(int i = 0 ; i < length ; i++)
					{
						var oldResult = ResultList[i];
						if(oldResult.Equals(triggerResult))
						{
							oldResult.State = ProjectileTriggerResult.TriggerState.Stay;
							ResultList[i] = oldResult;
							return;
						}
					}
					if(projectileLifeHit.HitCount > 0)
					{
						// ✅ `HitCount == 0`이면 총알 삭제
						projectileLifeHit.HitCount--;

						// ✅ Enter 이벤트 전달
						ResultList.Add(triggerResult);
						Ecb.SetComponent(projectileEntity.Index, projectileEntity, projectileLifeHit);
					}
				}
				else
				{
					Debug.Log("No Hit");
				}
			}
		}

		private NativeList<ProjectileTriggerResult> oldTriggerResultList;

		public void OnCreate(ref SystemState state)
		{
			oldTriggerResultList = new NativeList<ProjectileTriggerResult>(Allocator.Persistent);
		}

		public void OnUpdate(ref SystemState state)
		{
			var Manager = ProjectileManager.Manager;
			if(Manager == null || !Manager.enabled) return;

			var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
			var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

			int count = oldTriggerResultList.Length;
			for(int i = 0 ; i < count ; i++)
			{
				var result = oldTriggerResultList[i];
				result.State = ProjectileTriggerResult.TriggerState.Exit;
				oldTriggerResultList[i] = result;
			}

			var job1 = new TriggerEventJob
			{
				Ecb = ecb,
				ProjectileLookup = SystemAPI.GetComponentLookup<ProjectileLifeHit>(),
				ResultList = oldTriggerResultList,
			};
			var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
			state.Dependency = job1.Schedule(simulation, state.Dependency);
			state.Dependency.Complete();

			// ✅ `ProjectileManager`로 충돌 이벤트 전달
			int length = oldTriggerResultList.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var triggerResult = oldTriggerResultList[i];
				if(triggerResult.State == ProjectileTriggerResult.TriggerState.Stay) continue;
				bool isEnter = triggerResult.State == ProjectileTriggerResult.TriggerState.Enter;

				ProjectileManager.Manager?.OnProjectileImpact(new ProjectileImpactEvent() {
					ProjectileEntity = triggerResult.Projectile,
					HitEntity = triggerResult.Hit,
					IsEnter = isEnter,
				});

				if(!isEnter)
				{
					oldTriggerResultList.RemoveAt(i--);
					length = oldTriggerResultList.Length;
				}
			}
		}

		public void OnDestroy(ref SystemState state)
		{
			oldTriggerResultList.Dispose();
		}
	}

}