using System.Collections.Generic;
using System.Linq;

using BC.AIProvider;
using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class CharacterSystem : ObjectBehaviour//, IOdccUpdate
	{
		public async Awaitable<ObjectBehaviour> CreateCharacter(CharacterObjectData.PrefabData prefabData, CharacterObjectData.InstanceData instanceData)
		{
			var prefab = prefabData.PrefabObject;
			if(prefab == null) return null;

			GameObject[] newObjects = await InstantiateAsync(prefab, transform);
			int length2 = newObjects.Length;
			if(length2 == 0) return null;

			var newObject = newObjects[0].GetComponent<ObjectBehaviour>();

			if(newObject.ThisContainer.TryGetObject<CharacterObject>(out var characterObject))
			{
				Vector3 pos = instanceData.position;
				var near = WorldSystem.MainPath.GetNearest(pos);
				pos = near.node != null ? near.position : pos;

				characterObject.prefabIndex = instanceData.prefabIndex;
				characterObject.ThisTransform.SetLocalPositionAndRotation(pos, Quaternion.Euler(instanceData.rotation));
				characterObject.ThisTransform.localScale = Vector3.one;
			}

			if(newObject.ThisContainer.TryGetComponent<IFFProvider>(out var iff))
			{
				iff.TeamID = instanceData.iffTeamID;
				iff.UnitID = instanceData.iffUnitID;
			}

			if(instanceData.abilityStruct != null && newObject.ThisContainer.TryGetComponent<AbilityProvider>(out var abilityProvider))
			{
				if(abilityProvider.AbilityStruct == null)
				{
					abilityProvider.SetAbilityStruct(instanceData.abilityStruct.NewCopy());
				}
				else
				{
					abilityProvider.AbilityStruct.SetAbility(instanceData.abilityStruct);
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
		public int FindEmptyUnitID()
		{
			IFFProvider[] allIFFProvider = Resources.FindObjectsOfTypeAll<IFFProvider>();
			int[] usedIDs = allIFFProvider
				.Select(i => i.UnitID)
				.OrderBy(i => i)
				.Distinct()
				.ToArray();

			int current = 0;

			while(current < usedIDs.Length && usedIDs[current] == current)
			{
				current++;
			}
			return current;
		}
		public int[] FindEmptyUnitID(int count)
		{
			ThisContainer.GetAllComponentInChild<IFFProvider>(out var allIFFProvider);

			int[] usedIDs = allIFFProvider
				.Select(i => i.UnitID)
				.OrderBy(i => i)
				.Distinct()
				.ToArray();

			List<int> emptyIDs = new List<int>();
			int current = 0;
			int usedIndex = 0;

			while(emptyIDs.Count < count)
			{
				if(usedIndex < usedIDs.Length && usedIDs[usedIndex] == current)
				{
					usedIndex++;
				}
				else
				{
					emptyIDs.Add(current);
				}

				current++;
			}

			return emptyIDs.ToArray();
		}
	}
}