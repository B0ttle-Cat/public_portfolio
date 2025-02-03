using System.Linq;

using BC.AISensor;
using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;
namespace TFContent
{
	public class CharacterSystem : ObjectBehaviour//, IOdccUpdate
	{
		[ShowInInspector,ReadOnly]
		private uint unitInstantiateID = 0;
		protected override void BaseAwake()
		{
			unitInstantiateID = 0;
		}

		public async Awaitable<GameObject> CreateCharacter(CharacterObjectData.PrefabData prefabData, bool isUserControl = false)
		{
			var prefab = prefabData.Prefab;
			if(prefab == null) return null;

			GameObject[] newObjects = await InstantiateAsync(prefab.gameObject, transform);
			int length2 = newObjects.Length;
			if(length2 == 0) return null;

			var newObject = newObjects[0];

			Vector3 pos = prefabData.position;
			var near = WorldSystem.MainPath.GetNearest(pos);
			pos = near.node != null ? near.position : pos;
			newObject.transform.SetLocalPositionAndRotation(pos, Quaternion.Euler(prefabData.rotation));
			newObject.transform.localScale = prefabData.scale;

			if(prefab.ThisContainer.TryGetComponent<IFFProvider>(out var iff))
			{
				iff.TeamID = prefabData.IFFTeamID;
				iff.UnitID = ++unitInstantiateID;
			}


			if(isUserControl)
			{
				if(!newObject.TryGetComponent<UserCommandConnector>(out _))
				{
					newObject.AddComponent<UserCommandConnector>();
				}
			}

			return newObject;
		}

		public async Awaitable CharacterDataSoloLoad(CharacterObject characterObject, SaveLoadObject.ObjectData objectData)
		{
			if(characterObject == null) return;
			if(!characterObject.gameObject.TryGetComponent<SaveLoader>(out var loader)) return;

			loader.OnLoadSolo(objectData.objectPath, objectData.dataList);

			if(characterObject.ThisContainer.TryGetComponent<IFFProvider>(out var iff))
			{
				if(unitInstantiateID < iff.UnitID) unitInstantiateID = iff.UnitID;
			}
		}
		public void CharacterDataMultiLoad(CharacterObject[] characterObjects, SaveLoadObject.ObjectData[] objectDatas)
		{
			if(characterObjects == null || characterObjects.Length == 0) return;
			if(objectDatas == null || objectDatas.Length == 0) return;
			if(characterObjects.Length ==  objectDatas.Length) return;

			var saveLoaderList = characterObjects.Select(i => i.GetComponent<SaveLoader>()).ToArray();
			int length = saveLoaderList.Length;
			for(int i = 0 ; i<length ; i++)
			{
				SaveLoadObject.ObjectData data = objectDatas[i];
				saveLoaderList[i].OnLoadInstance(data.objectPath, data.dataList);
			}
			for(int i = 0 ; i<length ; i++)
			{
				saveLoaderList[i].OnLoadSubInstance();
			}
			for(int i = 0 ; i<length ; i++)
			{
				saveLoaderList[i].OnLoadData();
			}
			for(int i = 0 ; i<length ; i++)
			{
				if(characterObjects[i].ThisContainer.TryGetComponent<IFFProvider>(out var iff))
				{
					if(unitInstantiateID < iff.UnitID) unitInstantiateID = iff.UnitID;
				}
			}
		}
	}
}