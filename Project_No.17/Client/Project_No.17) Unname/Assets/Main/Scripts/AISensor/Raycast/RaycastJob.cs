using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace BC.AISensor
{
	[BurstCompile]
	public partial struct RaycastJob : IJobEntity
	{
		[ReadOnly] public PhysicsWorld PhysicsWorld;
		public EntityCommandBuffer.ParallelWriter CommandBuffer;

		void Execute(Entity entity, [EntityIndexInQuery] int index, ref RaycastRequest raycastData)
		{
			RaycastInput raycastInput = new RaycastInput
			{
				Start = raycastData.StartPosition,
				End = raycastData.EndPosition,
				Filter = raycastData.Filter
			};

			if(PhysicsWorld.CastRay(raycastInput, out RaycastHit hit))
			{
				CommandBuffer.AddComponent(index, entity, new RaycastResponse {
					IsHit = true,
					Origin = raycastData.StartPosition,
					HitPoint = hit.Position,
					SurfaceNormal = hit.SurfaceNormal,
					HitEntity = PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity
				});
			}
			else
			{
				CommandBuffer.AddComponent(index, entity, new RaycastResponse {
					IsHit = false,
					Origin = raycastData.StartPosition,
					HitPoint = float3.zero,
					SurfaceNormal = float3.zero,
					HitEntity = Entity.Null
				});
			}

			CommandBuffer.RemoveComponent<RaycastRequest>(index, entity);
		}
	}
}
