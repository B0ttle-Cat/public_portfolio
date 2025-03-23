namespace TFContent
{
	using Unity.Collections;
	using Unity.Entities;

	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(PhysicsTriggerSystem))]
	public partial struct ProjectileRemoveSystem : ISystem
	{
		private EntityQuery projectileQueryAlive;

		public void OnCreate(ref SystemState state)
		{
			projectileQueryAlive = SystemAPI.QueryBuilder().WithAll<ProjectileAlive, ProjectileLifeTime, ProjectileLifeHit>().Build();

			state.RequireForUpdate(projectileQueryAlive);
		}

		public void OnUpdate(ref SystemState state)
		{
			var entityManager = state.EntityManager;

			float DeltaTime = SystemAPI.Time.fixedDeltaTime;
			using(var entities = projectileQueryAlive.ToEntityArray(Allocator.Temp))
			{
				foreach(var entity in entities)
				{
					var projectileAlive = entityManager.GetComponentData<ProjectileAlive>(entity);
					var lifeTime = entityManager.GetComponentData<ProjectileLifeTime>(entity);
					var lifeHit = entityManager.GetComponentData<ProjectileLifeHit>(entity);

					if(lifeTime.LifeTime > 0f)
					{
						lifeTime.LifeTime -= DeltaTime;
						entityManager.SetComponentData(entity, lifeTime);
					}

					if(projectileAlive.IsAlive && (lifeTime.LifeTime <= 0f || lifeHit.HitCount <= 0))
					{
						projectileAlive.IsAlive = false;
						entityManager.SetComponentData(entity, projectileAlive);

						entityManager.DestroyEntity(entity);
					}
				}
			}

		}
	}
}
