using Unity.Collections;

using UnityEngine;

namespace BC.AISensor
{
	public class CutoutOfIndexStartEnd : SensorArray
	{
		public enum RangeMode
		{
			[InspectorName("InIndexRange 범위 내부를 남김")]
			IncludeRange, // Start-End 범위 내부를 남김
			[InspectorName("InIndexRange 범위 외부를 남김")]
			ExcludeRange  // Start-End 범위 외부를 남김
		}

		public RangeMode mode;
		public Vector2Int inIndexRange;
		public bool includeStart;
		public bool includeEnd;

		protected override void SensorUpdate(SensorProvider self, ref NativeSlice<SensorProvider.ColliderData> dataList)
		{
			int start = inIndexRange.x;
			int end = inIndexRange.y;
			int listLength = dataList.Length;

			// Start와 End를 범위 내로 조정
			start = Mathf.Clamp(start, 0, listLength - 1);
			end = Mathf.Clamp(end, 0, listLength - 1);

			// Start와 End 정렬 (start가 end보다 크면 스왑)
			if(start > end)
			{
				(start, end) = (end, start);
			}

			// Start와 End 인덱스 조정
			if(!includeStart) start++;
			if(!includeEnd) end--;

			// 새 리스트 생성
			if(mode == RangeMode.IncludeRange)
			{
				// 범위 내부를 남기고 나머지 삭제
				if(start > 0)
				{
					dataList = new NativeSlice<SensorProvider.ColliderData>(dataList, 0, start);
					end -= start; // End 위치 조정
				}
				if(end < listLength - 1)
				{
					dataList = new NativeSlice<SensorProvider.ColliderData>(dataList, end + 1, listLength - (end + 1));
				}
			}
			else // ExcludeRange
			{
				// 범위 외부를 남기고 내부 삭제
				if(end < listLength - 1)
				{
					dataList = new NativeSlice<SensorProvider.ColliderData>(dataList, end + 1, listLength - (end + 1));
				}
				if(start > 0)
				{
					dataList = new NativeSlice<SensorProvider.ColliderData>(dataList, 0, start);
				}
			}
		}

		protected override void Init(SensorProvider self)
		{
		}

		protected override void Release(SensorProvider self)
		{
		}


	}
}
