using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

using Random = Unity.Mathematics.Random;

namespace BC.AISensor
{
	public class FilterOfRaycast : SensorFilter
	{
		public int hitCountMax;
		public LayerMask blockingLayerMask;

		protected override void Init(SensorProvider self)
		{

		}
		protected override void SensorUpdate(SensorProvider self, in List<Collider> colliderList, ref NativeSlice<SensorProvider.ColliderData> dataList)
		{
			NativeList<int> notPassIndex = new NativeList<int>(Allocator.TempJob);
			DoRaycast(ref dataList);
			DoDetailsRaycast(ref dataList);
			notPassIndex.Dispose();

			void DoRaycast(ref NativeSlice<SensorProvider.ColliderData> dataList)
			{
				int dataLength = dataList.Length;
				if(dataLength == 0) return;

				int rayLength = dataLength;
				for(int i = 0 ; i < dataLength ; i++)
				{
					var data = dataList[i];
					data.IsSafeFilter = false;
					dataList[i] = data;
				}

				NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(rayLength, Allocator.TempJob);
				NativeArray<RaycastHit> colliderHit = new NativeArray<RaycastHit>(dataLength, Allocator.TempJob);

				var commandJob = new CreateRaycastCommand(){
					dataList = dataList,
					blockLayerMask = blockingLayerMask.value,

					commands = commands,
				};
				int innerloopBatchCount = math.clamp(dataLength/8,8, 32);
				var commandHandle = commandJob.Schedule(dataLength, innerloopBatchCount, default);
				commandHandle.Complete();
				int minCommandsPerJob =  math.clamp(commands.Length/8,8,32);
				var raycastHandle = RaycastCommand.ScheduleBatch(commands, colliderHit, minCommandsPerJob, commandHandle);
				raycastHandle.Complete();

				int length = colliderHit.Length;
				for(int i = 0 ; i < length ; i++)
				{
					int dataIndex = i;
					var hit0 = colliderHit[i];

					if(hit0.collider == null)
					{
						var data = dataList[dataIndex];
						data.IsSafeFilter = true;
						dataList[dataIndex] = data;
					}
					else
					{
						notPassIndex.Add(i);
					}
				}

				commands.Dispose();
				colliderHit.Dispose();
			}

			void DoDetailsRaycast(ref NativeSlice<SensorProvider.ColliderData> dataList)
			{
				int dataLength = notPassIndex.Length;
				if(dataLength == 0) return;

				int detailsCast = 5;
				int rayLength = dataLength * detailsCast;
				NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(rayLength, Allocator.TempJob);
				NativeArray<RaycastHit> colliderHit = new NativeArray<RaycastHit>(dataLength, Allocator.TempJob);

				var commandJob = new CreateDetailsRaycastCommand(){
					dataList = dataList,
					notPassIndex = notPassIndex,
					blockLayerMask = blockingLayerMask.value,
					commands = commands,
					random =  new Random((uint)System.DateTime.UtcNow.Ticks),
				};
				int innerloopBatchCount = math.clamp(dataLength/8,8, 32);
				var commandHandle = commandJob.Schedule(dataLength, innerloopBatchCount, default);
				commandHandle.Complete();
				int minCommandsPerJob = math.clamp(commands.Length/8,8,32);
				var raycastHandle = RaycastCommand.ScheduleBatch(commands, colliderHit, minCommandsPerJob, commandHandle);
				raycastHandle.Complete();

				int length = colliderHit.Length;
				for(int i = 0 ; i < length ; i+=detailsCast)
				{
					int dataIndex = notPassIndex[i/detailsCast];
					var hit0 = colliderHit[i + 0];
					var hit1 = colliderHit[i + 1];
					var hit2 = colliderHit[i + 2];
					var hit3 = colliderHit[i + 3];

					if(hit0.colliderInstanceID == 0
						|| hit1.colliderInstanceID == 0
						|| hit2.colliderInstanceID == 0
						|| hit3.colliderInstanceID == 0)
					{
						var data = dataList[dataIndex];
						data.IsSafeFilter = true;
						dataList[dataIndex] = data;
					}
				}

				commands.Dispose();
				colliderHit.Dispose();
			}
		}
		protected override void Release(SensorProvider self)
		{
		}

