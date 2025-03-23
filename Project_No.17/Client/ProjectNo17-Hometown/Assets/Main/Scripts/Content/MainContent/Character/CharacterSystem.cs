using System.Linq;

using BC.AIProvider;
using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;
namespace TFContent
{
	public class CharacterSystem : ObjectBehaviour//, IOdccUpdate
	{
		[ShowInInspector,ReadOnly]
		private int unitInstantiateID = 0;
		protected override void BaseAwake()
		{
			unitInstantiateID = 0;
		}

		public async Awaitable<ObjectBehaviour> CreateCharacter(CharacterObjectData.PrefabData prefabData, CharacterObjectData.IFFData iffData, AbilityStruct ability)
		{
			var prefab = prefabData.Prefab;
			if(prefab == null) return null;

			GameObject[] newObjects = await InstantiateAsync(prefab.gameObject, transform);
			int length2 = newObjects.Length;
			if(length2 == 0) return null;

			var newObject = newObjects[0].GetComponent<ObjectBehaviour>();

			Vector3 pos = prefabData.position;
			var near = WorldSystem.MainPath.GetNearest(pos);
			pos = near.node != null ? near.position : pos;
			newObject.transform.SetLocalPositionAndRotation(pos, Quaternion.Euler(prefabData.rotation));
			newObject.transform.localScale = prefabData.scale;

			if(newObject.ThisContainer.TryGetComponent<IFFProvider>(out var iff))
			{
				iff.TeamID = iffData.IFFTeamID;
				iff.UnitID = ++unitInstantiateID;
			}

			if(ability != null && newObject.ThisContainer.TryGetComponent<AbilityProvider>(out var abilityProvider))
			{
				if(abilityProvider.AbilityStruct == null)
				{
					abilityProvider.AbilityStruct = ability;
				}
				else
				{
					abilityProvider.AbilityStruct.SetAbility(ability);
				}
			}

			bool isUserControl = iff.TeamID == IFFProvider.UserControlTeamID;
			if(isUserControl)
			{
				if(!newObject.TryGetComponent<UserCommandConnector>(out _))
				{
					newObject.gameObject.AddComponent<UserCommandConnector>();
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