using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

using static BC.AISensor.SensorProvider;

using Collider = UnityEngine.Collider;
namespace BC.AISensor
{
	public abstract class SensorModule
	{
#if UNITY_EDITOR
		[Title("@Editor_Title_Text")]
		[PropertyOrder(-101), ShowInInspector, DisplayAsString, EnableGUI, HideLabel, PropertySpace(0, -24)]
		private string Editor_Title => "";
		private string Editor_Title_Text => $"SensorModule({GetType().Name}): {key}";
#endif
		[PropertyOrder(-99)]
		[ToggleLeft, HorizontalGroup("Editor_HorizontalGroup", width: 70)]
		public bool enable;
		[PropertyOrder(-99)]
		[HideLabel, HorizontalGroup("Editor_HorizontalGroup", width: 30)]
		[Sirenix.OdinInspector.ReadOnly]
		public int index;
		[PropertyOrder(-99)]
		[HideLabel, HorizontalGroup("Editor_HorizontalGroup")]
		[PropertySpace(0, 10)]
		public string key;

		private bool IsInit = false;
		public void OnInit(SensorProvider self)
		{
			if(!IsInit)
			{
				IsInit = true;
				Init(self);
			}
		}
		public void OnRelease(SensorProvider self)
		{
			if(IsInit)
			{
				IsInit = false;
				Release(self);
			}
		}

		protected abstract void Init(SensorProvider self);
		protected abstract void Release(SensorProvider self);
	}

	[Serializable, HideReferenceObjectPicker]
	public abstract class SensorCollector : SensorModule
	{
		public void OnSensorUpdate(SensorProvider self, ref HashSet<Collider> list)
		{
			if(enable)
			{
				OnInit(self);
				SensorUpdate(self, ref list);
			}
			else
			{
				OnRelease(self);
			}
		}
		protected abstract void SensorUpdate(SensorProvider self, ref HashSet<Collider> list);

		public virtual void OnDraw(Camera cam) { }
	}
	[Serializable, HideReferenceObjectPicker]
	public abstract class SensorArray : SensorModule
	{
		public void OnSensorUpdate(SensorProvider self, ref NativeSlice<ColliderData> dataList)
		{
			if(enable)
			{
				OnInit(self);
				SensorUpdate(self, ref dataList);
			}
			else
			{
				OnRelease(self);
			}
		}
		protected abstract void SensorUpdate(SensorProvider self, ref NativeSlice<ColliderData> dataList);
	}
	[Serializable, HideReferenceObjectPicker]
	public abstract class SensorFilter : SensorModule
	{
		public void OnSensorUpdate(SensorProvider self, in List<Collider> colliderList, ref NativeSlice<ColliderData> dataList)
		{
			if(enable)
			{
				OnInit(self);
				SensorUpdate(self, in colliderList, ref dataList);

				// 필터링을 위한 Job 실행
				var filterJob = new FilterCollidersJob
				{
					data = dataList
				};
				JobHandle jobHandle = filterJob.Schedule();
				jobHandle.Complete();

				// dataList를 필터링된 범위만 남도록 조정
				dataList = dataList.Slice(0, filterJob.validCount);
			}
			else
			{
				OnRelease(self);
			}
		}
		protected abstract void SensorUpdate(SensorProvider self, in List<Collider> colliderList, ref NativeSlice<ColliderData> dataList);

		public virtual void OnDraw(Camera cam) { }


		[BurstCompile]
		struct FilterCollidersJob : IJob
		{
			public NativeSlice<ColliderData> data;
			public int validCount;

			public void Execute()
			{
				int lastValidIndex = 0;

				for(int i = 0 ; i < data.Length ; i++)
				{
					if(data[i].IsSafeFilter)
					{
						// 유효한 데이터는 앞으로 이동
						if(i != lastValidIndex)
						{
							data[lastValidIndex] = data[i];
						}
						lastValidIndex++;
					}
				}

				validCount = lastValidIndex;
			}
		}
	}
}