		public override void OnDraw(Camera cam)
		{

		}
		[BurstCompile]
		private struct CreateRaycastCommand : IJobParallelFor
		{
			[ReadOnly]
			public NativeSlice<SensorProvider.ColliderData> dataList;
			[ReadOnly]
			public int blockLayerMask;

			[WriteOnly]
			public NativeArray<RaycastCommand> commands;
			public void Execute(int index)
			{
				SensorProvider.ColliderData data = dataList[index];

				float3 rayPoint = data.TransformPosition;
				float3 rayDir = data.Direction;
				float maxLength  = data.Distance;

				QueryParameters queryParameters = new QueryParameters(blockLayerMask, false, QueryTriggerInteraction.Ignore, false);

				commands[index] = new RaycastCommand(rayPoint, rayDir, queryParameters, maxLength);
			}
		}
		[BurstCompile]
		private struct CreateDetailsRaycastCommand : IJobParallelFor
		{
			[ReadOnly]
			public NativeSlice<SensorProvider.ColliderData> dataList;
			[ReadOnly]
			public NativeList<int> notPassIndex;
			[ReadOnly]
			public int blockLayerMask;
			[ReadOnly]
			public Random random;

			[WriteOnly]
			public NativeArray<RaycastCommand> commands;
			public void Execute(int index)
			{
				SensorProvider.ColliderData data = dataList[notPassIndex[index]];

				float3 rayPoint = data.TransformPosition;
				float3 rayDir = data.Direction;
				float maxLength  = data.Distance;

				QueryParameters queryParameters = new QueryParameters(blockLayerMask, false, QueryTriggerInteraction.Ignore, false);

				float3 subPoint0 = float3.zero;
				float3 subPoint1 = float3.zero;
				float3 subPoint2 = float3.zero;
				float3 subPoint3 = float3.zero;
				float3 centerPoint = data.CenterPoint;
				float3 toRight = math.normalize(math.cross(new float3(0, 1, 0), data.Direction));

				switch(data.ColliderType)
				{
					case SensorProvider.ColliderData.Type.SphereCollider:
					{
						float size = data.ColliderSize.x;
						subPoint0 = centerPoint + toRight * size * 1f;
						subPoint1 = centerPoint + toRight * size * random.NextFloat(0.1f, 0.9f);
						subPoint2 = centerPoint - toRight * size * random.NextFloat(0.1f, 0.9f);
						subPoint3 = centerPoint - toRight * size * 1f;
						break;
					}
					case SensorProvider.ColliderData.Type.BoxCollider:
					{
						float size = math.length(data.ColliderSize.xz) * 0.5f;
						subPoint0 = centerPoint + toRight * size * 1f;
						subPoint1 = centerPoint + toRight * size * random.NextFloat(0.1f, 0.9f);
						subPoint2 = centerPoint - toRight * size * random.NextFloat(0.1f, 0.9f);
						subPoint3 = centerPoint - toRight * size * 1f;
						break;
					}
					default:
					{
						subPoint0 = centerPoint + toRight * 0.25f;
						subPoint1 = centerPoint + toRight * 0.125f;
						subPoint2 = centerPoint - toRight * 0.125f;
						subPoint3 = centerPoint - toRight * 0.25f;
						break;
					}
				}

				commands[index * 5 + 0] = new RaycastCommand(rayPoint, subPoint0, queryParameters, maxLength);
				commands[index * 5 + 1] = new RaycastCommand(rayPoint, subPoint1, queryParameters, maxLength);
				commands[index * 5 + 2] = new RaycastCommand(rayPoint, subPoint2, queryParameters, maxLength);
				commands[index * 5 + 3] = new RaycastCommand(rayPoint, subPoint3, queryParameters, maxLength);
			}
		}
	}
}
