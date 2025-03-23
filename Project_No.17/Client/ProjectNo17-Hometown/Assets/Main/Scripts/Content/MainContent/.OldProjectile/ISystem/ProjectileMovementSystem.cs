namespace TFContent
{
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Mathematics;
	using Unity.Physics;
	using Unity.Transforms;

	[BurstCompile]
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(ProjectileSpawnSystem))]
	public partial struct ProjectileUpdateSystem : ISystem
	{
		[BurstCompile]
		private partial struct ProjectilesTrackingJob : IJobEntity
		{
			public EntityCommandBuffer.ParallelWriter Ecb;
			[ReadOnly] public ComponentLookup<ProjectileAlive> AliveLookup;
			[ReadOnly] public ComponentLookup<TrackingPosition> TargetPositionLookup;

			private unsafe void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in LocalToWorld transform, in ProjectileTargetPosition tracking, ref PhysicsVelocity velocity)
			{
				if(!AliveLookup.HasComponent(entity))
				{
					return;
				}

				if(tracking.TrackingEntity == Entity.Null || !TargetPositionLookup.HasComponent(tracking.TrackingEntity)) return;

				float3 trackingPosition = TargetPositionLookup[tracking.TrackingEntity].Position;
				trackingPosition += tracking.TargetPositionOffset;
				float3 projectilePosition = transform.Position;

				float3 newDirection = math.normalize(trackingPosition - projectilePosition);
				if(float.IsNaN(newDirection.x) || float.IsNaN(newDirection.y) || float.IsNaN(newDirection.z)) return;
				float3 oldDirection = math.normalize(velocity.Linear);
				if(float.IsNaN(oldDirection.x) || float.IsNaN(oldDirection.y) || float.IsNaN(oldDirection.z)) return;

				var dotProduct = math.dot(newDirection, oldDirection);
				var angle = math.acos(dotProduct) * math.TODEGREES;
				if(angle > 45 || float.IsNaN(angle)) return;
				float speed = math.length(velocity.Linear);

				velocity.Linear = newDirection * speed;

				Ecb.SetComponent(chunkIndex, entity, velocity);
			}
		}

		private EntityQuery projectileTrackingQuery;
		public void OnCreate(ref SystemState state)
		{
			projectileTrackingQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld, ProjectileTargetPosition, PhysicsVelocity>().Build();

			state.RequireForUpdate(projectileTrackingQuery);
		}

		public void OnUpdate(ref SystemState state)
		{
			var Manager = ProjectileManager.Manager;
			if(Manager == null || !Manager.enabled) return;

			var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
			var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

			var job1 = new ProjectilesTrackingJob
			{
				Ecb = ecb,
				TargetPositionLookup = SystemAPI.GetComponentLookup<TrackingPosition>(true),
				AliveLookup = SystemAPI.GetComponentLookup<ProjectileAlive>(true),
			};
			state.Dependency = job1.ScheduleParallel(projectileTrackingQuery, state.Dependency);
			state.Dependency.Complete();
		}
	}
}