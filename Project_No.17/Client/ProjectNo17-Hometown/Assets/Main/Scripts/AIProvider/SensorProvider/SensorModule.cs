//using System;
//using System.Collections.Generic;

//using Sirenix.OdinInspector;

//using Unity.Collections;

//using UnityEngine;

//using static BC.AIProvider.SensorProvider;

//using Collider = UnityEngine.Collider;
//namespace BC.AIProvider
//{
//	public abstract class SensorModule
//	{
//#if UNITY_EDITOR
//		[PropertyOrder(-101), ShowInInspector, DisplayAsString(EnableRichText = true), EnableGUI, HideLabel]
//		[InlineButton("PingThisDataScript", " This Script ")]
//		private string Editor_Title_Text => $"<b>SensorModule({GetType().Name}): {key}</b>";
//		private double Editor_LastClickTime = -1; // 마지막 클릭 시간을 기록
//		private const double Editor_ClickInterval = 0.25; // 클릭 간격
//		private void PingThisDataScript()
//		{
//			double currentTime = UnityEditor.EditorApplication.timeSinceStartup;
//			if(Editor_LastClickTime > 0 && (currentTime - Editor_LastClickTime) <= Editor_ClickInterval)
//			{
//				// 파일 열기
//				OpenThisDataScript();
//			}
//			else
//			{
//				// 현재 컴포넌트의 이름을 기준으로 스크립트 검색
//				string scriptName = GetType().Name;
//				string scriptFileName = $"{GetType().Name}.cs";

//				string[] guids = UnityEditor.AssetDatabase.FindAssets($"{scriptName} t:Script");

//				foreach(string guid in guids)
//				{
//					// 첫 번째 검색 결과를 기준으로 Asset 경로 가져오기
//					string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
//					if(System.IO.Path.GetFileName(path) == scriptFileName)
//					{
//						UnityEngine.Object scriptAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

//						// 에디터에서 해당 스크립트를 강조
//						UnityEditor.EditorGUIUtility.PingObject(scriptAsset);
//						break;
//					}
//				}
//			}
//			Editor_LastClickTime = currentTime;
//		}
//		private void OpenThisDataScript()
//		{
//			string scriptName = GetType().Name;
//			string scriptFileName = $"{scriptName}.cs";

//			string[] guids = UnityEditor.AssetDatabase.FindAssets($"{scriptName} t:Script");

//			foreach(string guid in guids)
//			{
//				string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
//				if(System.IO.Path.GetFileName(path) == scriptFileName)
//				{
//					// 에디터에서 해당 파일 열기
//					UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
//				}
//			}
//		}
//#endif
//		[PropertyOrder(-99)]
//		[ToggleLeft, HorizontalGroup("Editor_HorizontalGroup", width: 70)]
//		public bool enable;
//		[PropertyOrder(-99)]
//		[HideLabel, HorizontalGroup("Editor_HorizontalGroup", width: 30)]
//		[Sirenix.OdinInspector.ReadOnly]
//		public int index;
//		[PropertyOrder(-99)]
//		[HideLabel, HorizontalGroup("Editor_HorizontalGroup")]
//		[PropertySpace(0, 10)]
//		public string key;

//		private bool IsInit = false;
//		public void OnInit(SensorProvider self)
//		{
//			if(!IsInit)
//			{
//				IsInit = true;
//				Init(self);
//			}
//		}
//		public void OnRelease(SensorProvider self)
//		{
//			if(IsInit)
//			{
//				IsInit = false;
//				Release(self);
//			}
//		}

//		protected abstract void Init(SensorProvider self);
//		protected abstract void Release(SensorProvider self);
//	}

//	[Serializable, HideReferenceObjectPicker]
//	public abstract class SensorCollector : SensorModule
//	{
//		public void OnSensorUpdate(SensorProvider self, ref HashSet<Collider> list)
//		{
//			if(enable)
//			{
//				OnInit(self);
//				SensorUpdate(self, ref list);
//			}
//			else
//			{
//				OnRelease(self);
//			}
//		}
//		protected abstract void SensorUpdate(SensorProvider self, ref HashSet<Collider> list);

//		public virtual void OnDraw(Camera cam) { }
//	}
//	[Serializable, HideReferenceObjectPicker]
//	public abstract class SensorArray : SensorModule
//	{
//		public void OnSensorUpdate(SensorProvider self, ref NativeSlice<SensoringData> dataList)
//		{
//			if(enable)
//			{
//				OnInit(self);
//				SensorUpdate(self, ref dataList);
//			}
//			else
//			{
//				OnRelease(self);
//			}
//		}
//		protected abstract void SensorUpdate(SensorProvider self, ref NativeSlice<SensoringData> dataList);
//	}
//	[Serializable, HideReferenceObjectPicker]
//	public abstract class SensorFilter : SensorModule
//	{
//		public void OnSensorUpdate(SensorProvider self, in List<Collider> colliderList, ref NativeSlice<SensoringData> dataList)
//		{
//			if(enable)
//			{
//				OnInit(self);
//				SensorUpdate(self, in colliderList, ref dataList);

//				int validCount = FilterUpdate(dataList);
//				// dataList를 필터링된 범위만 남도록 조정
//				dataList = dataList.Slice(0, validCount);
//			}
//			else
//			{
//				OnRelease(self);
//			}
//		}
//		protected abstract void SensorUpdate(SensorProvider self, in List<Collider> colliderList, ref NativeSlice<SensoringData> dataList);

//		public virtual void OnDraw(Camera cam) { }

//		private int FilterUpdate(NativeSlice<SensoringData> data)
//		{
//			int lastValidIndex = 0;

//			for(int i = 0 ; i < data.Length ; i++)
//			{
//				if(data[i].IsSafeFilter)
//				{
//					// 유효한 데이터는 앞으로 이동
//					if(i != lastValidIndex)
//					{
//						data[lastValidIndex] = data[i];
//					}
//					lastValidIndex++;
//				}
//			}

//			return lastValidIndex;
//		}
//	}
//}