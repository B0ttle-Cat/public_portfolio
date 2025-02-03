using System.Collections.Generic;

using Unity.Collections;
using Unity.Mathematics;

namespace BC.AISensor
{
	public class SortOfAngle : SensorArray
	{
		public enum SortMode
		{
			FrontToBack, // 정면에서 뒤로 가는 순
			BackToFront  // 뒤에서 정면으로 가는 순
		}

		// 정렬 방식 선택
		public SortMode sortMode = SortMode.FrontToBack;


		protected override void Init(SensorProvider self)
		{
		}

		protected override void SensorUpdate(SensorProvider self, ref NativeSlice<SensorProvider.ColliderData> dataList)
		{
			dataList.SortJob(new AngleComparer(sortMode));
		}

		protected override void Release(SensorProvider self)
		{
		}

		// IComparer 구현
		private struct AngleComparer : IComparer<SensorProvider.ColliderData>
		{
			private SortMode sortMode;

			public AngleComparer(SortMode sortMode)
			{
				this.sortMode=sortMode;
			}

			public int Compare(SensorProvider.ColliderData a, SensorProvider.ColliderData b)
			{
				float angleA = math.abs(a.AngleYAxis); // 정면 기준 각도
				float angleB = math.abs(b.AngleYAxis);
				return sortMode == SortMode.FrontToBack
					? angleA.CompareTo(angleB)
					: angleB.CompareTo(angleA);
			}
		}
	}
}
