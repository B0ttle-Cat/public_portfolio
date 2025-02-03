using System.Collections.Generic;
using System.Linq;

using BC.Base;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFContent
{

	[CreateAssetMenu(fileName = "CharacterObjectData", menuName = "BC/CharacterObjectData")]
	public class CharacterObjectData : ScriptableObject
	{
		public const string CharacterResourcesPath = "Assets/Main/Resources/CharacterObject";

		[System.Serializable]
		public struct PrefabData
		{
			public ResourcesKey<CharacterObject> prefab;
			public Vector3 position; // 위치 정보
			public Vector3 rotation; // 회전 정보
			public Vector3 scale; // 스케일 정보
			[ValueDropdown("IFFTeamList")]
			public int IFFTeamID;

			public CharacterObject Prefab => prefab.LoadAsset();
#if UNITY_EDITOR
			public void OnValidate()
			{
				prefab.OnValidate();
			}
			private ValueDropdownList<int> IFFTeamList()
			{
				var list = new ValueDropdownList<int>();

				var gameContentLoader = GameObject.FindAnyObjectByType<GameContentLoader>();
				if(gameContentLoader == null) return list;
				if(gameContentLoader.iffMatchingInfoData == null) return list;

				var infoList = gameContentLoader.iffMatchingInfoData.IFFMatchingInfo;
				int max = infoList.Select(i => i.MatchNumber.y).Max();
				for(int i = 0 ; i < max ; i++)
				{
					list.Add(i);
				}
				return list;
			}
#endif
		}
		[ListDrawerSettings(CustomAddFunction = "AddPrefabData")]
		public List<PrefabData> prefabDatas = new List<PrefabData>(); // 여러 프리팹 정보를 관리하는 리스트

#if UNITY_EDITOR
		private void AddPrefabData()
		{
			PrefabData newPrefabData = new PrefabData
			{
				prefab = new ResourcesKey<CharacterObject>("Assets/Main/Resources/CharacterObject"),
				position = Vector3.zero,
				rotation = Vector3.zero,
				scale = Vector3.one // 기본값 설정
			};

			prefabDatas.Add(newPrefabData);
		}
		internal void OnValidate()
		{
			int length = prefabDatas.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var data = prefabDatas[i];
				data.OnValidate();
				prefabDatas[i] = data;
			}
		}
#endif
	}
}
