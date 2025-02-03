using Unity.Entities;
using Unity.Physics;

namespace BC.AISensor
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public partial class RaycastSystem : SystemBase
	{
		private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

		protected override void OnCreate()
		{
			_endSimulationEcbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			var commandBuffer = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
			var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

			// RaycastJob 실행
			var raycastJob = new RaycastJob
			{
				PhysicsWorld = physicsWorld,
				CommandBuffer = commandBuffer
			};

			Dependency = raycastJob.ScheduleParallel(Dependency);
			_endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}
