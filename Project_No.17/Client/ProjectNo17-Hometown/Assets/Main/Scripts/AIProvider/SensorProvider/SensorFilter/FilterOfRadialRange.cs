//using System.Collections.Generic;
//using System.Linq;

//using BC.Shape;

//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Mathematics;

//using UnityEngine;

//namespace BC.AIProvider
//{
//	public class FilterOfRadialRange : SensorFilter
//	{
//		public StructArcShape[] radialShapes; // 여러 방사형 범위를 배열로 관리

//		public bool isDraw;
//		public Color drawColor = Color.white;
//		private IShapeRenderer.DrawStyle drawStyle;

//		protected override void SensorUpdate(SensorProvider self, in List<Collider> colliderList, ref NativeSlice<SensorProvider.SensoringData> dataList)
//		{
//			// NativeArray로 방사형 범위 전달
//			var radialRangeArray = new NativeArray<StructArcShape>(radialShapes, Allocator.TempJob);

//			// Job 생성 및 데이터 전달
//			var filterJob = new FilterJob
//			{
//				ColliderData = dataList,
//				RadialShapes = radialRangeArray,
//			};

//			// Job 실행
//			var handle = filterJob.Schedule(dataList.Length, 64);
//			handle.Complete();

//			// 메모리 해제
//			radialRangeArray.Dispose();
//		}

//		protected override void Init(SensorProvider self)
//		{
//			drawStyle = new IShapeRenderer.DrawStyle(drawColor);
//			drawStyle.ResetValue();
//		}

//		protected override void Release(SensorProvider self)
//		{
//		}


//		public override void OnDraw(Camera cam)
//		{
//			if(!isDraw) return;

//			drawStyle.color = drawColor;
//			drawStyle.ResetValue();
//			drawStyle.SetDrawStyle();

//			int length = radialShapes.Length;
//			for(int i = 0 ; i < length ; i++)
//			{
//				var shape = radialShapes[i];
//				shape.OnDrawShapes();
//			}
//		}
//		private struct FilterJob : IJobParallelFor
//		{
//			public NativeSlice<SensorProvider.SensoringData> ColliderData;

//			[Unity.Collections.ReadOnly]
//			public NativeArray<StructArcShape> RadialShapes;

//			public bool UseInRange;

//			public void Execute(int index)
//			{
//				// 각 데이터 가져오기
//				SensorProvider.SensoringData data = ColliderData[index];

//				data.IsSafeFilter = IsColliderHit(in data);
//				// 결과 저장
//				ColliderData[index] = data;

//			}
//			bool IsColliderHit(in SensorProvider.SensoringData data)
//			{
//				if(data.ColliderType == SensorProvider.SensoringData.Type.SphereCollider)
//				{
//					if(SphereColliderHit(in data))
//					{
//						return true;
//					}
//				}
//				else if(data.ColliderType == SensorProvider.SensoringData.Type.BoxCollider)
//				{
//					if(BoxColliderHit(in data))
//					{
//						return true;
//					}
//				}
//				if(UnsupportedColliderHit(in data))
//				{
//					return true;
//				}
//				return false;
//			}

//			private bool UnsupportedColliderHit(in SensorProvider.SensoringData data)
//			{
//				float normalizedAngle = NormalizeAngle(data.AngleYAxis);
//				float distance = data.Distance;

//				// 방사형 범위 확인
//				int RadialRangesLength = RadialShapes.Length;
//				for(int i = 0 ; i < RadialRangesLength ; i++)
//				{
//					StructArcShape rangeData = RadialShapes[i];

//					float centerAngle = rangeData.CenterAngle;
//					float halfAngleOffset  = rangeData.AngleOffset * 0.5f;
//					float minDistance = rangeData.MinDistance;
//					float maxDistance = rangeData.MaxDistance;

//					// 각도 조건 확인
//					float deltaAngle = math.abs(DeltaAngle(centerAngle, normalizedAngle));
//					bool angleInRange = deltaAngle <= halfAngleOffset;

//					// 거리 조건 확인
//					bool distanceInRange = distance >= minDistance && distance <= maxDistance;

//					// 각도와 거리 조건 모두 만족하면 범위 안에 포함
//					if(angleInRange && distanceInRange)
//					{
//						return true;
//					}
//				}
//				return false;
//			}
//			private bool SphereColliderHit(in SensorProvider.SensoringData data)
//			{
//				// 방사형 범위 확인
//				int RadialRangesLength = RadialShapes.Length;

//				float2 TransformPosition = data.TransformPosition.xz;
//				float arcRotateY= data.TransformYAxis;
//				float2 ColliderPosition = data.CenterPoint.xz;
//				float ColliderSize = data.ColliderSize.x;
//				float AngleYAxis = data.AngleYAxis360;
//				for(int i = 0 ; i < RadialRangesLength ; i++)
//				{
//					StructArcShape rangeData = RadialShapes[i];
//					if(rangeData.SphereColliderHit(TransformPosition, arcRotateY, AngleYAxis, ColliderPosition, ColliderSize))
//					{
//						return true;
//					}
//				}
//				return false;
//			}
//			private bool BoxColliderHit(in SensorProvider.SensoringData data)
//			{
//				// 방사형 범위 확인
//				int RadialRangesLength = RadialShapes.Length;

//				float2 TransformPosition = data.TransformPosition.xz;
//				float2 ColliderPosition = data.CenterPoint.xz;
//				float2 ColliderSize = data.ColliderSize.xz;
//				float ColliderRotate = data.ColliderRotateY;
//				for(int i = 0 ; i < RadialRangesLength ; i++)
//				{
//					StructArcShape rangeData = RadialShapes[i];
//					if(rangeData.BoxColliderHit(TransformPosition, ColliderPosition, ColliderSize, ColliderRotate))
//					{
//						return true;
//					}
//				}
//				return false;
//			}

//			private float NormalizeAngle(float angle)
//			{
//				// 0~360도로 정규화
//				return (angle % 360 + 360) % 360;
//			}

//			public static float DeltaAngle(float current, float target)
//			{
//				float num = Repeat(target - current, 360f);
//				if(num > 180f)
//				{
//					num -= 360f;
//				}

//				return num;
//				float Repeat(float t, float length)
//				{
//					return math.clamp(t - math.floor(t / length) * length, 0f, length);
//				}
//			}
//		}


//		public void OnDrawSensor(SensorProvider self)
//		{
//			Vector3 position = self.transform.position;
//			float angleY = self.transform.eulerAngles.y;

//			StructArcShape[] drawRadialShapes = radialShapes.ToArray();

//			int length = drawRadialShapes.Length;
//			for(int i = 0 ; i < length ; i++)
//			{
//				var draw = drawRadialShapes[i];
//				draw.CenterAngle += angleY;
//				drawRadialShapes[i] = draw;
//			}
//		}
//	}
//}
