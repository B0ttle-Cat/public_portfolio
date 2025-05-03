using BC.AIProvider;
using BC.ODCC;

using TFSystem;

using UnityEngine;
namespace TFContent
{
	[ExecuteInEditMode]
	public class InitSequenceCharacter : GameSequenceItem
	{
		private CharacterSystem characterSystem;
		private CharacterObjectData characterData;
		private IFFMatchingObjectData iffMatchingInfoData;
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
			characterData = DataCarrier.GetSeparate<InitGameContentLoader>().GetData<CharacterObjectData>();
			iffMatchingInfoData = DataCarrier.GetSeparate<InitGameContentLoader>().GetData<IFFMatchingObjectData>();
			return true;
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(characterSystem == null) return;
			if(characterData == null) return;

			IFFProvider.IFFMatching = iffMatchingInfoData;

			var prefabDatas = characterData.prefabDatas;
			var instanceDatas = characterData.instanceDatas;
			int length = instanceDatas.Count;
			for(int i = 0 ; i < length ; i++)
			{
				CharacterObjectData.InstanceData instanceData =  instanceDatas[i];
				if(instanceData.unused) continue;
				if(instanceData.prefabIndex < 0) continue;
				int prefabIndex = instanceData.prefabIndex % prefabDatas.Count;
				CharacterObjectData.PrefabData prefabData = prefabDatas[prefabIndex];
				ObjectBehaviour newCharacterObject = await characterSystem.CreateCharacter(prefabData, instanceData);
				if(newCharacterObject != null) newCharacterObject.SetActive(false);
			}
		}

		protected override void OnActionEnd()
		{
			DestroyThis();
		}
	}
}