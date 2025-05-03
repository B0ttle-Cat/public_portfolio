#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

using BC.AIProvider;
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
		private InitGameContentLoader gameContentLoader;
		private CharacterObjectData CharacterData => gameContentLoader == null ? null : gameContentLoader.characterObjectData;

		[SerializeField]
		private bool autoPreview;
		[SerializeField]
		private bool canEdit;

		private void OnEnable()
		{
			if(EditorApplication.isPlaying) return;
			if(!autoPreview) return;
			ResetCurrentData();
		}

		private void OnDisable()
		{
			if(EditorApplication.isPlaying) return;
			if(!autoPreview) return;
			ClearPreviews();
		}

		[ButtonGroup("PreviewButton")]
		private void ResetCurrentData()
		{
			if(previewParent == null) return;

			ClearPreviews();

			gameContentLoader = GetComponent<InitGameContentLoader>();
			if(CharacterData == null || CharacterData.prefabDatas == null)
			{
				Debug.LogError("CharacterInWorldData가 설정되지 않았습니다.");
				return;
			}

			// 프리뷰 오브젝트 배열 초기화
			var prefabDatas = CharacterData.prefabDatas;
			var instanceDatas = CharacterData.instanceDatas;
			for(int i = 0 ; i < instanceDatas.Count ; i++)
			{
				var instanceData = instanceDatas[i];
				if(instanceData.unused) continue;
				if(instanceData.prefabIndex < 0) continue;
				int prefabIndex = instanceData.prefabIndex % prefabDatas.Count;
				var data = prefabDatas[prefabIndex];
				var prefab = data.PrefabObject;
				// 프리뷰 오브젝트 생성
				GameObject preview = PrefabUtility.InstantiatePrefab(prefab, previewParent) as GameObject;
				if(preview != null)
				{
					preview.tag = "EditorOnly";

					CharacterObject characterObject = preview.GetComponentInChildren<CharacterObject>();
					characterObject.prefabIndex = prefabIndex;

					var previewTr = characterObject.ThisTransform;
					previewTr.localPosition = instanceData.position;
					previewTr.localRotation = Quaternion.Euler(instanceData.rotation);
					previewTr.localScale = Vector3.one;

					if(!canEdit)
					{
						preview.hideFlags = HideFlags.DontSave;
						HideObjectInEditor(preview);
					}
					AbilityProvider abilityProvider = preview.GetComponentInChildren<AbilityProvider>();
					if(abilityProvider != null)
					{
						abilityProvider.SetAbilityStruct(instanceData.abilityStruct.NewCopy());
					}
					IFFProvider iffProvider = preview.GetComponentInChildren<IFFProvider>();
					if(iffProvider != null)
					{
						iffProvider.TeamID = instanceData.iffTeamID;
						iffProvider.UnitID = instanceData.iffUnitID;
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

		[ButtonGroup("PreviewButton")]
		public void SaveCurrentData()
		{
			if(previewParent == null) return;

			var hashPrefab = new List<GameObject>();
			var previewDatas = new List<CharacterObjectData.PrefabData>();
			var instanceDatas = new List<CharacterObjectData.InstanceData>();
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
						int index = hashPrefab.FindIndex(i=>i==prefab);
						if(index < 0)
						{
							previewDatas.Add(new CharacterObjectData.PrefabData() {
								prefab = new ResourcesKey<CharacterObject>(prefab),
							});
						}
						instanceDatas.Add(new CharacterObjectData.InstanceData() {
							unused = false,
							prefabIndex = index,
							position = childTr.localPosition,
							rotation = childTr.localRotation.eulerAngles,

							iffTeamID = childObj.GetComponentInChildren<IFFProvider>()?.TeamID ?? -1,
							abilityStruct = childObj.GetComponentInChildren<AbilityProvider>()?.AbilityStruct?.NewCopy()
						});
					}
				}
			}
			instanceDatas.AddRange(CharacterData.instanceDatas.Where(i => i.unused));
			gameContentLoader = GetComponent<InitGameContentLoader>();
			if(CharacterData == null) return;
			CharacterData.prefabDatas = previewDatas;
			CharacterData.instanceDatas = instanceDatas;
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
