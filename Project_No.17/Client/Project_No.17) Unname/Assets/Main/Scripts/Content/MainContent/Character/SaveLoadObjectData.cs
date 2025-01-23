using System.Collections.Generic;

using UnityEngine;

namespace TFContent
{
	[CreateAssetMenu(fileName = "SaveLoadObjectData", menuName = "BC/SaveLoadObjectData")]
	public class SaveLoadObjectData : ScriptableObject
	{
		public List<SaveLoadObject.ObjectData> saveLoadObjectDataList = new List<SaveLoadObject.ObjectData>();

		public void Save()
		{
			saveLoadObjectDataList = SaveLoadObject.OnSaveGame();
		}
		public void Load()
		{
			SaveLoadObject.OnLoadGame(saveLoadObjectDataList);
		}
	}
}
