using System;
using System.Collections.Generic;
using System.IO;

using BC.Base;

using Sirenix.OdinInspector;

using UnityEngine;

using Object = UnityEngine.Object;

namespace TFContent
{
	[CreateAssetMenu(fileName = "EpisodeObjectData", menuName = "BC/EpisodeObjectData")]
	public class EpisodeObjectData : ScenarioObjectData
	{
		private const string RootPath = "Assets/Main/Resources/ScenarioData";

		[ValueDropdown("GetEpisodeKeys")]
		public string episodeKey;
		public string episodeName;

		[Serializable]
		public struct ChapterKey
		{
			[HideLabel, ReadOnly, HorizontalGroup("ChapterKey")]
			public string Key;
			[HideLabel, HorizontalGroup("ChapterKey")]
			public string Name;

			[HideLabel, HorizontalGroup("ChapterKey")]
			public ChapterObject.ChapterState state;

			[HideLabel]
			public ResourcesKey<ChapterObject> chapter;
#if UNITY_EDITOR
			internal void OnValidate(EpisodeObjectData episodeObject)
			{
				if(string.IsNullOrWhiteSpace(episodeObject.episodeKey))
				{
					chapter.OnValidate(EpisodeObjectData.RootPath);
				}
				else
				{
					chapter.OnValidate(EpisodeObjectData.RootPath + "/" + episodeObject.episodeKey);
				}
				Key = chapter.resourcesName;
			}
#endif
		}

		public List<ChapterKey> chapterObjectKey;

		public ChapterKey? FindPlayingChapter()
		{
			foreach(var data in chapterObjectKey)
			{
				if(data.state == ChapterObject.ChapterState.Playing)
				{
					return data;
				}
			}
			return null;
		}
		public ChapterKey? FindNextChapter()
		{
			foreach(var data in chapterObjectKey)
			{
				if(data.state == ChapterObject.ChapterState.Waiting)
				{
					return data;
				}
			}
			return null;
		}
		public ChapterKey? FindPrevChapter()
		{
			ChapterKey? prev = null;
			foreach(var data in chapterObjectKey)
			{
				if(data.state == ChapterObject.ChapterState.Finished)
				{
					prev = data;
				}
				else
				{
					return prev;
				}
			}
			return null;
		}
		public void ChangeState(string chapterKey, ChapterObject.ChapterState state)
		{
			if(string.IsNullOrWhiteSpace(chapterKey)) return;

			for(int i = 0 ; i<chapterObjectKey.Count ; i++)
			{
				ChapterKey data = chapterObjectKey[i];
				if(data.Key == chapterKey)
				{
					data.state = state;
					chapterObjectKey[i] = data;
					return;
				}
			}
		}
#if UNITY_EDITOR
		public override void OnValidate()
		{
			int length = chapterObjectKey == null ? 0 : chapterObjectKey.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var data = chapterObjectKey[i];
				data.OnValidate(this);
				chapterObjectKey[i] = data;
			}
		}
		private ValueDropdownList<string> GetEpisodeKeys()
		{
			ValueDropdownList<string> list = new ValueDropdownList<string>();
			string path = Path.Combine(Application.dataPath, "Main/Resources/ScenarioData");

			if(Directory.Exists(path))
			{
				string[] folders = Directory.GetDirectories(path);
				foreach(string folder in folders)
				{
					string folderName = Path.GetFileName(folder);
					list.Add(folderName);
				}
			}
			return list;
		}
#endif
		public void SaveAllObject()
		{
			// chapterObjectKey는 ChangeState 를 통해 항상 최신 싱태가 유지되기에 별도로 저장할 필요 없음.

		}
		public async Awaitable LoadAllObject(Transform ThisTransform)
		{
			var chapter = FindPlayingChapter();
			// 재생이 필요한 시나리오가 있음.
			if(chapter.HasValue)
			{
				ChapterObject[] newChapterObjects = await Object.InstantiateAsync<ChapterObject>(chapter.Value.chapter.LoadAsset(), ThisTransform, Vector3.zero, Quaternion.identity);
				int length = newChapterObjects.Length;
				for(int i = 0 ; i < length ; i++)
				{
					newChapterObjects[i].chapterName = chapter.Value.Key;
					newChapterObjects[i].gameObject.name = chapter.Value.Key;
				}
			}
		}
	}
}
