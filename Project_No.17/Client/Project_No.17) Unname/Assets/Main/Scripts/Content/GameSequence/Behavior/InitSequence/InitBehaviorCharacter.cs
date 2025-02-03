using BC.AISensor;
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
		private IFFMatchingInfoData iffData;
		protected override void BaseAwake()
		{
#if UNITY_EDITOR
			int childCount = transform.childCount;
			for(int i = 0 ; i < childCount ; i++)
			{
				if(transform.GetChild(i).gameObject.tag =="EditorOnly")
				{
					DestroyImmediate(transform.GetChild(i).gameObject);
					i--;
					childCount--;
				}
			}
#endif
		}

		protected override bool OnActionValid()
		{
			characterSystem = ThisContainer.GetObject<CharacterSystem>();
			characterData = DataCarrier.GetSeparate<GameContentLoader>().GetData<CharacterObjectData>();
			iffData = DataCarrier.GetSeparate<GameContentLoader>().GetData<IFFMatchingInfoData>();
			return true;
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(characterSystem == null) return;
			if(characterData == null) return;

			var prefabDatas = characterData.prefabDatas;
			int length = prefabDatas.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var prefabData = prefabDatas[i];
				GameObject newCharacterObject = await characterSystem.CreateCharacter(prefabData, prefabData.IFFTeamID == iffData.userControlTeamID);
				if(newCharacterObject != null) newCharacterObject.SetActive(false);
			}
		}

		protected override void OnActionEnd()
		{
		}
	}
}