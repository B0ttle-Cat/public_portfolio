using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using UnityEngine;

namespace TFContent.SaveLoad
{
	[CreateAssetMenu(fileName = "SaveLoadObjectData", menuName = "BC/SaveLoadObjectData")]
	public class SaveLoadObjectData : ScenarioObjectData
	{
		public SaveLoadData rootData;

		public override void OnValidate()
		{
		}

		[ButtonGroup("SaveLoad")]
		public void SaveAllObject()
		{
			SaveLoader[] saveList = Object.FindObjectsByType<SaveLoader>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
			rootData = new SaveLoadData();

			int length = saveList.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var saveLoader = saveList[i];
				var key = saveLoader.GetSaveLoadKey();
				var data = saveLoader.OnSave();
				if(key.IsNotValid || data.IsEmpty) continue;
				rootData.SaveData(saveLoader.GetSaveLoadKey(), saveLoader.OnSave());
			}
		}
		[ButtonGroup("SaveLoad")]
		public void LoadAllObject()
		{
			var loadList = rootData.SaveList;
			int length = loadList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var load = loadList[i];
				SaveLoadKey key = load.key;

				if(key.IsNotValid)
				{
					Debug.LogError($"올바른 키 값 으로 지정되지 않았습니다. {key}");
					continue;
				}

				if(load.value is not SaveLoadData data)
				{
					Debug.LogError($"Not Found SaveLoadData : {key} : (해당하는 key의 데이터 타입이 SaveLoadData가 아닙니다.) (현재 Type:{load.value.GetType().FullName})");
					continue;
				}

				if(data.IsEmpty)
				{
					Debug.LogError($"Not Found SaveLoadData : {key} : 빈 데이터 값을 가진 키입니다.");
					continue;
				}
				string[] paths = key.path.Split('/');
				string rootName = '/'+paths[0];
				GameObject rootObject = GameObject.Find(rootName);

				if(rootObject == null)
				{
					Debug.LogError($"Not Found Root Object : {rootName} : (Root 오브젝트가 생성되지 않았거나, 비활성 상태 일 수 있습니다.)");
					continue;
				}

				Transform rootTr = rootObject.transform;
				string childPath = paths.Length > 1 ? string.Join('/', paths[1..^1]) : "";
				Transform targetTr = string.IsNullOrWhiteSpace(childPath)? rootTr : rootTr.Find(childPath);

				if(targetTr == null)
				{
					Debug.LogError($"Not Found Child Object : {childPath} : (해당하는 오브젝트가 생성되지 않은 것 같습니다.)");
					continue;
				}

				if(!targetTr.TryGetComponent<SaveLoader>(out var saveLoader))
				{
					Debug.LogError($"Not Found SaveLoader : {childPath} : (해당하는 오브젝트에 SaveLoader가 없습니다.)");
					continue;
				}

				saveLoader.OnLoad(data);
			}
		}
	}
}
