using System;
using System.Collections.Generic;
using System.IO;

using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

using Object = UnityEngine.Object;

namespace TFContent
{
	[CreateAssetMenu(fileName = "ScenarioStorage", menuName = "BC/ScenarioStorage")]
	public class ScenarioStorage : ScriptableObject
	{
		public List<EpisodeData> episodeDatas;

		[Serializable]
		public struct EpisodeData
		{
			[TitleGroup("Episode", order: - 2)]
			[ValueDropdown("GetSiblingFolderNames")]
			[HideLabel, InlineButton("ReloadEpisodeObjects", "새로고침")]
			public string episodeName;
			[TitleGroup("Episode")]
			[TextArea(1,5), LabelText("Description")]
			public string episodeDescription;

			[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = false)]
			[ListDrawerSettings(DefaultExpandedState = false)]
			public ScenarioObjectData[] episodeObjectData;

#if UNITY_EDITOR
			private void ReloadEpisodeObjects()
			{
				if(Selection.activeObject == null || Selection.activeObject is not ScenarioStorage)
					return;

				string storageAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
				if(string.IsNullOrEmpty(storageAssetPath))
					return;

				string storageDirFullPath = Path.GetDirectoryName(Path.GetFullPath(storageAssetPath));
				if(string.IsNullOrEmpty(storageDirFullPath) || string.IsNullOrEmpty(episodeName))
					return;

				string targetDir = Path.Combine(storageDirFullPath, episodeName);
				if(!Directory.Exists(targetDir))
				{
					Debug.LogWarning($"폴더가 존재하지 않습니다: {targetDir}");
					return;
				}

				string[] assetFiles = Directory.GetFiles(targetDir, "*.asset", SearchOption.AllDirectories);
				List<ScenarioObjectData> loadedObjects = new();

				foreach(string filePath in assetFiles)
				{
					string relativePath = filePath.Substring(filePath.IndexOf("Assets")).Replace("\\", "/");
					Object obj = AssetDatabase.LoadAssetAtPath(relativePath, typeof(ScriptableObject));
					if(obj != null && obj is ScenarioObjectData episodeObj)
					{
						loadedObjects.Add(episodeObj);
					}
				}

				episodeObjectData = loadedObjects.ToArray();

				EditorUtility.SetDirty(Selection.activeObject); // 변경 감지
				AssetDatabase.SaveAssets();
			}
			private IEnumerable<string> GetSiblingFolderNames()
			{
				if(Selection.activeObject == null)
					yield break;

				if(Selection.activeObject is not ScenarioStorage)
					yield break;

				string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
				if(string.IsNullOrEmpty(assetPath))
					yield break;

				string currentDir = Path.GetDirectoryName(Path.GetFullPath(assetPath));
				if(string.IsNullOrEmpty(currentDir))
					yield break;

				string[] directories = Directory.GetDirectories(currentDir);
				foreach(string dir in directories)
				{
					yield return Path.GetFileName(dir);
				}
			}
#endif
			internal void OnValidate()
			{
				if(episodeObjectData == null) episodeObjectData = new ScenarioObjectData[0];
				foreach(ScenarioObjectData data in episodeObjectData)
				{
					if(data != null)
					{
						data.OnValidate();
					}
				}
			}
			internal bool TryGetData<T>(out T setData) where T : ScenarioObjectData
			{
				int length = episodeObjectData == null ? 0 : episodeObjectData.Length;
				for(int i = 0 ; i < length ; i++)
				{
					if(episodeObjectData[i] is T data)
					{
						setData = data;
						return true;
					}
				}
				setData = null;
				return false;
			}
			internal T GetData<T>() where T : ScenarioObjectData
			{
				TryGetData<T>(out T t);
				return t;
			}
		}
		private void OnValidate()
		{
			if(episodeDatas == null)
				episodeDatas = new List<EpisodeData>();
			for(int i = 0 ; i<episodeDatas.Count ; i++)
			{
				EpisodeData data = episodeDatas[i];
				data.OnValidate();
				episodeDatas[i] = data;
			}
		}
		public bool TryGetEpisodeData(string episodeName, out EpisodeData episodeData)
		{
			int index = episodeDatas.FindIndex(data => data.episodeName == episodeName);
			if(index<0)
			{
				Debug.LogError($"EpisodeStorage 에서 Episode({episodeName})를 찾을 수 없습니다.");
				episodeData = default;
				return false;
			}
			episodeData = episodeDatas[index];
			return true;
		}
	}
}
