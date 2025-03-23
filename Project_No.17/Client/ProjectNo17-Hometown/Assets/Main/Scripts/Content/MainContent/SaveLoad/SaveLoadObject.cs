using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using BC.Base;

using UnityEngine;

using static TFContent.ISaveLoad;

using Object = UnityEngine.Object;

namespace TFContent
{
	public static class SaveLoadObject
	{
		[Serializable]
		public struct ObjectData
		{
			public string objectPath;
			public List<SaveLoadData> dataList;

			public ObjectData(SaveLoader saveLoader)
			{
				objectPath = saveLoader.objectPath;
				dataList = saveLoader.dataList;
			}
		}

		private static Dictionary<string, Component> AllComponentInstanceID;

		public static List<ObjectData> OnSaveGame()
		{
			List<ObjectData> objectList = new List<ObjectData>();

			AllComponentInstanceID = new Dictionary<string, Component>();

			SaveLoader[] allSaveLoader = Object.FindObjectsByType<SaveLoader>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
			int length = allSaveLoader.Length;
			for(int i = 0 ; i < length ; i++)
			{
				SaveLoader saveLoader = allSaveLoader[i];
				saveLoader.OnSave();
				objectList.Add(new ObjectData(saveLoader));
			}

			AllComponentInstanceID.Clear();

			return objectList;
		}
		public static void OnLoadGame(List<ObjectData> objectList)
		{
			AllComponentInstanceID = new Dictionary<string, Component>();

			List<(SaveLoader loader, ObjectData data)> allSaveLoader = new List<(SaveLoader,ObjectData)>();
			int length = objectList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				string path = objectList[i].objectPath;
				GameObject gameObject = GameObject.Find(path);
				if(gameObject.TryGetComponent<SaveLoader>(out var saveLoader))
				{
					allSaveLoader.Add((saveLoader, objectList[i]));
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

			AllComponentInstanceID.Clear();
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
		public static GameObject FindChildObject(Transform transform, string path)
		{
			if(string.IsNullOrWhiteSpace(path)) return transform.gameObject;
			Transform findTr = transform.Find(path);
			return findTr == null ? null : findTr.gameObject;
		}
		public static List<string> GetInstanceIDInJson(string jsonData)
		{
			List<string> ids = new List<string>();
			MatchCollection matches = Regex.Matches(jsonData, @"""instanceID""\s*:\s*(-?\d+)");
			foreach(Match item in matches)
			{
				string instanceID = Regex.Replace(item.Value, @"[^-\d]", "");
				ids.Add(instanceID);
			}
			return ids;
		}

		internal static List<ISaveLoad.InstanceData> CreateInstanceDataList(SaveLoader saveLoader, List<string> ids)
		{
			List<ISaveLoad.InstanceData> instanceDataList = new List<ISaveLoad.InstanceData>();

			if(AllComponentInstanceID == null)
			{
				AllComponentInstanceID = new Dictionary<string, Component>();
				Component[] allComponents = Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
				for(int i = 0 ; i<allComponents.Length ; i++)
				{
					Component item = allComponents[i];
					AllComponentInstanceID[item.GetInstanceID().ToString()] = item;
				}
			}

			int length = ids.Count;
			for(int i = 0 ; i < length ; i++)
			{
				string instanceID = ids[i];
				if(AllComponentInstanceID.TryGetValue(ids[i], out var component))
				{
					bool isChild = true;
					string path = TransformPath(saveLoader.transform, component.transform);
					if(path.IsNullOrWhiteSpace() && saveLoader.transform != component.transform)
					{
						isChild = false;
						path = ObjectPath(component.transform);
					}
					string type = component.GetType().AssemblyQualifiedName;
					instanceDataList.Add(new ISaveLoad.InstanceData(isChild, path, type, instanceID));
				}
			}

			return instanceDataList;
		}



		internal static void FindCurrentInstanceID(ref ISaveLoad.SaveLoadData saveLoadData, Transform transform)
		{
			List<ISaveLoad.InstanceData> instanceDataList = saveLoadData.instanceDataList;
			int length2 = instanceDataList.Count;
			for(int ii = 0 ; ii < length2 ; ii++)
			{
				ISaveLoad.InstanceData instanceData = instanceDataList[ii];
				bool isChild = instanceData.isChild;
				string savePath = instanceData.savePath;
				string typeName = instanceData.typeName;

				GameObject target = isChild ? SaveLoadObject.FindChildObject(transform, savePath) : GameObject.Find(savePath);
				if(target.TryGetComponent(Type.GetType(typeName), out Component component))
				{
					instanceData.loadInstanceID = component.GetInstanceID().ToString();
				}
				else
				{
					instanceData.loadInstanceID = "0";
				}
				instanceDataList[ii] = instanceData;
			}
		}
		internal static void JsonInstanceIDConvertSaveToLoad(ref ISaveLoad.SaveLoadData saveLoadData)
		{
			string jsonData = saveLoadData.jsonData;
			List<ISaveLoad.InstanceData> instanceDataList = saveLoadData.instanceDataList;
			int length = instanceDataList.Count;
			List<string> replaceText = new List<string>();
			for(int i = 0 ; i < length ; i++)
			{
				string saveID = instanceDataList[i].saveInstanceID;
				string loadID = instanceDataList[i].loadInstanceID;
				jsonData = Regex.Replace(jsonData, $@"(""instanceID""\s*:\s*)({saveID})", $"$1{loadID}");
			}

			saveLoadData.jsonData = jsonData;
		}
	}
}