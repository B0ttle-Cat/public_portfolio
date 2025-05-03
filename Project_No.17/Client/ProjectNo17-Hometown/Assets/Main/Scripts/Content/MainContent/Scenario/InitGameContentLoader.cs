using System.Linq;

using BC.Base;

using Sirenix.OdinInspector;

using TFContent.SaveLoad;

using TFSystem;
using TFSystem.SaveIO;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace TFContent
{
	public class InitGameContentLoader : GameSequenceItem, IGameContentLoader
	{
#if UNITY_EDITOR
		private const string FromSaveSlotInEditor = "From Save Slot In Editor";
#endif
		public SaveSlotIO.SlotID currentSlotID;
		[ShowIf("@episodeStorage == null")]
		[InfoBox("EpisodeStorage가 없습니다. EpisodeStorage를 설정해주세요.", InfoMessageType.Error)]
		public ScenarioStorage episodeStorage;
		[ValueDropdown("EpisodeStorageNameList")]
		[InlineButton("ClearCurrentEpisodeData", "Clear")]
		[InlineButton("Editor_SetEpisodeStorage", "Set This")]
		[ShowIf("@episodeStorage != null")]
		public string currentEpisodeKey;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", "World", TabLayouting = TabLayouting.Shrink)]
		[ShowIf("@episodeStorage != null")]
		public WorldObjectData worldObjectData;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", "Character", TabLayouting = TabLayouting.Shrink)]
		[ShowIf("@episodeStorage != null")]
		public CharacterObjectData characterObjectData;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", "Episode", TabLayouting = TabLayouting.Shrink)]
		[ShowIf("@episodeStorage != null")]
		public EpisodeObjectData episodeObjectData;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", "IFFMatching", TabLayouting = TabLayouting.Shrink)]
		[ShowIf("@episodeStorage != null")]
		public IFFMatchingObjectData iffMatchingObjectData;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", "SaveLoad", TabLayouting = TabLayouting.Shrink)]
		[ShowIf("@episodeStorage != null")]
		public SaveLoadObjectData saveLoadObjectData;

		WorldObjectData IGameContentLoader.WorldObjectData => worldObjectData;
		CharacterObjectData IGameContentLoader.CharacterObjectData => characterObjectData;
		EpisodeObjectData IGameContentLoader.EpisodeObjectData => episodeObjectData;
		IFFMatchingObjectData IGameContentLoader.IFFMatchingObjectData => iffMatchingObjectData;
		SaveLoadObjectData IGameContentLoader.SaveLoadObjectData => saveLoadObjectData;

		protected override bool OnActionValid()
		{
			return true;
		}

		private void OnValidate()
		{
			if(worldObjectData) worldObjectData.OnValidate();
			if(characterObjectData) characterObjectData.OnValidate();
			if(episodeObjectData) episodeObjectData.OnValidate();
			if(iffMatchingObjectData) iffMatchingObjectData.OnValidate();
			if(saveLoadObjectData) saveLoadObjectData.OnValidate();
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(DataCarrier.TryGetData<SaveSlotIO.SlotID>(ConstString.CurrentSlotID, out var tempSlotID))
			{
				// 일반적인 Save Slot 에서 로드하는 경우.
				currentSlotID = tempSlotID;
				if(await LoadFromSaveSlot())
				{
					SetupScenarioObject();
					return;
				}
				// SaveSlot 에서 로드하지 못한경우 다음 로드방벙을 고려함.
			}
			if(DataCarrier.TryGetData(ConstString.CurrentEpisodeName, out string tempEpisodeKey))
			{
				// 일반적인 게임을 시작하는 경우.
				currentEpisodeKey = tempEpisodeKey;
				LoadFromEpisodeStorage();
				SetupScenarioObject();
				return;
			}
#if UNITY_EDITOR
			if(FromSaveSlotInEditor.Equals(currentEpisodeKey))
			{
				// 가능한 경우, 인스펙터에 지정된 currentSlotID 로 게임을 로드함.
				if(await LoadFromSaveSlot())
				{
					SetupScenarioObject();
					return;
				}
			}
#endif
			// 최후의 수단. 가능한 경우, 인스펙터에 지정된 currentEpisodeKey 로 게임을 로드함.
			// 사실상 게임 로드 및 실행 실패임
			Debug.LogError("올바른 ConstString.CurrentSlotID 또는 ConstString.CurrentEpisodeName 으로부터 게임을 시작하지 못했습니다. 필요한 값을 확인해 주세요." +
				$"\n ConstString.CurrentSlotID: {ConstString.CurrentSlotID}" +
				$"\n ConstString.CurrentEpisodeName: {ConstString.CurrentEpisodeName}");
			LoadFromEpisodeStorage();
			SetupScenarioObject();
		}
		private async Awaitable<bool> LoadFromSaveSlot()
		{
			ISaveLoadKeyValue slotData = await SaveLoadGameManager.LoadGameSlot(currentSlotID);
			if(slotData != null)
			{
				SaveLoadGameManager.LoadGameContent(slotData,
					ref worldObjectData,
					ref characterObjectData,
					ref episodeObjectData,
					ref iffMatchingObjectData,
					ref saveLoadObjectData
				);
				currentEpisodeKey = episodeObjectData.episodeKey;
				return true;
			}
			return false;
		}
#if UNITY_EDITOR
		private async Awaitable Editor_SetEpisodeStorage()
		{
			if(FromSaveSlotInEditor.Equals(currentEpisodeKey))
			{
				SaveGameFileIO.Init();
				// 가능한 경우, 인스펙터에 지정된 currentSlotID 로 게임을 로드함.
				if(await LoadFromSaveSlot())
				{
					SetupScenarioObject();
				}
				else
				{
					LoadFromEpisodeStorage();
					SetupScenarioObject();
				}
				SaveGameFileIO.Dispose();
				currentEpisodeKey= FromSaveSlotInEditor;
			}
			else
			{
				LoadFromEpisodeStorage();
				SetupScenarioObject();
			}
		}
#endif
		private void LoadFromEpisodeStorage()
		{
			if(episodeStorage != null && episodeStorage.TryGetEpisodeData(currentEpisodeKey, out ScenarioStorage.EpisodeData loadData))
			{
				worldObjectData = loadData.GetData<WorldObjectData>();
				characterObjectData = loadData.GetData<CharacterObjectData>();
				episodeObjectData = loadData.GetData<EpisodeObjectData>();
				iffMatchingObjectData = loadData.GetData<IFFMatchingObjectData>();
				saveLoadObjectData = loadData.GetData<SaveLoadObjectData>();
			}
		}
		private void SetupScenarioObject()
		{
			DataCarrier.DeleteSeparate<InitGameContentLoader>();
			DataCarrier.GetSeparate<InitGameContentLoader>().SetData<WorldObjectData>(worldObjectData);
			DataCarrier.GetSeparate<InitGameContentLoader>().SetData<CharacterObjectData>(characterObjectData);
			DataCarrier.GetSeparate<InitGameContentLoader>().SetData<EpisodeObjectData>(episodeObjectData);
			DataCarrier.GetSeparate<InitGameContentLoader>().SetData<IFFMatchingObjectData>(iffMatchingObjectData);
			DataCarrier.GetSeparate<InitGameContentLoader>().SetData<SaveLoadObjectData>(saveLoadObjectData);
		}
		private void ClearCurrentEpisodeData()
		{
			worldObjectData = null;
			characterObjectData = null;
			episodeObjectData = null;
			iffMatchingObjectData = null;
			saveLoadObjectData = null;
		}

		protected override void OnActionEnd()
		{
		}


		protected override void BaseDestroy()
		{
			ClearCurrentEpisodeData();
			episodeStorage = null;
		}

#if UNITY_EDITOR
		[HideIf("NoPreview")]
		[Button]
		[PropertyOrder(-50)]
		public void CreatePreviewCharacterComponent()
		{
			gameObject.AddComponent<PreviewCharacterObjectData>();
		}
		private bool NoPreview()
		{
			return TryGetComponent<PreviewCharacterObjectData>(out _);
		}
		private ValueDropdownList<string> EpisodeStorageNameList()
		{
			ValueDropdownList<string> list = new ValueDropdownList<string>();
			if(episodeStorage == null) return list;
			if(episodeStorage.episodeDatas == null) return list;
			var nameList = episodeStorage.episodeDatas.Select(i => i.episodeName);
			foreach(var item in nameList)
			{
				list.Add(item);
			}
			list.Add(FromSaveSlotInEditor);
			return list;
		}
#endif
	}
}