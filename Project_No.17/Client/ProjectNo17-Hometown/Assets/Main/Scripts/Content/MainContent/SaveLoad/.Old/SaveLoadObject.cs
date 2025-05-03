using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using BC.Base;

using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace TFContent
{
	public static class SaveLoadObject
	{
		[Serializable]
		public struct ObjectData
		{
			public string objectPath;
			public Vector3 localPosition;
			public Quaternion localRotation;
			public Vector3 localScale;
			public int siblingIndex;
			public List<OldISaveLoadTarget.SaveLoadData> dataList;

			public ObjectData(OldSaveLoader saveLoader)
			{
				objectPath = saveLoader.objectPath;
				localPosition = saveLoader.transform.localPosition;
				localRotation = saveLoader.transform.localRotation;
				localScale = saveLoader.transform.localScale;
				siblingIndex = saveLoader.transform.GetSiblingIndex();
				dataList = saveLoader.dataList;
			}
		}

		public static List<ObjectData> OnSaveGame()
		{
			List<ObjectData> objectList = new List<ObjectData>();

			OldSaveLoader[] allSaveLoader = Object.FindObjectsByType<OldSaveLoader>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
			int length = allSaveLoader.Length;
			for(int i = 0 ; i < length ; i++)
			{
				OldSaveLoader saveLoader = allSaveLoader[i];
				saveLoader.OnSave();
				objectList.Add(new ObjectData(saveLoader));
			}

			return objectList;
		}
		public static void OnLoadGame(List<ObjectData> objectList)
		{
			List<(OldSaveLoader loader, ObjectData data)> allSaveLoader = new List<(OldSaveLoader,ObjectData)>();
			int length = objectList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				string path = objectList[i].objectPath;
				GameObject gameObject = HierarchyPathFind.FindObject(path);
				if(gameObject != null && gameObject.TryGetComponent<OldSaveLoader>(out var saveLoader))
				{
					allSaveLoader.Add((saveLoader, objectList[i]));
					var saveLoaderTransform = saveLoader.transform;
					saveLoaderTransform.localPosition = objectList[i].localPosition;
					saveLoaderTransform.localRotation = objectList[i].localRotation;
					saveLoaderTransform.localScale = objectList[i].localScale;
					saveLoaderTransform.SetSiblingIndex(objectList[i].siblingIndex);
				}
				else
				{
					Debug.LogError($"OnLoadGame Missing GameObject: {path}");
				}
			}
			length = allSaveLoader.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var loader = allSaveLoader[i].loader;
				var data = allSaveLoader[i].data;
				loader.OnLoadInstance(data.objectPath, data.dataList);
			}
			for(int i = 0 ; i < length ; i++)
			{
				allSaveLoader[i].loader.OnLoadSubInstance();
			}
			for(int i = 0 ; i < length ; i++)
			{
				allSaveLoader[i].loader.OnLoadData();
			}
		}
		public static string ObjectPath(Transform target)
		{
			return TransformPath(null, target);
		}
		public static string TransformPath(Transform root, Transform target)
		{
			if(target == null) return "";
			if(root == target) return "";

			Stack<string> names = new Stack<string>();

			Transform check = target;
			if(root == null)
			{
				while(check != null)
				{
					names.Push(check.name);
					check = check.parent;
				}
			}
			else
			{
				while(check != root)
				{
					if(check == null) return "";
					names.Push(check.name);
					check = check.parent;
				}
			}

			string path = "";
			while(names.Count > 0)
			{
				path += names.Pop();
				if(names.Count != 0)
				{
					path += "/";
				}
			}
			return path;
		}
		public static GameObject CreateChildObject(Transform transform, string path)
		{
			string[] parts = path.Split('/'); // 경로를 '/' 기준으로 나눔
			Transform current = transform;   // 현재 Transform 시작점

			int length = parts.Length;
			for(int i = 0 ; i < length ; i++)
			{
				string part = parts[i];
				Transform child = current.Find(part);

				if(child == null) // 자식이 없으면 새로 생성
				{
					GameObject newChild = new GameObject(part);
					newChild.transform.parent = current; // 부모를 설정
					child = newChild.transform;
				}

				current = child; // 다음 단계로 이동
			}
			return current.gameObject;
		}
		public static List<int> GetInstanceIDInJson(string jsonData)
		{
			List<int> ids = new List<int>();
			MatchCollection matches = Regex.Matches(jsonData, @"""instanceID""\s*:\s*(-?\d+)");
			foreach(Match item in matches)
			{
				string instanceID = Regex.Replace(item.Value, @"[^-\d]", "");
				if(int.TryParse(instanceID, out int _instanceID))
				{
					ids.Add(_instanceID);
				}
				else
				{
					Debug.Log($"int.TryParse == False : {instanceID}");
				}
			}
			return ids;
		}

		internal static List<OldISaveLoadTarget.InstanceData> CreateInstanceDataList(OldSaveLoader saveLoader, List<int> ids)
		{
			List<OldISaveLoadTarget.InstanceData> instanceDataList = new List<OldISaveLoadTarget.InstanceData>();

			int length = ids.Count;
			for(int i = 0 ; i < length ; i++)
			{
				int instanceID = ids[i];
				if(instanceID == 0) continue;
				if(Resources.InstanceIDIsValid(instanceID) == false)
				{
					Debug.Log($"Resources.InstanceIDIsValid == False : {instanceID}");
					continue;
				}
				Object obj = Resources.InstanceIDToObject(instanceID);
				if(obj is Component component)
				{
					bool isChild = true;
					string path = TransformPath(saveLoader.transform, component.transform);
					if(path.IsNullOrWhiteSpace() && saveLoader.transform != component.transform)
					{
						isChild = false;
						path = ObjectPath(component.transform);
					}
					string type = component.GetType().AssemblyQualifiedName;
					instanceDataList.Add(new OldISaveLoadTarget.InstanceData(isChild, path, type, instanceID));
				}
				else if(obj is GameObject gameObject)
				{
					bool isChild = true;
					string path = TransformPath(saveLoader.transform, gameObject.transform);
					if(path.IsNullOrWhiteSpace() && saveLoader.transform != gameObject.transform)
					{
						isChild = false;
						path = ObjectPath(gameObject.transform);
					}
					string type = gameObject.GetType().AssemblyQualifiedName;
					instanceDataList.Add(new OldISaveLoadTarget.InstanceData(isChild, path, type, instanceID));
				}
				else if(obj is ScriptableObject scriptableObject)
				{
					Debug.Log($"이런 형태의 저장은 지원하지 않는다. 저장이 필요한 경우 {typeof(OldISaveLoadTarget).Name} 를 구현하여 처리할 것");
				}
			}

			return instanceDataList;
		}



		internal static void FindCurrentInstanceID(ref OldISaveLoadTarget.SaveLoadData saveLoadData, Transform transform)
		{
			List<OldISaveLoadTarget.InstanceData> instanceDataList = saveLoadData.instanceDataList;
			int length = instanceDataList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				OldISaveLoadTarget.InstanceData instanceData = instanceDataList[i];
				bool isChild = instanceData.isChild;
				string savePath = instanceData.savePath;
				string typeName = instanceData.typeName;
				Type type = Type.GetType(typeName);

				if(type.IsSubclassOf(typeof(Component)) || type.Equals(typeof(GameObject)))
				{
					GameObject target = HierarchyPathFind.FindObject(savePath, isChild ? transform : null);
					if(target == null)
					{
						Debug.LogError($"OnLoadGame Missing {(isChild ? "Child " : "")}GameObject : {transform.name} : {savePath}");
						continue;
					}
					if(target.TryGetComponent(Type.GetType(typeName), out Component component))
					{
						instanceData.loadInstanceID = component.GetInstanceID();
					}
					else
					{
						instanceData.loadInstanceID = 0;
					}
				}
				else
				{

				}
				instanceDataList[i] = instanceData;
			}
		}
		internal static void JsonInstanceIDConvertSaveToLoad(ref OldISaveLoadTarget.SaveLoadData saveLoadData)
		{
			string jsonData = saveLoadData.jsonData;
			List<OldISaveLoadTarget.InstanceData> instanceDataList = saveLoadData.instanceDataList;
			int length = instanceDataList.Count;
			List<string> replaceText = new List<string>();
			for(int i = 0 ; i < length ; i++)
			{
				int saveID = instanceDataList[i].saveInstanceID;
				int loadID = instanceDataList[i].loadInstanceID;
				jsonData = Regex.Replace(jsonData, $@"(""instanceID""\s*:\s*)({saveID})", $"$1{loadID}");
			}

			saveLoadData.jsonData = jsonData;
		}
	}
}