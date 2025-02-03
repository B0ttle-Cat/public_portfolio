#if UNITY_EDITOR
using System;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

namespace TFContent
{
	[ExecuteInEditMode]
	public class PreviewWorldObjectData : MonoBehaviour
	{
		public Transform previewParent;
		private GameContentLoader gameContentLoader;
		private WorldObjectData WorldData => gameContentLoader == null ? null : gameContentLoader.worldObjectData;
		[SerializeField]
		private bool canEdit;

		private void OnEnable()
		{
			if(EditorApplication.isPlaying) return;
			ClearPreviews();
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
			if(WorldData == null)
			{
				Debug.LogError("CharacterInWorldData가 설정되지 않았습니다.");
				return;
			}
			GameObject sample = null;
			GameObject previewObject = null;
			// 프리뷰 오브젝트 배열 초기화
			sample = WorldData.TerrainObject.LoadAsset();
			if(sample != null)
			{
				previewObject = PrefabUtility.InstantiatePrefab(sample, previewParent) as GameObject;
				if(previewObject != null)
				{
					previewObject.tag = "EditorOnly";
					previewObject.transform.localPosition = Vector3.zero;
					previewObject.transform.localRotation = Quaternion.identity;
					previewObject.transform.localScale = Vector3.one;
					if(!canEdit)
					{
						HideObjectInEditor(previewObject);
						previewObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
						SceneVisibilityManager.instance.DisablePicking(previewObject, true);
					}
				}
			}

			sample = WorldData.PrefabBuilderGroup.LoadAsset();
			if(sample != null)
			{
				previewObject = PrefabUtility.InstantiatePrefab(sample, previewParent) as GameObject;
				if(previewObject != null)
				{
					previewObject.tag = "EditorOnly";
					previewObject.transform.localPosition = Vector3.zero;
					previewObject.transform.localRotation = Quaternion.identity;
					previewObject.transform.localScale = Vector3.one;
					if(!canEdit)
					{
						HideObjectInEditor(previewObject);
						previewObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
						SceneVisibilityManager.instance.DisablePicking(previewObject, true);
					}
				}
			}
			sample = WorldData.OtherObjectGroup.LoadAsset();
			if(sample != null)
			{
				previewObject = PrefabUtility.InstantiatePrefab(sample, previewParent) as GameObject;
				if(previewObject != null)
				{
					previewObject.tag = "EditorOnly";
					previewObject.transform.localPosition = Vector3.zero;
					previewObject.transform.localRotation = Quaternion.identity;
					previewObject.transform.localScale = Vector3.one;
					if(!canEdit)
					{
						HideObjectInEditor(previewObject);
						previewObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
						SceneVisibilityManager.instance.DisablePicking(previewObject, true);
					}
				}
			}
			sample = WorldData.AstarPathObject.LoadAsset();
			if(sample != null)
			{
				previewObject = PrefabUtility.InstantiatePrefab(sample, previewParent) as GameObject;
				if(previewObject != null)
				{
					previewObject.tag = "EditorOnly";
					previewObject.transform.localPosition = Vector3.zero;
					previewObject.transform.localRotation = Quaternion.identity;
					previewObject.transform.localScale = Vector3.one;
					if(!canEdit)
					{
						HideObjectInEditor(previewObject);
						previewObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
						SceneVisibilityManager.instance.DisablePicking(previewObject, true);
					}
				}
			}
			sample = WorldData.FogOfWarObject.LoadAsset();
			if(sample != null)
			{
				previewObject = PrefabUtility.InstantiatePrefab(sample, previewParent) as GameObject;
				if(previewObject != null)
				{
					previewObject.tag = "EditorOnly";
					previewObject.transform.localPosition = Vector3.zero;
					previewObject.transform.localRotation = Quaternion.identity;
					previewObject.transform.localScale = Vector3.one;
					if(!canEdit)
					{
						HideObjectInEditor(previewObject);
						previewObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
						SceneVisibilityManager.instance.DisablePicking(previewObject, true);
					}
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

		void HideObjectInEditor(GameObject previewObject)
		{
			if(previewObject == null) return;
			Transform previewTr = previewObject.transform;
			int childCount = previewTr.childCount;
			for(int c = 0 ; c < childCount ; c++)
			{
				Traverse(previewTr.GetChild(c), (tr) => {
					tr.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.DontSave;
					SceneVisibilityManager.instance.DisablePicking(tr.gameObject, true);
				});
			}
			var components = previewObject.GetComponents<Component>();
			foreach(var component in components)
			{
				if(component is Transform componentTr)
				{
					continue;
				}
				component.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector | HideFlags.DontSave;
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