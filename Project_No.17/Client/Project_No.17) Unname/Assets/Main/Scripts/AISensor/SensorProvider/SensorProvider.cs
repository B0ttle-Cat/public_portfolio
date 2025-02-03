using System.Collections.Generic;
using System.Linq;

using BC.Base;
using BC.ODCC;
using BC.Shape;

using Shapes;

using Sirenix.OdinInspector;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

using UnityEngine;

using Collider = UnityEngine.Collider;

namespace BC.AISensor
{
	public class SensorProvider : ComponentBehaviour, IOdccUpdate.Fixed, IShapeRenderer
	{
		[Title("Collector")]
		[SerializeReference]
		public SensorCollector[] sensorCollectors = new SensorCollector[0];
		[Title("Array")]
		[SerializeReference]
		public SensorArray[] sensorArrays = new SensorArray[0];
		[Title("Filter")]
		[SerializeReference]
		public SensorFilter[] sensorFilters = new SensorFilter[0];
		[Title("Result")]
		public List<Collider> sensorResult;

		public PhysicsWorld physicsWorld { get; private set; }
		public CollisionWorld collisionWorld { get; private set; }

		void IOdccUpdate.Fixed.BaseFixedUpdate()
		{
			SensorUpdate();
		}

		protected override void BaseStart()
		{
			World world = World.DefaultGameObjectInjectionWorld;
			var entityManager = world.EntityManager;
			EntityQuery query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PhysicsWorldSingleton>());
			PhysicsWorldSingleton physicsWorldSingleton = query.GetSingleton<PhysicsWorldSingleton>();
			physicsWorld = physicsWorldSingleton.PhysicsWorld;
			collisionWorld = physicsWorld.CollisionWorld;

			sensorResult = new List<Collider>();
		}

		public void SensorUpdate()
		{
			if(!isActiveAndEnabled) return;
			if(sensorResult == null) return;

			World world = World.DefaultGameObjectInjectionWorld;
			var entityManager = world.EntityManager;
			EntityQuery query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PhysicsWorldSingleton>());
			PhysicsWorldSingleton physicsWorldSingleton = query.GetSingleton<PhysicsWorldSingleton>();
			physicsWorld = physicsWorldSingleton.PhysicsWorld;
			collisionWorld = physicsWorld.CollisionWorld;

