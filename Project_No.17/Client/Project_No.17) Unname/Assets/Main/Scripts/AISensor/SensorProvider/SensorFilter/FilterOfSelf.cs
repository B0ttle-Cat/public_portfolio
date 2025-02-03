using System.Collections.Generic;

using Unity.Collections;

using UnityEngine;

namespace BC.AISensor
{
	public class FilterOfSelf : SensorFilter
	{
		private List<Collider> selfColliders;
		protected override void Init(SensorProvider self)
		{
			selfColliders = new List<Collider>(self.ThisObject.GetComponentsInChildren<Collider>());
		}

		protected override void SensorUpdate(SensorProvider self, in List<Collider> colliderList, ref NativeSlice<SensorProvider.ColliderData> dataList)
		{
			if(selfColliders == null || selfColliders.Count == 0) return;

			int length = dataList.Length;
			int length2 = selfColliders.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var data = dataList[i];
				var dataCollider = colliderList[data.Index];

				int findIndex = selfColliders.FindIndex(find => find == dataCollider);
				data.IsSafeFilter = findIndex < 0;
				dataList[i] = data;
			}
		}

		protected override void Release(SensorProvider self)
		{
			selfColliders.Clear();
			selfColliders = null;
		}
	}
}
