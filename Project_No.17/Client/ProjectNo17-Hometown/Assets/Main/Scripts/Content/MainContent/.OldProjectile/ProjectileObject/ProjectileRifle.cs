using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace TFContent
{
	public class ProjectileRifle : ProjectileDataBaker
	{
		public override void OnEntityDataBake(EntityManager entityManager, Entity entity)
		{
			HasAndAdd<ProjectileAlive>();
			HasAndAdd<ProjectileLifeTime>();
			HasAndAdd<ProjectileLifeHit>();
			HasAndAdd<ProjectileTargetPosition>();
			HasAndAdd<LocalTransform>();
			HasAndAdd<PhysicsVelocity>();

			void HasAndAdd<T>() where T : unmanaged, IComponentData
			{
				if(entityManager.HasComponent<T>(entity)) return;
				entityManager.AddComponentData<T>(entity, new T());
			}
		}
	}
}
