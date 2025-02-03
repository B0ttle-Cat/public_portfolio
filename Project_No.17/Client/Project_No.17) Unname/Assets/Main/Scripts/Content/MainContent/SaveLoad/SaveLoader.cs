using System;
using System.Collections.Generic;

using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;

using static TFContent.ISaveLoad;

namespace TFContent
{
	public class SaveLoader : MonoBehaviour, ISaveLoadIgnore
	{
		void Reset()
		{
			if(!TryGetComponent<ObjectBehaviour>(out _))
			{
				DestroyImmediate(this);
			}
		}
		public bool isOnlyISaveLoad;
		[NonSerialized]
		public string objectPath;
		[NonSerialized]
		public List<SaveLoadData> dataList;


		[ButtonGroup("Save")]
		internal virtual void OnSave()
		{
			objectPath = SaveLoadObject.ObjectPath(transform);
			dataList = new List<SaveLoadData>();
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
						SaveLoadData? saveLoadData = obj is ISaveLoad iSave
						? iSave.Save(this, savePath)
						: isOnlyISaveLoad ? null : new ISaveLoad.SaveLoadData(this, savePath, obj);

						if(saveLoadData.HasValue)
						{
							if(dataList.Count == 0) dataList.Add(saveLoadData.Value);
							else dataList.Insert(0, saveLoadData.Value);
						}
					}
					else if(odccItem is ComponentBehaviour com)
					{
						string savePath = SaveLoadObject.TransformPath(objectTarget.transform, com.transform);
						SaveLoadData? saveLoadData = com is ISaveLoad iSave
						? iSave.Save(this, savePath)
						: isOnlyISaveLoad ? null : new ISaveLoad.SaveLoadData(this, savePath, com);

						if(saveLoadData.HasValue)
						{
							dataList.Add(saveLoadData.Value);
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
					else if(component is ISaveLoad iSave)
					{
						var saveData = iSave.Save(this, "");
						if(saveData.HasValue) dataList.Add(saveData.Value);
					}
					else if(!isOnlyISaveLoad)
					{
						dataList.Add(new ISaveLoad.SaveLoadData(this, "", component));
					}
				}
			}
		}

		[ButtonGroup("Load")]
		internal virtual void OnLoadInstance(string objectPath, List<SaveLoadData> dataList)
		{
			if(string.IsNullOrWhiteSpace(objectPath) || dataList == null) return;

			if(TryGetComponent<ObjectBehaviour>(out _))
			{
				Debug.LogError("ObjectBehaviour와 동일한 위치에 있어야 합니다.");

				this.objectPath = objectPath;
				this.dataList = dataList;


				gameObject.SetActive(false);

				int length = dataList.Count;
				GameObject[] gameObjects = new GameObject[length];
				for(int i = 0 ; i < length ; i++)
				{
					SaveLoadData loadData = dataList[i];
					gameObjects[i] = SaveLoadObject.CreateChildObject(transform, dataList[i].savePath);
				}
				for(int i = 0 ; i < length ; i++)
				{
					GameObject target = gameObjects[i];
					if(target == null) continue;

					SaveLoadData loadData = dataList[i];
					string typeName = loadData.typeName;

					Type type = Type.GetType(typeName);
					if(!target.TryGetComponent(type, out Component component))
					{
						component = target.AddComponent(type);
					}
				}
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
				SaveLoadData loadData = dataList[i];
				SaveLoadObject.FindCurrentInstanceID(ref loadData, transform);
				dataList[i] = loadData;
			}

			for(int i = 0 ; i < length ; i++)
			{
				SaveLoadData loadData = dataList[i];
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
				SaveLoadData loadData = dataList[i];
				GameObject target = SaveLoadObject.FindChildObject(transform, loadData.savePath);
				if(target == null) continue;

				Type type = Type.GetType(loadData.typeName);
				string jsonData = loadData.jsonData;

				if(target.TryGetComponent(type, out Component component))
				{
					if(component is ISaveLoadIgnore)
					{
						continue;
					}
					else if(component is ISaveLoad load)
					{
						load.Load(loadData);
					}
					else if(!isOnlyISaveLoad)
					{
						ISaveLoad.Load(jsonData, component);
					}
				}
			}
			gameObject.SetActive(true);
		}

		/// <summary>
		/// 내부 데이터의 인스턴스가 번호가 아직 생성되지 않은 오브젝트를 참조해서는 안됨.
		/// </summary>
		public void OnLoadSolo(string objectPath, List<SaveLoadData> dataList)
		{
			OnLoadInstance(objectPath, dataList);
			OnLoadSubInstance();
			OnLoadData();
		}
	}
}