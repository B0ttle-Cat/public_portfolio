//using System.Collections.Generic;

//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Mathematics;

//using UnityEngine;

//using Random = Unity.Mathematics.Random;

//namespace BC.AIProvider
//{
//	public class FilterOfRaycast : SensorFilter
//	{
//		public int hitCountMax;
//		public LayerMask blockingLayerMask;

//		protected override void Init(SensorProvider self)
//		{

//		}
//		protected override void SensorUpdate(SensorProvider self, in List<Collider> colliderList, ref NativeSlice<SensorProvider.SensoringData> dataList)
//		{
//			NativeList<int> notPassIndex = new NativeList<int>(Allocator.TempJob);
//			DoRaycast(ref dataList);
//			//DoDetailsRaycast(ref dataList);
//			notPassIndex.Dispose();

//			void DoRaycast(ref NativeSlice<SensorProvider.SensoringData> dataList)
//			{
//				//int dataLength = dataList.Length;
//				//if(dataLength == 0) return;

//				//int rayLength = dataLength;
//				//for(int i = 0 ; i < dataLength ; i++)
//				//{
//				//	var data = dataList[i];
//				//	data.IsSafeFilter = false;
//				//	dataList[i] = data;
//				//}
//				QueryParameters? queryParameters = null;
//				int length = notPassIndex.Length;
//				for(int i = 0 ; i < length ; i++)
//				{
//					int dataIndex = notPassIndex[i];
//					var data = dataList[dataIndex];
//					if(data.IsSafeFilter == false) continue;

//					float3 rayPoint = data.TransformPosition;
//					float3 rayDir = data.Direction;
//					float maxLength  = data.Distance;

//					if(Physics.Raycast(rayPoint, rayDir, out var rayhit, maxLength, blockingLayerMask.value))
//					{
//						data.IsSafeFilter = true;

//						float3 left2Right = data.ClosestPointRight - data.ClosestPointLeft;
//						float left2RightLength = math.length(left2Right);
//						int rayCount = (int)math.round(left2RightLength / 0.5f);
//						float rayDistanceStep = left2RightLength / rayCount;
//						QueryParameters _queryParameters =  queryParameters ??= new QueryParameters(blockingLayerMask.value);
//						NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(rayCount, Allocator.Temp);
//						NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(rayCount, Allocator.Temp);
//						for(int ii = 0 ; ii < rayCount ; ii++)
//						{
//							float3 rayStepTarget = data.ClosestPointRight + math.normalize(left2Right) * rayDistanceStep * ii;
//							rayDir = rayStepTarget - rayPoint;
//							commands[i] = new RaycastCommand(rayPoint, math.normalize(rayDir), _queryParameters, math.length(rayDir));
//						}
//						var raycastHandle = RaycastCommand.ScheduleBatch(commands, results, 10, default);
//						raycastHandle.Complete();

//						rayCount = results.Length;
//						for(int ii = 0 ; ii < length ; ii++)
//						{
//							if(results[i].colliderInstanceID == 0)
//							{
//								// 이런게 하나라도 있으면 시야를 가로막는 물체가 없다는 뜻.
//								data.IsSafeFilter = false;
//								break;
//							}
//						}
//						commands.Dispose();
//						results.Dispose();
//					}
//					dataList[dataIndex] = data;
//				}
//			}

//			//void DoDetailsRaycast(ref NativeSlice<SensorProvider.ColliderData> dataList)
//			//{
//			//	int dataLength = notPassIndex.Length;
//			//	if(dataLength == 0) return;

//			//	int detailsCast = 5;
//			//	int rayLength = dataLength * detailsCast;
//			//	NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(rayLength, Allocator.TempJob);
//			//	NativeArray<RaycastHit> colliderHit = new NativeArray<RaycastHit>(dataLength, Allocator.TempJob);

//			//	var commandJob = new CreateDetailsRaycastCommand(){
//			//		dataList = dataList,
//			//		notPassIndex = notPassIndex,
//			//		queryParameters = new QueryParameters(blockingLayerMask.value, false, QueryTriggerInteraction.Ignore, false),
//			//		commands = commands,
//			//		random =  new Random((uint)System.DateTime.UtcNow.Ticks),
//			//		dataLength = dataLength,
//			//	};

//			//	var commandHandle = commandJob.Schedule();
//			//	commandHandle.Complete();
//			//	var raycastHandle = RaycastCommand.ScheduleBatch(commands, colliderHit, 10, commandHandle);
//			//	raycastHandle.Complete();

//			//	int length = colliderHit.Length;
//			//	for(int i = 0 ; i < length ; i+=detailsCast)
//			//	{
//			//		int dataIndex = notPassIndex[i/detailsCast];
//			//		var hit0 = colliderHit[i + 0];
//			//		var hit1 = colliderHit[i + 1];
//			//		var hit2 = colliderHit[i + 2];
//			//		var hit3 = colliderHit[i + 3];

