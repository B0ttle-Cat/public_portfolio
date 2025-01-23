using BC.OdccBase;

using TFSystem;

using UnityEngine;
namespace TFContent
{
	[ExecuteInEditMode]
	public class InitBehaviorCharacter : OdccBehaviorTarget
	{
		private CharacterObjectData characterData;

		protected override bool OnActionValid()
		{
			characterData = DataCarrier.GetSeparate<GameContentLoader>().GetData<CharacterObjectData>();
#if UNITY_EDITOR
			if(TryGetComponent<PreviewCharacterObjectData>(out var preview))
			{
				Destroy(preview);
			}
#endif
			return true;
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(characterData == null) return;
			var prefabDatas = characterData.prefabDatas;
			int length = prefabDatas.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var prefabData = prefabDatas[i];

				GameObject[] newObjects = await InstantiateAsync(prefabData.Prefab, transform);
				int create = newObjects.Length;
				for(int ii = 0 ; ii < length ; ii++)
				{
					var newObject = newObjects[i];
					newObject.transform.SetLocalPositionAndRotation(prefabData.position, Quaternion.Euler(prefabData.rotation));
					newObject.transform.localScale = prefabData.scale;
				}
			}
		}

		protected override void OnActionEnd()
		{
			DestroyThis();
		}
	}
}