			HashSet<Collider> detectedList = new HashSet<Collider>();
			{
				int length = sensorCollectors == null ? 0 : sensorCollectors.Length;
				for(int i = 0 ; i < length ; i++)
				{
					var sensorModule = sensorCollectors[i];
					sensorModule.index = i;
					sensorModule.OnSensorUpdate(this, ref detectedList);
				}
			}
			if(detectedList.Count > 0)
			{
				List<Collider> colliderList = detectedList.ToList();
				NativeArray<ColliderData> colliderDataList = RunCalculateColliderJob(ThisTransform, in colliderList);
				NativeSlice<ColliderData> dataList = new NativeSlice<ColliderData>(colliderDataList);

				int length = sensorArrays == null ? 0 : sensorArrays.Length;
				for(int i = 0 ; i < length ; i++)
				{
					var sensorModule = sensorArrays[i];
					sensorModule.index = i;
					sensorModule.OnSensorUpdate(this, ref dataList);
				}

				length = sensorFilters == null ? 0 : sensorFilters.Length;
				for(int i = 0 ; i < length ; i++)
				{
					var sensorModule = sensorFilters[i];
					sensorModule.index = i;
					sensorModule.OnSensorUpdate(this, in colliderList, ref dataList);
				}

				sensorResult.Clear();
				length = colliderDataList.Length;
				for(int i = 0 ; i < length ; i++)
				{
					var data = colliderDataList[i];
					if(data.IsSafeFilter)
					{
						sensorResult.Add(colliderList[data.Index]);
					}
				}

				colliderList.Clear();
				colliderDataList.Dispose();
			}
			else
			{
				sensorResult.Clear();
			}
		}

		private NativeArray<ColliderData> RunCalculateColliderJob(Transform referenceTransform, in List<Collider> Colliders)
		{
			// 콜라이더 위치 배열 생성
			// 결과 저장 배열 생성
			NativeArray<ColliderData> results = new NativeArray<ColliderData>(Colliders.Count, Allocator.TempJob);
			for(int i = 0 ; i<Colliders.Count ; i++)
			{
				Collider item = Colliders[i];
				results[i] = new ColliderData(referenceTransform, item, i);
			}

			return results;
		}

		public struct ColliderData
		{
			public int Index;                           // 원래 리스트에서 콜리더의 인덱스
			public bool IsSafeFilter;                     // 필터 조건을 만족하면 true

			public enum Type : byte
			{
				[InspectorName("구형")]
				SphereCollider = 0,
				[InspectorName("규브")]
				BoxCollider = 1,
				[InspectorName("미지원")]
				Unsupported = 255
			}
			public Type ColliderType;

			public float3 CenterPoint;                  // 중심 지점
			public float3 ColliderSize;                 // 크기
			public float ColliderRotateY;               // 회전
			public float3 ClosestPoint;                 // 가장 가까운 지점

			public float3 TransformPosition;            // 월드 기준 Transform 위치
			public float3 TransformForward;             // 월드 기준 Transform 정면 방향

			public float Distance;                      // Transform => ClosestPoint 의 거리
			public float3 Direction;                    // Transform => CenterPoint 의 방향의 백터
			public float AngleYAxis;                    // TransformForward 과 Direction사이 각도 (-180 ~ 180)
			public float AngleYAxis360 => AngleYAxis >= 0 ? AngleYAxis : 360f - AngleYAxis; // TransformForward 과 Direction사이 각도 (0 ~ 360)

			public float3 DirectionXZnormalize;         // Direction 의 (XZ + normalize)
			public float3 TransformForwardXZnormalize;  // TransformForward 의 (XZ + normalize)

			public ColliderData(Transform transform, Collider collider, int index)
			{
				Index = index;
				IsSafeFilter = true;

				CenterPoint = collider.bounds.center;
				switch(collider)
				{
					case UnityEngine.SphereCollider sphere:
					{
						ColliderType = Type.SphereCollider;
						ColliderSize = Vector3.one * sphere.radius;
						ColliderRotateY = 0f;
					}
					break;
					case UnityEngine.CapsuleCollider capsule:
					{
						ColliderType = Type.SphereCollider;
						ColliderSize = Vector3.one * capsule.radius;
						ColliderRotateY = 0f;
					}
					break;
					case UnityEngine.BoxCollider box:
					{
						ColliderType = Type.BoxCollider;
						ColliderSize = box.size;
						ColliderRotateY = box.transform.rotation.y;
					}
					break;
					default:
					{
						ColliderType = Type.Unsupported;
						ColliderSize = Vector3.zero;
						ColliderRotateY = 0f;
					}
					break;
				}
				CenterPoint = collider.bounds.center;
				ClosestPoint = collider.ClosestPoint(transform.position);
				TransformPosition = transform.position;
				TransformForward = transform.forward;

				Direction = CenterPoint - TransformPosition;
				Distance = math.distance(ClosestPoint, TransformPosition);

				DirectionXZnormalize = new float3(Direction.x, 0, Direction.z);
				TransformForwardXZnormalize = new float3(TransformForward.x, 0, TransformForward.z);
				DirectionXZnormalize = math.normalize(DirectionXZnormalize);
				TransformForwardXZnormalize = math.normalize(TransformForwardXZnormalize);
				float dot = math.dot(TransformForwardXZnormalize, DirectionXZnormalize);
				AngleYAxis = math.degrees(math.acos(dot));
				float3 cross = math.cross(TransformForwardXZnormalize, DirectionXZnormalize);
				if(cross.y < 0)
				{
					AngleYAxis = -AngleYAxis;
				}
			}
		}

		void IShapeRenderer.OnDrawShapes(Camera cam)
		{
			if(!isActiveAndEnabled) return;
			Draw.Matrix = transform.localToWorldMatrix;
			Vector3 thisPos = transform.position;
			thisPos.y += 0.1f;
			int count = 0;
			count = sensorCollectors ==null ? 0 : sensorCollectors.Length;
			for(int i = 0 ; i < count ; i++)
			{
				sensorCollectors[i]?.OnDraw(cam);
			}

			count = sensorFilters ==null ? 0 : sensorFilters.Length;
			for(int i = 0 ; i < count ; i++)
			{
				sensorFilters[i]?.OnDraw(cam);
			}


			int resultCount = sensorResult == null ? 0 : sensorResult.Count;
			Draw.ResetAllDrawStates();
			Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);
			Color drawColor = Color.yellow;
			drawColor.a = 0.5f;
			Draw.Color = drawColor;
			Color lineStartColor = Color.red;
			lineStartColor.a = 0.5f;
			for(int i = 0 ; i < resultCount ; i++)
			{
				var result = sensorResult[i];
				if(result == null) continue;
				var bounds = result.bounds;
				var boundCenter= bounds.center;
				boundCenter.y = thisPos.y;
				Draw.Line(thisPos, boundCenter, 0.1f, lineStartColor, drawColor);
				Draw.RectangleBorder(boundCenter, rot, bounds.size.XZ(), 0.1f);
			}
		}
	}
}