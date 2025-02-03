#if UNITY_EDITOR
using System;
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
			if(EditorApplication.isPlaying) return;
			ResetCurrentData();
		}

		private void OnDisable()
		{
			if(EditorApplication.isPlaying) return;
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
				var prefab = data.Prefab.gameObject;
				// 프리뷰 오브젝트 생성
				GameObject preview = PrefabUtility.InstantiatePrefab(prefab, previewParent) as GameObject;
				if(preview != null)
				{
					preview.tag = "EditorOnly";
					var previewTr = preview.transform;
					previewTr.localPosition = data.position;
					previewTr.localRotation = Quaternion.Euler(data.rotation);
					previewTr.localScale = data.scale;

					//preview.hideFlags = HideFlags.DontSave;
					//HideObjectInEditor(preview);
				}
			}
		}

		[ButtonGroup("PreviewButton")]
		private void ClearPreviews()
		{
			if(previewParent == null) return;

			int childCount = previewParent.childCount;
			for(int i = 0 ; i < childCount ; i++)
			{
				//if(previewParent.GetChild(i).gameObject.tag =="EditorOnly")
				{
					DestroyImmediate(previewParent.GetChild(i).gameObject);
					i--;
					childCount--;
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
					if(prefab.TryGetComponent<CharacterObject>(out var character))
					{
						previewData.Add(new CharacterObjectData.PrefabData() {
							prefab = new ResourcesKey<CharacterObject>(prefab),
							position = childTr.localPosition,
							rotation = childTr.localRotation.eulerAngles,
							scale = prefab.transform.localScale
						});
					}
				}
			}
			if(CharacterData == null) return;
			CharacterData.prefabDatas = previewData;
			CharacterData.OnValidate();
			EditorUtility.SetDirty(CharacterData);
			AssetDatabase.SaveAssets();
			ResetCurrentData();
		}

		void HideObjectInEditor(GameObject previewObject)
		{
			if(previewObject == null) return;
			Transform previewTr = previewObject.transform;
			int childCount = previewTr.childCount;
			for(int c = 0 ; c < childCount ; c++)
			{
				Traverse(previewTr.GetChild(c), (tr) => {
					tr.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
				});
			}
			var components = previewObject.GetComponents<Component>();
			foreach(var component in components)
			{
				if(component is Transform componentTr)
				{
					continue;
				}
				component.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
			}

			void Traverse(Transform current, Action<Transform> action)
			{
				action?.Invoke(current);
				foreach(Transform child in current)
				{
					Traverse(child, action);
				}
			}
		}
	}
}
#endif
