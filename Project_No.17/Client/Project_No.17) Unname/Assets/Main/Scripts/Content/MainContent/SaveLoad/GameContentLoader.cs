using BC.AISensor;
using BC.OdccBase;

using Sirenix.OdinInspector;

using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class GameContentLoader : OdccBehaviorTarget, IGameContentLoader
	{
		public SaveSlotIO.SlotID currentSlotID;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", "World", TabLayouting = TabLayouting.Shrink)]
		public WorldObjectData worldObjectData;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", "Character", TabLayouting = TabLayouting.Shrink)]
		public CharacterObjectData characterObjectData;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", "SaveLoad", TabLayouting = TabLayouting.Shrink)]
		public SaveLoadObjectData saveLoadObjectData;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", "Scenario", TabLayouting = TabLayouting.Shrink)]
		public ScenarioObjectData scenarioObjectData;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", "IFFMatching", TabLayouting = TabLayouting.Shrink)]
		public IFFMatchingInfoData iffMatchingInfoData;
		protected override bool OnActionValid()
		{
			return true;
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(DataCarrier.TryPopData<SaveSlotIO.SlotID>("CurrentSlotID", out currentSlotID))
			{
				SaveSlotIO.SlotData slotData = await SaveSlotIO.SaveSlot.LoadData(currentSlotID);

				worldObjectData = slotData.GetData<WorldObjectData>();
				characterObjectData = slotData.GetData<CharacterObjectData>();
				saveLoadObjectData = slotData.GetData<SaveLoadObjectData>();
				scenarioObjectData = slotData.GetData<ScenarioObjectData>();
				iffMatchingInfoData = slotData.GetData<IFFMatchingInfoData>();
			}
			DataCarrier.DeleteSeparate<GameContentLoader>();
			DataCarrier.GetSeparate<GameContentLoader>().SetData<WorldObjectData>(worldObjectData);
			DataCarrier.GetSeparate<GameContentLoader>().SetData<CharacterObjectData>(characterObjectData);
			DataCarrier.GetSeparate<SaveLoadObjectData>().SetData<SaveLoadObjectData>(saveLoadObjectData);
			DataCarrier.GetSeparate<ScenarioObjectData>().SetData<ScenarioObjectData>(scenarioObjectData);
			DataCarrier.GetSeparate<ScenarioObjectData>().SetData<IFFMatchingInfoData>(iffMatchingInfoData);
		}

		protected override void OnActionEnd()
		{
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
#endif
	}
}