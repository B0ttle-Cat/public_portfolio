using Unity.Collections;
using Unity.Entities;

namespace TFContent
{

	public struct ProjectilePoolComponent : IComponentData
	{
		public Entity PrefabEntity; // 프리팹 엔티티
		public int PoolSize; // 풀의 크기
		public FixedString64Bytes ProjectileType; // 탄환 유형 
	}

	[InternalBufferCapacity(64)] // 기본적으로 64개까지 내부에서 관리
	public struct ProjectilePoolBuffer : IBufferElementData
	{
		public Entity PooledEntity;
	}

	public static class ProjectilePoolManager
	{
		public static bool IsValidPrefab(EntityManager entityManager, Entity prefabEntity)
		{
			if(prefabEntity == Entity.Null || !entityManager.Exists(prefabEntity))
				return false;

			// ✅ prefabEntity가 실제로 프리팹인지 확인 (EntityManager의 Prefab 체크)
			if(entityManager.HasComponent<Prefab>(prefabEntity))
			{
				return true;
			}
			return false;
		}


		public static Entity FindOrCreatePool(EntityManager entityManager, Entity prefabEntity, FixedString64Bytes projectileType)
		{
			var poolQuery = entityManager.CreateEntityQuery(typeof(ProjectilePoolComponent));

			using(var poolEntities = poolQuery.ToEntityArray(Allocator.Temp))
			{
				foreach(var poolEntity in poolEntities)
				{
					var poolComponent = entityManager.GetComponentData<ProjectilePoolComponent>(poolEntity);
					if(poolComponent.ProjectileType.Equals(projectileType))
					{
						return poolEntity; // ✅ 일치하는 풀 찾기
					}
				}
			}

			if(!IsValidPrefab(entityManager, prefabEntity))
			{
				return Entity.Null;
			}
			return CreatePool(entityManager, prefabEntity, projectileType, 24);
		}

		public static Entity CreatePool(EntityManager entityManager, Entity prefabEntity, FixedString64Bytes projectileType, int poolSize)
		{
			if(!IsValidPrefab(entityManager, prefabEntity))
			{
				return Entity.Null;
			}
			Entity poolEntity = entityManager.CreateEntity();
			entityManager.AddComponentData(poolEntity, new ProjectilePoolComponent {
				PrefabEntity = prefabEntity,
				PoolSize = poolSize,
				ProjectileType = projectileType
			});

			DynamicBuffer<ProjectilePoolBuffer> buffer = entityManager.AddBuffer<ProjectilePoolBuffer>(poolEntity);

			for(int i = 0 ; i < poolSize ; i++)
			{
				Entity newProjectile = entityManager.Instantiate(prefabEntity);
				entityManager.SetEnabled(newProjectile, false); // ✅ 풀에 추가할 때 비활성화
				buffer.Add(new ProjectilePoolBuffer { PooledEntity = newProjectile });
			}

			//Debug.Log($"[ProjectilePoolManager] Created pool for {projectileType} with {poolSize} entities.");
			return poolEntity;
		}

		public static Entity GetProjectile(EntityManager entityManager, Entity prefabEntity, FixedString64Bytes projectileType)
		{
			Entity poolEntity = FindOrCreatePool(entityManager, prefabEntity, projectileType);
			if(poolEntity == Entity.Null) return Entity.Null;

			var buffer = entityManager.GetBuffer<ProjectilePoolBuffer>(poolEntity);

			if(buffer.Length > 0)
			{
				Entity projectile = buffer[buffer.Length - 1].PooledEntity;
				buffer.RemoveAt(buffer.Length - 1);

				var ecb = new EntityCommandBuffer(Allocator.Temp);
				ecb.SetEnabled(projectile, true); // ✅ 풀에서 꺼낼 때 활성화
				ecb.Playback(entityManager);
				ecb.Dispose();

				return projectile;
			}

			var poolComponent = entityManager.GetComponentData<ProjectilePoolComponent>(poolEntity);
			return entityManager.Instantiate(poolComponent.PrefabEntity);
		}

		public static void ReturnProjectile(EntityManager entityManager, Entity projectileEntity, FixedString64Bytes projectileType)
		{
			Entity poolEntity = FindOrCreatePool(entityManager, projectileEntity, projectileType);
			if(poolEntity == Entity.Null) return;

			var buffer = entityManager.GetBuffer<ProjectilePoolBuffer>(poolEntity);

			var ecb = new EntityCommandBuffer(Allocator.Temp);
			ecb.SetEnabled(projectileEntity, value: false); // ✅ 풀에 반환할 때 비활성화
			ecb.Playback(entityManager);
			ecb.Dispose();

			buffer.Add(new ProjectilePoolBuffer { PooledEntity = projectileEntity });
		}

		public static void ClearAllPools(EntityManager entityManager)
		{
			var poolQuery = entityManager.CreateEntityQuery(typeof(ProjectilePoolComponent));

			var ecb = new EntityCommandBuffer(Allocator.TempJob);

			using(var poolEntities = poolQuery.ToEntityArray(Allocator.Temp))
			{
				foreach(var poolEntity in poolEntities)
				{
					if(!entityManager.Exists(poolEntity)) continue;

					if(entityManager.HasBuffer<ProjectilePoolBuffer>(poolEntity))
					{
						var buffer = entityManager.GetBuffer<ProjectilePoolBuffer>(poolEntity);

						foreach(var pooledEntity in buffer)
						{
							if(entityManager.Exists(pooledEntity.PooledEntity))
							{
								ecb.DestroyEntity(pooledEntity.PooledEntity); // ✅ ECB 사용
							}
						}
					}

					ecb.DestroyEntity(poolEntity);
				}
			}

			ecb.Playback(entityManager);
			ecb.Dispose();

			//	Debug.Log("[ProjectilePoolManager] All projectile pools cleared.");
		}
	}
}
