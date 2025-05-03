using System;

using TFSystem;
using TFSystem.SaveIO;
using TFSystem.SaveLoad;

using UnityEngine;

using Object = UnityEngine.Object;

namespace TFContent.SaveLoad
{
	public static class SaveLoadGameManager
	{
		public static async Awaitable SaveGame(int saveSlotID, string saveMessage, bool autoSave = false)
		{
			GamePlaySystem gamePlaySystem =  GameObject.FindAnyObjectByType<GamePlaySystem>();
			if(gamePlaySystem == null) return;

			if(!gamePlaySystem.ThisContainer.TryGetComponent<IGameContentLoader>(out var contentLoader)) return;

			SaveSlotIO.SlotID slotID = new SaveSlotIO.SlotID(autoSave, saveSlotID);

			slotID.saveMessage = string.IsNullOrWhiteSpace(saveMessage) ? "비어 있음" : saveMessage;
			slotID.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			SaveSlotIO.SlotData slotData = new SaveSlotIO.SlotData(saveSlotID);
			ISaveLoadKeyValue iSaveData = slotData;

			WorldObjectData worldObjectData = contentLoader.WorldObjectData == null
				? new ()
				: ScriptableObject.Instantiate(contentLoader.WorldObjectData);
			CharacterObjectData characterObjectData = contentLoader.CharacterObjectData == null
				? new ()
				: ScriptableObject.Instantiate(contentLoader.CharacterObjectData);
			EpisodeObjectData chapterObjectData = contentLoader.EpisodeObjectData == null
				? new ()
				: ScriptableObject.Instantiate(contentLoader.EpisodeObjectData);
			IFFMatchingObjectData iffMatchingObjectData = contentLoader.IFFMatchingObjectData == null
				? new ()
				: ScriptableObject.Instantiate(contentLoader.IFFMatchingObjectData);
			SaveLoadObjectData saveLoadObjectData = contentLoader.SaveLoadObjectData == null
				? new SaveLoadObjectData()
				: ScriptableObject.Instantiate(contentLoader.SaveLoadObjectData);

			worldObjectData.SaveAllObject();
			characterObjectData.SaveAllObject();
			chapterObjectData.SaveAllObject();
			iffMatchingObjectData.SaveAllObject();
			saveLoadObjectData.SaveAllObject();
			iSaveData.SetJsonData<WorldObjectData>(worldObjectData);
			iSaveData.SetJsonData<CharacterObjectData>(characterObjectData);
			iSaveData.SetJsonData<EpisodeObjectData>(chapterObjectData);
			iSaveData.SetJsonData<IFFMatchingObjectData>(iffMatchingObjectData);
			iSaveData.SetData<SaveLoadData>(saveLoadObjectData.rootData);
			saveLoadObjectData.rootData = new SaveLoadData();
			if(iSaveData.TryGetData<SaveLoadData>(out var loadData))
			{
				saveLoadObjectData.rootData = loadData;
			}

			EpisodeObjectData.ChapterKey? chapterKey = chapterObjectData.FindPlayingChapter() ?? chapterObjectData.FindPrevChapter();
			slotID.episodeKey = chapterObjectData.episodeKey;
			slotID.episodeName = chapterObjectData.episodeName;
			slotID.chapterKey = chapterKey?.Key ?? "";
			slotID.chapterName = chapterKey?.Name ?? "";

			Camera camera = CameraSystem.MainCamera;
			await SaveSlotIO.SaveSlot.SaveData(slotID, camera, slotData);

			iSaveData = SaveMainIO.SaveMain;
			await SaveMainIO.SaveMain.SaveAsync();

			Object.Destroy(worldObjectData);
			Object.Destroy(characterObjectData);
			Object.Destroy(chapterObjectData);
			Object.Destroy(iffMatchingObjectData);
			Object.Destroy(saveLoadObjectData);
			worldObjectData = null;
			characterObjectData = null;
			chapterObjectData = null;
			iffMatchingObjectData= null;
			saveLoadObjectData = null;
		}
		public static async Awaitable<ISaveLoadKeyValue> LoadGameSlot(SaveSlotIO.SlotID saveSlotID)
		{
			return await SaveSlotIO.SaveSlot.LoadData(saveSlotID);
		}
		public static void LoadGameContent(ISaveLoadKeyValue iLoadData,
			ref WorldObjectData worldObjectData,
			ref CharacterObjectData characterObjectData,
			ref EpisodeObjectData episodeObjectData,
			ref IFFMatchingObjectData iffMatchingObjectData,
			ref SaveLoadObjectData saveLoadObjectData
			)
		{
			if(worldObjectData == null) worldObjectData = new();
			if(characterObjectData == null) characterObjectData = new();
			if(episodeObjectData == null) episodeObjectData = new();
			if(iffMatchingObjectData == null) iffMatchingObjectData = new();
			if(saveLoadObjectData == null) saveLoadObjectData = new();
			iLoadData.TryGetJsonOverwrite<WorldObjectData>(ref worldObjectData);
			iLoadData.TryGetJsonOverwrite<CharacterObjectData>(ref characterObjectData);
			iLoadData.TryGetJsonOverwrite<EpisodeObjectData>(ref episodeObjectData);
			iLoadData.TryGetJsonOverwrite<IFFMatchingObjectData>(ref iffMatchingObjectData);
			if(iLoadData.TryGetData<SaveLoadData>(out var loadData))
			{
				saveLoadObjectData.rootData = loadData;
			}
		}
	}
}
