using System;
using System.Collections.Generic;

using UnityEngine;

namespace TFContent
{
	public interface OldISaveLoadTarget
	{
		[Serializable]
		public struct SaveLoadData
		{
			public string savePath;
			public string typeName;
			[TextArea(3,10)]
			public string jsonData;
			public List<InstanceData> instanceDataList;

			public SaveLoadData(OldSaveLoader saveLoader, string savePath, Type saveType, string jsonData)
			{
				this.savePath=savePath;
				this.typeName=saveType.AssemblyQualifiedName;
				this.jsonData=jsonData;

				List<int> ids = SaveLoadObject.GetInstanceIDInJson(jsonData);
				instanceDataList = SaveLoadObject.CreateInstanceDataList(saveLoader, ids);
			}

			public SaveLoadData(OldSaveLoader saveLoader, string savePath, UnityEngine.Object saveObject)
			{
				this.savePath=savePath;
				this.typeName=saveObject.GetType().AssemblyQualifiedName;
				this.jsonData=JsonUtility.ToJson(saveObject, true);

				List<int> ids = SaveLoadObject.GetInstanceIDInJson(jsonData);
				instanceDataList = SaveLoadObject.CreateInstanceDataList(saveLoader, ids);
			}
			public SaveLoadData(OldSaveLoader saveLoader, string savePath, object saveObject)
			{
				this.savePath=savePath;
				this.typeName=saveObject.GetType().AssemblyQualifiedName;
				this.jsonData=JsonUtility.ToJson(saveObject, true);
				List<int> ids = SaveLoadObject.GetInstanceIDInJson(jsonData);
				instanceDataList = SaveLoadObject.CreateInstanceDataList(saveLoader, ids);
			}
		}
		[Serializable]
		public struct InstanceData
		{
			public bool isChild;
			public string savePath;
			public string typeName;
			public int saveInstanceID;
			public int loadInstanceID;

			public InstanceData(bool isChild, string savePath, string typeName, int instanceID)
			{
				this.isChild = isChild;
				this.savePath=savePath;
				this.typeName=typeName;
				this.saveInstanceID=instanceID;
				loadInstanceID = 0;
			}
		}

		public string Save();
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