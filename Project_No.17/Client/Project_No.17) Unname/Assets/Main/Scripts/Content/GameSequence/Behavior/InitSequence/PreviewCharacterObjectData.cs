#if UNITY_EDITOR
using System.Collections.Generic;

using BC.Base;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace TFContent
{
	[ExecuteInEditMode]
	public class PreviewCharacterObjectData : MonoBehaviour
	{
		public Transform previewParent;
		private GameContentLoader gameContentLoader;
		private CharacterObjectData CharacterData => gameContentLoader == null ? null : gameContentLoader.characterObjectData;
		private void OnEnable()
		{
			ResetCurrentData();
		}

		private void OnDisable()
		{
			ClearPreviews();
		}

		[ButtonGroup("PreviewButton")]
		private void ResetCurrentData()
		{
			if(previewParent == null) return;

			ClearPreviews();

			gameContentLoader = GetComponent<GameContentLoader>();
			if(CharacterData == null || CharacterData.prefabDatas == null)
			{
				Debug.LogError("CharacterInWorldData가 설정되지 않았습니다.");
				return;
			}

			// 프리뷰 오브젝트 배열 초기화
			var prefabData = CharacterData.prefabDatas;
			for(int i = 0 ; i < prefabData.Count ; i++)
			{
				var data = prefabData[i];
				// 프리뷰 오브젝트 생성
				GameObject preview = PrefabUtility.InstantiatePrefab(data.Prefab, previewParent) as GameObject;
				if(preview != null)
				{
					preview.tag = "EditorOnly";
					var previewTr = preview.transform;
					previewTr.localPosition = data.position;
					previewTr.localRotation = Quaternion.Euler(data.rotation);
					previewTr.localScale = data.scale;

					int childCount = previewTr.childCount;
					for(int c = 0 ; c < childCount ; c++)
					{
						previewTr.GetChild(c).gameObject.hideFlags = HideFlags.HideInHierarchy;
					}
					var components = preview.GetComponents<Component>();
					foreach(var component in components)
					{
						if(component is Transform componentTr)
						{
							continue;
						}
						component.hideFlags = HideFlags.HideInInspector;
					}
					previewTr.hideFlags = HideFlags.DontSave;
				}
			}
		}

		private void ClearPreviews()
		{
			if(previewParent == null) return;

			int childCount = previewParent.childCount;
			for(int i = 0 ; i < childCount ; i++)
			{
				var childTr = previewParent.GetChild(0);
				var childObj = childTr.gameObject;

				if(PrefabUtility.GetPrefabInstanceStatus(childObj) == PrefabInstanceStatus.Connected)
				{
					DestroyImmediate(childObj);
				}
			}
		}

		[ButtonGroup("PreviewButton")]
		public void SaveCurrentData()
		{
			if(previewParent == null) return;

			var previewData = new List<CharacterObjectData.PrefabData>();

			int childCount = previewParent.childCount;
			for(int i = 0 ; i < childCount ; i++)
			{
				var childTr = previewParent.GetChild(i);
				var childObj = childTr.gameObject;

				if(PrefabUtility.GetPrefabInstanceStatus(childObj) == PrefabInstanceStatus.Connected)
				{
					GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(childObj);
					previewData.Add(new CharacterObjectData.PrefabData() {
						prefab = new ResourcesKey<GameObject>(prefab),
						position = childTr.localPosition,
						rotation = childTr.localRotation.eulerAngles,
						scale = prefab.transform.localScale
					});
				}
			}

			if(CharacterData == null) return;
			CharacterData.prefabDatas = previewData;
			CharacterData.OnValidate();
			EditorUtility.SetDirty(CharacterData);
			AssetDatabase.SaveAssets();
			ResetCurrentData();
		}
	}
}
#endif
