using Sirenix.OdinInspector;

using Unity.Collections;

using UnityEngine;

namespace BC.AISensor
{
	public class CutoutOfIndex : SensorArray
	{
		public enum RemoveMode
		{
			RemoveBefore, // 지정된 인덱스 이전 삭제
			RemoveAfter,   // 지정된 인덱스 이후 삭제
		}

		public RemoveMode mode;
		public bool includeIndex;
		[MinValue(0)]
		public int index = 0;

		protected override void Init(SensorProvider self)
		{
		}


		protected override void SensorUpdate(SensorProvider self, ref NativeSlice<SensorProvider.ColliderData> dataList)
		{
			int start= 0;
			int count = dataList.Length;

			index = Mathf.Clamp(index, 0, count - 1);

			if(mode == RemoveMode.RemoveBefore)
			{
				start = 0;
				count = includeIndex ? index + 1 : index;
			}
			else // RemoveMode.RemoveAfter
			{
				start = includeIndex ? index : index + 1;
				count = count - start;
			}

			// 범위 내에서 삭제 실행
			if(count > 0)
			{
				dataList = new NativeSlice<SensorProvider.ColliderData>(dataList, start, count);
			}
		}

		protected override void Release(SensorProvider selfSS)
		{
		}


	}
}
