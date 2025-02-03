using System;
using System.Collections.Generic;

using BC.Shape;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace BC.AISensor
{
	[Obsolete]
	public class VisionCollector : SensorCollector
	{
		[Header("Ray Option")]
		public SphereShape SphereData;
		public LayerMask blockingLayerMask;
		public float rayLength;
		public int firstStepRayCount;

		[Header("Collect Option")]
		public LayerMask collectLayerMask;

		protected override void Init(SensorProvider self)
		{
			Debug.LogError("미완성 코드를 사용중");
		}

		protected override void SensorUpdate(SensorProvider self, ref HashSet<Collider> list)
		{
			if(firstStepRayCount == 0) return;

			Vector3 rayPoint = self.transform.TransformPoint(SphereData.Center);

			NativeArray<RaycastCommand> stap1_Command = new NativeArray<RaycastCommand>(firstStepRayCount, Allocator.TempJob);
			var stap1_Job = new Step1_Raycasting(){
				rayStartPoint =  rayPoint,
				queryParameters = new QueryParameters(blockingLayerMask, false, QueryTriggerInteraction.Ignore, false),
				rayLength = rayLength,
				rayAnglePerIndex = 360f/(float)firstStepRayCount,
				commands = stap1_Command,
			};
			int innerloopBatchCount = math.clamp(firstStepRayCount/8,8,32);
			JobHandle stap1_Handle = stap1_Job.Schedule(firstStepRayCount, innerloopBatchCount, default);

			NativeArray<RaycastHit> stap1_raycastHits = new NativeArray<RaycastHit>(stap1_Command.Length, Allocator.TempJob);
			int minCommandsPerJob = math.clamp(firstStepRayCount/8,8,32);
			JobHandle stap1_raycastJob = RaycastCommand.ScheduleBatch(stap1_Command, stap1_raycastHits, minCommandsPerJob,stap1_Handle);

			int length = stap1_raycastHits.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var stap1_hit = stap1_raycastHits[i];
			}





			stap1_raycastHits.Dispose();
			stap1_Command.Dispose();
		}

		protected override void Release(SensorProvider self)
		{
		}


		[BurstCompile]
		public struct Step1_Raycasting : IJobParallelFor
		{
			[ReadOnly]
			public float rayLength;
			[ReadOnly]
			public float3 rayStartPoint;
			[ReadOnly]
			public QueryParameters queryParameters;
			[ReadOnly]
			public float rayAnglePerIndex;

			[WriteOnly]
			public NativeArray<RaycastCommand> commands;
			public void Execute(int index)
			{
				Vector3 diraction = Quaternion.Euler(0f, rayAnglePerIndex * index, 0f) * Vector3.forward;
				commands[index] = new RaycastCommand(rayStartPoint, diraction, queryParameters, rayLength);
			}
		}
	}
}
