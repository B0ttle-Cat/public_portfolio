using System;
using System.Collections.Generic;

using UnityEngine;

namespace TFContent
{
	public interface ISaveLoad
	{
		[Serializable]
		public struct SaveLoadData
		{
			public string savePath;
			public string typeName;
			public string jsonData;
			public List<InstanceData> instanceDataList;

			public SaveLoadData(SaveLoader saveLoader, string savePath, string typeName, string jsonData)
			{
				this.savePath=savePath;
				this.typeName=typeName;
				this.jsonData=jsonData;

				List<string> ids = SaveLoadObject.GetInstanceIDInJson(jsonData);
				instanceDataList = SaveLoadObject.CreateInstanceDataList(saveLoader, ids);
			}

			public SaveLoadData(SaveLoader saveLoader, string savePath, UnityEngine.Object saveObject)
			{
				this.savePath=savePath;
				this.typeName=saveObject.GetType().AssemblyQualifiedName;
				this.jsonData=JsonUtility.ToJson(saveObject);

				List<string> ids = SaveLoadObject.GetInstanceIDInJson(jsonData);
				instanceDataList = SaveLoadObject.CreateInstanceDataList(saveLoader, ids);
			}
			public SaveLoadData(SaveLoader saveLoader, string savePath, object saveObject)
			{
				this.savePath=savePath;
				this.typeName=saveObject.GetType().AssemblyQualifiedName;
				this.jsonData=JsonUtility.ToJson(saveObject);
				List<string> ids = SaveLoadObject.GetInstanceIDInJson(jsonData);
				instanceDataList = SaveLoadObject.CreateInstanceDataList(saveLoader, ids);
			}
		}
		[Serializable]
		public struct InstanceData
		{
			public bool isChild;
			public string savePath;
			public string typeName;
			public string saveInstanceID;
			public string loadInstanceID;

			public InstanceData(bool isChild, string savePath, string typeName, string instanceID)
			{
				this.isChild = isChild;
				this.savePath=savePath;
				this.typeName=typeName;
				this.saveInstanceID=instanceID;
				loadInstanceID = "";
			}
		}

		public SaveLoadData? Save(SaveLoader saveLoader, string savePath);
		public void Load(SaveLoadData loadData);

		public static void Load(string jsonData, Component component)
		{
			JsonUtility.FromJsonOverwrite(jsonData, component);
		}
		public static object Load(string jsonData, Type type)
		{
			return JsonUtility.FromJson(jsonData, type);
		}
	}


	public interface ISaveLoadIgnore
	{
	}
}