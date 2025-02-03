using System.Collections.Generic;
using System.Linq;

using BC.Shape;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace BC.AISensor
{
	public class FilterOfRadialRange : SensorFilter
	{
		public ArcShape[] radialShapes; // 여러 방사형 범위를 배열로 관리

		public bool isDraw;
		public Color drawColor = Color.white;
		private IShapeRenderer.DrawStyle drawStyle;

		protected override void SensorUpdate(SensorProvider self, in List<Collider> colliderList, ref NativeSlice<SensorProvider.ColliderData> dataList)
		{
			// NativeArray로 방사형 범위 전달
			var radialRangeArray = new NativeArray<ArcShape>(radialShapes, Allocator.TempJob);

			// Job 생성 및 데이터 전달
			var filterJob = new FilterJob
			{
				ColliderData = dataList,
				RadialShapes = radialRangeArray,
			};

			// Job 실행
			var handle = filterJob.Schedule(dataList.Length, 64);
			handle.Complete();

			// 메모리 해제
			radialRangeArray.Dispose();
		}

		protected override void Init(SensorProvider self)
		{
			drawStyle = new IShapeRenderer.DrawStyle(drawColor);
			drawStyle.ResetValue();
		}

		protected override void Release(SensorProvider self)
		{
		}


		public override void OnDraw(Camera cam)
		{
			if(!isDraw) return;

			drawStyle.color = drawColor;
			drawStyle.ResetValue();
			drawStyle.SetDrawStyle();

			int length = radialShapes.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var shape = radialShapes[i];
				shape.OnDrawShapes();
			}
		}
		[BurstCompile]
		private struct FilterJob : IJobParallelFor
		{
			public NativeSlice<SensorProvider.ColliderData> ColliderData;

			[Unity.Collections.ReadOnly]
			public NativeArray<ArcShape> RadialShapes;

			public bool UseInRange;

			public void Execute(int index)
			{
				// 각 데이터 가져오기
				SensorProvider.ColliderData data = ColliderData[index];

				data.IsSafeFilter = IsColliderHit(in data);
				// 결과 저장
				ColliderData[index] = data;

			}
			bool IsColliderHit(in SensorProvider.ColliderData data)
			{
				if(data.ColliderType == SensorProvider.ColliderData.Type.SphereCollider)
				{
					if(SphereColliderHit(in data))
					{
						return true;
					}
				}
				else if(data.ColliderType == SensorProvider.ColliderData.Type.BoxCollider)
				{
					if(BoxColliderHit(in data))
					{
						return true;
					}
				}
				return UnsupportedColliderHit(in data);
			}

			private bool UnsupportedColliderHit(in SensorProvider.ColliderData data)
			{
				float normalizedAngle = NormalizeAngle(data.AngleYAxis);
				float distance = data.Distance;

				// 방사형 범위 확인
				int RadialRangesLength = RadialShapes.Length;
				for(int i = 0 ; i < RadialRangesLength ; i++)
				{
					ArcShape rangeData = RadialShapes[i];

					float centerAngle = NormalizeAngle(rangeData.CenterAngle);
					float normalizedMinAngle = NormalizeAngle(rangeData.MinAngle);
					float normalizedMaxAngle = NormalizeAngle(rangeData.MaxAngle);
					float minDistance = rangeData.MinDistance;
					float maxDistance = rangeData.MaxDistance;

					// 각도 조건 확인
					bool angleInRange;
					if(normalizedMinAngle <= normalizedMaxAngle)
					{
						// 경계를 넘지 않는 경우
						angleInRange = normalizedAngle >= normalizedMinAngle && normalizedAngle <= normalizedMaxAngle;
					}
					else
					{
						// 경계를 넘는 경우 (예: 350 ~ 10)
						angleInRange = normalizedAngle >= normalizedMinAngle || normalizedAngle <= normalizedMaxAngle;
					}

					// 거리 조건 확인
					bool distanceInRange = distance >= minDistance && distance <= maxDistance;

					// 각도와 거리 조건 모두 만족하면 범위 안에 포함
					if(angleInRange && distanceInRange)
					{
						return true;
					}
				}
				return false;
			}
			private bool SphereColliderHit(in SensorProvider.ColliderData data)
			{
				// 방사형 범위 확인
				int RadialRangesLength = RadialShapes.Length;

				float2 TransformPosition = data.TransformPosition.xz;
				float2 ColliderPosition = data.CenterPoint.xz;
				float ColliderSize = data.ColliderSize.x;
				for(int i = 0 ; i < RadialRangesLength ; i++)
				{
					ArcShape rangeData = RadialShapes[i];
					if(rangeData.SphereColliderHit(TransformPosition, ColliderPosition, ColliderSize))
					{
						return true;
					}
				}
				return false;
			}
			private bool BoxColliderHit(in SensorProvider.ColliderData data)
			{
				// 방사형 범위 확인
				int RadialRangesLength = RadialShapes.Length;

				float2 TransformPosition = data.TransformPosition.xz;
				float2 ColliderPosition = data.CenterPoint.xz;
				float2 ColliderSize = data.ColliderSize.xz;
				float ColliderRotate = data.ColliderRotateY;
				for(int i = 0 ; i < RadialRangesLength ; i++)
				{
					ArcShape rangeData = RadialShapes[i];
					if(rangeData.BoxColliderHit(TransformPosition, ColliderPosition, ColliderSize, ColliderRotate))
					{
						return true;
					}
				}
				return false;
			}

			private float NormalizeAngle(float angle)
			{
				// 0~360도로 정규화
				return (angle % 360 + 360) % 360;
			}
		}


		public void OnDrawSensor(SensorProvider self)
		{
			Vector3 position = self.transform.position;
			float angleY = self.transform.eulerAngles.y;

			ArcShape[] drawRadialShapes = radialShapes.ToArray();

			int length = drawRadialShapes.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var draw = drawRadialShapes[i];
				draw.CenterAngle += angleY;
				drawRadialShapes[i] = draw;
			}
		}
	}
}
