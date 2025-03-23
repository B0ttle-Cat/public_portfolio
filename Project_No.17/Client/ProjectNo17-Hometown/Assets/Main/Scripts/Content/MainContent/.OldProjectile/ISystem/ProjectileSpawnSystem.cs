namespace TFContent
{
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Mathematics;
	using Unity.Physics;
	using Unity.Physics.Systems;
	using Unity.Transforms;

	using Random = Unity.Mathematics.Random;

	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(PhysicsSimulationGroup))]
	public partial struct ProjectileSpawnSystem : ISystem
	{
		private EntityQuery projectileInstantiateQuery;

		public void OnCreate(ref SystemState state)
		{
			projectileInstantiateQuery = SystemAPI.QueryBuilder()
				.WithAll<ProjectileInstantiateData>()
				.Build();

			state.RequireForUpdate(projectileInstantiateQuery);
		}

		public void OnUpdate(ref SystemState state)
		{
			var entityManager = state.EntityManager;
			var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
			var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

			var random = Random.CreateFromIndex((uint)state.GlobalSystemVersion);

			using(var entities = projectileInstantiateQuery.ToEntityArray(Allocator.Temp))
			{
				foreach(var entity in entities)
				{
					if(!entityManager.HasComponent<ProjectileInstantiateData>(entity)) continue;
					var instantiateData = entityManager.GetComponentData<ProjectileInstantiateData>(entity);

					Entity projectileEntity = entityManager.Instantiate(instantiateData.entityPrefab);
					Entity trackingEntity = instantiateData.trackingEntity;

					var startPosition = instantiateData.startPosition;
					var targetPosition = instantiateData.targetPosition;
					var targetPositionOffset = instantiateData.targetPositionOffset;

					var belongsTo = instantiateData.belongsTo;
					var collidesWith = instantiateData.collidesWith;

					// ✅ 탄환 생명 관리 컴포넌트 추가
					entityManager.SetComponentData(projectileEntity, new ProjectileAlive {
						TypeName = instantiateData.projectileType,
						IsAlive = true,
					});
					entityManager.SetComponentData(projectileEntity, new ProjectileLifeTime {
						LifeTime = instantiateData.maxLifeTime * random.NextFloat(0.5f, 1.1f),
					});
					entityManager.SetComponentData(projectileEntity, new ProjectileLifeHit {
						HitCount = instantiateData.maxHitCount,
					});

					// ✅ 추적 대상이 있을 경우 적용
					if(trackingEntity != Entity.Null && entityManager.HasComponent<TrackingPosition>(trackingEntity))
					{
						var trackingPosition = entityManager.GetComponentData<TrackingPosition>(trackingEntity).Position;
						targetPosition = trackingPosition;
					}
					targetPosition += targetPositionOffset;

					// ✅ 목표 위치 정보 추가
					entityManager.SetComponentData(projectileEntity, new ProjectileTargetPosition {
						StartPosition = startPosition,
						TargetPosition = targetPosition,
						TrackingEntity = trackingEntity,
						TargetPositionOffset = targetPositionOffset,
					});

					// ✅ 탄환 회전값 계산
					quaternion lookRotation = quaternion.LookRotation(math.normalize(targetPosition - startPosition), math.up());

					// ✅ 위치 및 물리 속성 적용
					entityManager.SetComponentData(projectileEntity, new LocalTransform {
						Position = startPosition,
						Rotation = lookRotation,
						Scale = 1f
					});

					entityManager.SetComponentData(projectileEntity, new PhysicsVelocity {
						Linear = math.mul(lookRotation, new float3(0, 0, instantiateData.speed * random.NextFloat(0.5f, 1f))), // 정면 방향으로 발사
						Angular = float3.zero
					});

					PhysicsCollider physicsCollider = entityManager.GetComponentData<PhysicsCollider>(projectileEntity);
					CollisionFilter collisionFilter = physicsCollider.Value.Value.GetCollisionFilter();
					collisionFilter.BelongsTo = belongsTo;
					collisionFilter.CollidesWith = collidesWith;
					physicsCollider.Value.Value.SetCollisionFilter(collisionFilter);
					entityManager.SetComponentData(projectileEntity, physicsCollider);

					// ✅ 기존 엔티티 삭제
					entityManager.DestroyEntity(entity);
				}
			}
		}

		public void OnDestroy(ref SystemState state)
		{
			ProjectilePoolManager.ClearAllPools(state.EntityManager);
		}
	}
}
