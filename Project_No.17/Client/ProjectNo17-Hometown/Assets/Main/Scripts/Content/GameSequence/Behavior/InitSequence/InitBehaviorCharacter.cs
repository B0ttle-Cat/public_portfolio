using BC.AIProvider;
using BC.ODCC;
using BC.OdccBase;

using TFSystem;

using UnityEngine;
namespace TFContent
{
	[ExecuteInEditMode]
	public class InitBehaviorCharacter : OdccBehaviorTarget
	{
		private CharacterSystem characterSystem;
		private CharacterObjectData characterData;
		private IFFMatchingInfoData iffMatchingInfoData;
		protected override void BaseAwake()
		{
#if UNITY_EDITOR
			int childCount = transform.childCount;
			for(int i = 0 ; i < childCount ; i++)
			{
				var editorOnly = transform.GetChild(i).gameObject;
				if(editorOnly.tag =="EditorOnly")
				{
					//	editorOnly.SetActive(false);
					Object.Destroy(editorOnly, 1f);
				}
			}
#endif
		}

		protected override bool OnActionValid()
		{
			characterSystem = ThisContainer.GetObject<CharacterSystem>();
			characterData = DataCarrier.GetSeparate<GameContentLoader>().GetData<CharacterObjectData>();
			iffMatchingInfoData = DataCarrier.GetSeparate<GameContentLoader>().GetData<IFFMatchingInfoData>();
			return true;
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(characterSystem == null) return;
			if(characterData == null) return;
#if UNITY_EDITOR
			var unusedInEditor = characterData.UnusedInEditor;
#endif
			IFFProvider.IFFMatching = iffMatchingInfoData;
			IFFProvider.UserControlTeamID = iffMatchingInfoData.userControlTeamID;

			var prefabDatas = characterData.prefabDatas;
			var iffDatas = characterData.iffDatas;
			var abilityDatas = characterData.abilityDatas;
			int length = prefabDatas.Count;
			for(int i = 0 ; i < length ; i++)
			{
#if UNITY_EDITOR
				var unusedFlag = unusedInEditor[i];
				if(unusedFlag) continue;
#endif
				CharacterObjectData.PrefabData prefabData = prefabDatas[i];
				CharacterObjectData.IFFData iffData =  iffDatas[i];
				AbilityStruct ability =  abilityDatas[i].NewCopy();
				ObjectBehaviour newCharacterObject = await characterSystem.CreateCharacter(prefabData, iffData, ability);
				if(newCharacterObject != null) newCharacterObject.SetActive(false);
			}
		}

		protected override void OnActionEnd()
		{
		}
	}
}