//			//		if(hit0.colliderInstanceID == 0
//			//			|| hit1.colliderInstanceID == 0
//			//			|| hit2.colliderInstanceID == 0
//			//			|| hit3.colliderInstanceID == 0)
//			//		{
//			//			var data = dataList[dataIndex];
//			//			data.IsSafeFilter = true;
//			//			dataList[dataIndex] = data;
//			//		}
//			//	}

//			//	commands.Dispose();
//			//	colliderHit.Dispose();
//			//}
//		}
//		protected override void Release(SensorProvider self)
//		{
//		}

//		public override void OnDraw(Camera cam)
//		{

//		}
//		private struct CreateRaycastCommand : IJobParallelFor
//		{
//			[ReadOnly]
//			public NativeSlice<SensorProvider.SensoringData> dataList;
//			[ReadOnly]
//			public int blockLayerMask;

//			[WriteOnly]
//			public NativeArray<RaycastCommand> commands;
//			public void Execute(int index)
//			{
//				SensorProvider.SensoringData data = dataList[index];

//				float3 rayPoint = data.TransformPosition;
//				float3 rayDir = data.Direction;
//				float maxLength  = data.Distance;

//				QueryParameters queryParameters = new QueryParameters(blockLayerMask, false, QueryTriggerInteraction.Ignore, false);

//				commands[index] = new RaycastCommand(rayPoint, rayDir, queryParameters, maxLength);
//			}
//		}
//		private struct CreateDetailsRaycastCommand : IJob
//		{
//			[ReadOnly]
//			public NativeSlice<SensorProvider.SensoringData> dataList;
//			[ReadOnly]
//			public NativeList<int> notPassIndex;
//			[ReadOnly]
//			public QueryParameters queryParameters;
//			[ReadOnly]
//			public Random random;
//			[WriteOnly]
//			public NativeArray<RaycastCommand> commands;
//			[ReadOnly]
//			public int dataLength;
//			public void Execute()
//			{
//				for(int i = 0 ; i < dataLength ; i++)
//				{
//					SensorProvider.SensoringData data = dataList[notPassIndex[i]];

//					float3 rayPoint = data.TransformPosition;
//					float3 rayDir = data.Direction;
//					float maxLength  = data.Distance;

//					float3 subPoint0 = float3.zero;
//					float3 subPoint1 = float3.zero;
//					float3 subPoint2 = float3.zero;
//					float3 subPoint3 = float3.zero;
//					float3 centerPoint = data.CenterPoint;
//					float3 toRight = math.normalize(math.cross(new float3(0, 1, 0), data.Direction));

//					switch(data.ColliderType)
//					{
//						case SensorProvider.SensoringData.Type.SphereCollider:
//						{
//							float size = data.ColliderSize.x;
//							subPoint0 = centerPoint + toRight * size * 1f;
//							subPoint1 = centerPoint + toRight * size * random.NextFloat(0.1f, 0.9f);
//							subPoint2 = centerPoint - toRight * size * random.NextFloat(0.1f, 0.9f);
//							subPoint3 = centerPoint - toRight * size * 1f;
//							break;
//						}
//						case SensorProvider.SensoringData.Type.BoxCollider:
//						{
//							float size = math.length(data.ColliderSize.xz) * 0.5f;
//							subPoint0 = centerPoint + toRight * size * 1f;
//							subPoint1 = centerPoint + toRight * size * random.NextFloat(0.1f, 0.9f);
//							subPoint2 = centerPoint - toRight * size * random.NextFloat(0.1f, 0.9f);
//							subPoint3 = centerPoint - toRight * size * 1f;
//							break;
//						}
//						default:
//						{
//							subPoint0 = centerPoint + toRight * 0.25f;
//							subPoint1 = centerPoint + toRight * 0.125f;
//							subPoint2 = centerPoint - toRight * 0.125f;
//							subPoint3 = centerPoint - toRight * 0.25f;
//							break;
//						}
//					}

//					subPoint0 = centerPoint + toRight * 0.01f;
//					subPoint1 = centerPoint + toRight * 0.02f;
//					subPoint2 = centerPoint + toRight * 0.03f;
//					subPoint3 = centerPoint + toRight * 0.04f;

//					commands[i*5+0] = new RaycastCommand(rayPoint, subPoint0, queryParameters, maxLength);
//					commands[i*5+1] = new RaycastCommand(rayPoint, subPoint1, queryParameters, maxLength);
//					commands[i*5+2] = new RaycastCommand(rayPoint, subPoint2, queryParameters, maxLength);
//					commands[i*5+3] = new RaycastCommand(rayPoint, subPoint3, queryParameters, maxLength);
//					commands[i*5+4] = new RaycastCommand(rayPoint, centerPoint, queryParameters, maxLength);
//				}
//			}
//		}
//	}
//}
