using System;
using System.Collections.Generic;
using System.Linq;

using BC.AIProvider;
using BC.Base;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

using Object = UnityEngine.Object;

namespace TFContent
{

	[CreateAssetMenu(fileName = "CharacterObjectData", menuName = "BC/CharacterObjectData")]
	public class CharacterObjectData : ScenarioObjectData
	{
		public const string CharacterResourcesPath = "Assets/Main/Resources/CharacterObject";
		[System.Serializable]
		public struct PrefabData
		{
			public ResourcesKey<CharacterObject> prefab;

			public GameObject PrefabObject => prefab.LoadAsset().gameObject;
#if UNITY_EDITOR
			public void OnValidate()
			{
				prefab.OnValidate(CharacterResourcesPath);
			}
#endif
		}
		[ListDrawerSettings(CustomAddFunction = "AddPrefabData"), TabGroup("Tab", "Prefabs", TabLayouting = TabLayouting.Shrink)]
		public List<PrefabData> prefabDatas = new List<PrefabData>(); // 여러 프리팹 정보를 관리하는 리스트

		[Serializable]
		public struct InstanceData
		{
			[ToggleLeft, HorizontalGroup("H1", width: 100, order: -1)]
			public bool unused;         // 미사용여부 (true : 사용하지 않음) 
			[HorizontalGroup("H1"), ValueDropdown("PrefabList")]
			public int prefabIndex;           // prefabData 에서의 Index
			[HorizontalGroup("H2"), TabGroup("H2/Tab", "Transform", TabLayouting = TabLayouting.Shrink, Order = 2, VisibleIf = "@VisibleIf")]
			public Vector3 position;    // 위치 정보
			[TabGroup("H2/Tab", "Transform")]
			public Vector3 rotation;    // 회전 정보

			[TabGroup("H2/Tab", "IFFTeamID", TabLayouting = TabLayouting.Shrink,VisibleIf = "@VisibleIf")]
			[ValueDropdown("IFFTeamList"), PropertyOrder(5)]
			public int iffTeamID;
			[TabGroup("H2/Tab", "IFFTeamID", TabLayouting = TabLayouting.Shrink,VisibleIf = "@VisibleIf")]
			[ReadOnly, PropertyOrder(5)]
			public int iffUnitID;
			[TabGroup("H2/Tab", "AbilityValue", TabLayouting = TabLayouting.Shrink, VisibleIf = "@VisibleIf")]
			[PropertyOrder(9)]
			public AbilityStruct abilityStruct;
#if UNITY_EDITOR
			private bool VisibleIf => !unused && prefabIndex >= 0;
			//[TabGroup("Tab", "Preview", TabLayouting = TabLayouting.Shrink, VisibleIf = "@VisibleIf")]
			[HorizontalGroup("H2", Width = 100, Order = 1, VisibleIf = "@VisibleIf")]
			[HideLabel, ReadOnly, EnableGUI, PreviewField(100, ObjectFieldAlignment.Center), ShowInInspector]
			private Object preview { get; set; }
			private List<PrefabData> prefabs { get; set; }
			public void OnValidate(int index, List<PrefabData> prefabDatas)
			{
				iffUnitID = index;

				prefabs = prefabDatas;
				int _prefabIndex = prefabIndex;
				if(_prefabIndex < 0)
				{
					preview = null;
					return;
				}
				_prefabIndex %= prefabDatas.Count;
				preview = prefabDatas[_prefabIndex].prefab.preview;
			}
			private ValueDropdownList<int> PrefabList()
			{
				var list = new ValueDropdownList<int>();
				if(prefabs == null) return list;

				int length = prefabs == null ? 0 : prefabs.Count;
				for(int i = 0 ; i < length ; i++)
				{
					list.Add($"{i:00} : {prefabs[i].prefab.resourcesName}", i);
				}
				return list;
			}
			private ValueDropdownList<int> IFFTeamList()
			{
				var list = new ValueDropdownList<int>();

				var gameContentLoader = GameObject.FindAnyObjectByType<InitGameContentLoader>();
				if(gameContentLoader == null) return list;
				if(gameContentLoader.iffMatchingObjectData == null) return list;

				var infoList = gameContentLoader.iffMatchingObjectData.IFFMatchingInfo;
				int max = infoList.Select(i => i.MatchNumber.y).Max();
				for(int i = 0 ; i < max ; i++)
				{
					list.Add(i);
				}
				return list;
			}

			[TabGroup("H2/Tab", "AbilityValue", TabLayouting = TabLayouting.Shrink, VisibleIf = "@VisibleIf")]
			[ButtonGroup("H2/Tab/Buttons"), PropertyOrder(8)]
			private void AddMissingType()
			{
				abilityStruct.AddMissingType();
			}
#endif
		}
		[TabGroup("Tab", "InstanceList", TabLayouting = TabLayouting.Shrink)]
		public List<InstanceData> instanceDatas = new List<InstanceData>();
#if UNITY_EDITOR
		private void AddPrefabData()
		{
			PrefabData newPrefabData = new PrefabData
			{
				prefab = new ResourcesKey<CharacterObject>(CharacterResourcesPath)
			};

			prefabDatas.Add(newPrefabData);
		}
		public override void OnValidate()
		{
			int length = prefabDatas.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var data = prefabDatas[i];
				data.OnValidate();
				prefabDatas[i] = data;
			}
			length = instanceDatas.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var data = instanceDatas[i];
				data.OnValidate(i, prefabDatas);
				instanceDatas[i] = data;
			}
		}

#endif
		public void SaveAllObject()
		{
			CharacterSystem characterSystem = FindAnyObjectByType<CharacterSystem>();
			if(characterSystem == null) return;
			instanceDatas = new List<InstanceData>();

			var characterObjectList = characterSystem.ThisContainer.GetChildAllObject<CharacterObject>(i => i.gameObject.activeInHierarchy);
			if(characterObjectList == null || characterObjectList.Count == 0) return;

			int length = characterObjectList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var characterObject = characterObjectList[i];
				if(characterObject == null) continue;
				Transform thisTransform = characterObject.ThisTransform;
				IFFProvider iffProvider = characterObject.ThisContainer.GetComponent<IFFProvider>();
				AbilityProvider abilityProvider = characterObject.ThisContainer.GetComponent<AbilityProvider>();
				instanceDatas.Add(new InstanceData {
					unused = false,
					prefabIndex = characterObject.prefabIndex,
					position = characterObject.ThisTransform.localPosition,
					rotation = characterObject.ThisTransform.localEulerAngles,

					iffTeamID = iffProvider != null ? iffProvider.TeamID : -1,
					iffUnitID = iffProvider != null ? iffProvider.UnitID : -1,
					abilityStruct = abilityProvider != null ? abilityProvider.AbilityStruct : null,
				});
			}
		}
	}
}
