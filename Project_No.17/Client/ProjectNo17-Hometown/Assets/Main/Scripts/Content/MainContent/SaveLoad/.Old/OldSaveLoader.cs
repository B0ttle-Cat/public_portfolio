using System;
using System.Collections.Generic;

using BC.Base;
using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace TFContent
{
	/// <summary>
	/// TODO::전면적인 개편 필요.
	/// SaveLoader 을 상속받아 각 오브젝트 특화된 SaveLoader 를 개발하기.
	/// + ISaveLoadTarget 를 통해 보조하기.
	/// + SaveLoadData 는 재귀형식으로 변경하기.
	/// </summary>
	public class OldSaveLoader : MonoBehaviour, ISaveLoadIgnore
	{
		void Reset()
		{
			if(!TryGetComponent<ObjectBehaviour>(out _))
			{
				DestroyImmediate(this);
			}
		}
		public bool useOnlyISaveLoadTarget;
		public string objectPath;
		public List<OldISaveLoadTarget.SaveLoadData> dataList;


		[ButtonGroup("Save")]
		internal virtual void OnSave()
		{
			objectPath = SaveLoadObject.ObjectPath(transform);
			dataList = new List<OldISaveLoadTarget.SaveLoadData>();
			if(TryGetComponent<ObjectBehaviour>(out ObjectBehaviour objectTarget))
			{
				List<IOdccItem> odccItems = objectTarget.ThisContainer.FindAllType<IOdccItem>();
				int length = odccItems.Count;
				for(int i = 0 ; i < length ; i++)
				{
					IOdccItem odccItem = odccItems[i];
					if(odccItem is ISaveLoadIgnore)
					{
						continue;
					}

					if(odccItem is ObjectBehaviour obj)
					{
						string savePath = "";
						OldISaveLoadTarget.SaveLoadData? saveLoadData = obj is OldISaveLoadTarget iTarget
						? new OldISaveLoadTarget.SaveLoadData(this, savePath, obj.GetType(), iTarget.Save())
						: useOnlyISaveLoadTarget ? null : new OldISaveLoadTarget.SaveLoadData(this, savePath, obj);

						if(saveLoadData.HasValue)
						{
							if(dataList.Count == 0) dataList.Add(saveLoadData.Value);
							else dataList.Insert(0, saveLoadData.Value);
						}
					}
					else if(odccItem is ComponentBehaviour com)
					{
						string savePath = SaveLoadObject.TransformPath(objectTarget.transform, com.transform);
						OldISaveLoadTarget.SaveLoadData? saveLoadData = com is OldISaveLoadTarget iTarget
						? new OldISaveLoadTarget.SaveLoadData(this, savePath, com.GetType(), iTarget.Save())
						: useOnlyISaveLoadTarget ? null : new OldISaveLoadTarget.SaveLoadData(this, savePath, com);

						if(saveLoadData.HasValue)
						{
							dataList.Add(saveLoadData.Value);
						}
					}
					else if(odccItem is DataObject data)
					{
						string savePath = "";
						OldISaveLoadTarget.SaveLoadData? saveLoadData = data is OldISaveLoadTarget iTarget
						? new OldISaveLoadTarget.SaveLoadData(this, savePath, data.GetType(), iTarget.Save())
						: useOnlyISaveLoadTarget ? null : new OldISaveLoadTarget.SaveLoadData(this, savePath, data);

						if(saveLoadData.HasValue)
						{
							if(dataList.Count == 0) dataList.Add(saveLoadData.Value);
							else dataList.Insert(0, saveLoadData.Value);
						}
					}
					else
					{
						Debug.LogError($"저장할 수 없는 오브젝트 타입입니다: {odccItem.GetType().FullName}");
						continue;
					}
				}
			}
			else
			{
				Component[] components =  GetComponents<Component>();
				int length = components.Length;
				for(int i = 0 ; i < length ; i++)
				{
					var component = components[i];
					if(component is null or ISaveLoadIgnore)
					{
						continue;
					}
					else if(component is OldISaveLoadTarget iTarget)
					{
						//var saveData = iTarget.Save();
						//if(saveData.HasValue) dataList.Add(saveData.Value);
					}
					else if(!useOnlyISaveLoadTarget)
					{
						dataList.Add(new OldISaveLoadTarget.SaveLoadData(this, "", component));
					}
				}
			}
		}

		[ButtonGroup("Load")]
		internal virtual void OnLoadInstance(string objectPath, List<OldISaveLoadTarget.SaveLoadData> dataList)
		{
			if(string.IsNullOrWhiteSpace(objectPath) || dataList == null) return;

			if(TryGetComponent<ObjectBehaviour>(out _))
			{

				this.objectPath = objectPath;
				this.dataList = dataList;


				gameObject.SetActive(false);

				int length = dataList.Count;
				GameObject[] gameObjects = new GameObject[length];
				for(int i = 0 ; i < length ; i++)
				{
					OldISaveLoadTarget.SaveLoadData loadData = dataList[i];
					gameObjects[i] = SaveLoadObject.CreateChildObject(transform, dataList[i].savePath);
				}
				for(int i = 0 ; i < length ; i++)
				{
					GameObject target = gameObjects[i];
					if(target == null) continue;

					OldISaveLoadTarget.SaveLoadData loadData = dataList[i];
					string typeName = loadData.typeName;

					Type type = Type.GetType(typeName);
					if(!target.TryGetComponent(type, out Component component))
					{
						component = target.AddComponent(type);
					}
				}
			}
			else
			{
				Debug.LogError("ObjectBehaviour와 동일한 위치에 있어야 합니다.");
			}
		}
		[ButtonGroup("Load")]
		internal virtual void OnLoadSubInstance()
		{
			if(string.IsNullOrWhiteSpace(objectPath) || dataList == null) return;

			gameObject.SetActive(false);

			int length = dataList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				OldISaveLoadTarget.SaveLoadData loadData = dataList[i];
				SaveLoadObject.FindCurrentInstanceID(ref loadData, transform);
				dataList[i] = loadData;
			}

			for(int i = 0 ; i < length ; i++)
			{
				OldISaveLoadTarget.SaveLoadData loadData = dataList[i];
				SaveLoadObject.JsonInstanceIDConvertSaveToLoad(ref loadData);
				dataList[i] = loadData;
			}
		}
		[ButtonGroup("Load")]
		internal virtual void OnLoadData()
		{
			if(string.IsNullOrWhiteSpace(objectPath) || dataList == null) return;

			int length = dataList.Count;

			for(int i = 0 ; i < length ; i++)
			{
				OldISaveLoadTarget.SaveLoadData loadData = dataList[i];
				GameObject target = HierarchyPathFind.FindObject(loadData.savePath, transform);
				if(target == null)
				{
					Debug.LogError($"OnLoadGame Missing GameObject: {loadData.savePath}");
					continue;
				}
				Type type = Type.GetType(loadData.typeName);
				string jsonData = loadData.jsonData;

				if(target.TryGetComponent(type, out Component component))
				{
					if(component is ISaveLoadIgnore)
					{
						continue;
					}
					else if(component is OldISaveLoadTarget iTarget)
					{
						iTarget.Load(loadData);
					}
					else if(!useOnlyISaveLoadTarget)
					{
						OldISaveLoadTarget.Load(jsonData, component);
					}
				}
			}
			gameObject.SetActive(true);
		}

		/// <summary>
		/// 내부 데이터의 인스턴스가 번호가 아직 생성되지 않은 오브젝트를 참조해서는 안됨.
		/// </summary>
		public void OnLoadSolo(string objectPath, List<OldISaveLoadTarget.SaveLoadData> dataList)
		{
			OnLoadInstance(objectPath, dataList);
			OnLoadSubInstance();
			OnLoadData();
		}
	}
}