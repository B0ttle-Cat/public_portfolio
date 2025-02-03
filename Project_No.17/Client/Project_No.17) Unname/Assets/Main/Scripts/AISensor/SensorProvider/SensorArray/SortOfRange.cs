using System.Collections.Generic;

using Unity.Collections;
using Unity.Jobs;

namespace BC.AISensor
{
	public class SortOfRange : SensorArray
	{
		// 정렬 방식을 나타내는 Enum
		public enum SortMode
		{
			Closest, // 가까운 순으로 정렬
			Farthest // 먼 순으로 정렬
		}

		// 정렬 방식 선택
		public SortMode sortMode = SortMode.Closest;


		protected override void Init(SensorProvider self)
		{
		}

		protected override void SensorUpdate(SensorProvider self, ref NativeSlice<SensorProvider.ColliderData> dataList)
		{
			dataList.SortJob(new DistanceComparer(sortMode));
		}
		protected override void Release(SensorProvider self)
		{
		}

		struct DistanceJob : IJobParallelFor
		{
			[ReadOnly] public NativeArray<float> Distances;
			[WriteOnly] public NativeArray<int> Indices;
			public bool SortClosest;

			public void Execute(int index)
			{
				// 간단한 Bubble Sort (Job System은 빠른 정렬 라이브러리를 직접 제공하지 않음)
				for(int i = 0 ; i < Indices.Length - 1 ; i++)
				{
					for(int j = i + 1 ; j < Indices.Length ; j++)
					{
						bool condition = SortClosest
						? Distances[Indices[i]] > Distances[Indices[j]] // 가까운 순
                        : Distances[Indices[i]] < Distances[Indices[j]]; // 먼 순

						if(condition)
						{
							// Swap
							int temp = Indices[i];
							Indices[i] = Indices[j];
							Indices[j] = temp;
						}
					}
				}
			}
		}

		private struct DistanceComparer : IComparer<SensorProvider.ColliderData>
		{
			SortMode sortMode;
			public DistanceComparer(SortMode sortMode)
			{
				this.sortMode=sortMode;
			}
			public int Compare(SensorProvider.ColliderData x, SensorProvider.ColliderData y)
			{
				return sortMode == SortMode.Closest
					? x.Distance.CompareTo(y.Distance)
					: y.Distance.CompareTo(x.Distance);
			}
		}

	}
}
