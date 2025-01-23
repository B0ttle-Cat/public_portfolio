using BC.OdccBase;

using Sirenix.OdinInspector;

using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class GameContentLoader : OdccBehaviorTarget, IGameContentLoader
	{
		public SaveSlotIO.SlotID currentSlotID;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", nameof(WorldObjectData))]
		public WorldObjectData worldObjectData;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", nameof(CharacterObjectData))]
		public CharacterObjectData characterObjectData;

		[InlineEditor(objectFieldMode:InlineEditorObjectFieldModes.Boxed, Expanded = true), TabGroup("LoadableData", nameof(SaveLoadObjectData))]
		public SaveLoadObjectData saveLoadObjectData;
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
			}
			DataCarrier.DeleteSeparate<GameContentLoader>();
			DataCarrier.GetSeparate<GameContentLoader>().SetData<WorldObjectData>(worldObjectData);
			DataCarrier.GetSeparate<GameContentLoader>().SetData<CharacterObjectData>(characterObjectData);
			DataCarrier.GetSeparate<SaveLoadObjectData>().SetData<SaveLoadObjectData>(saveLoadObjectData);
		}

		protected override void OnActionEnd()
		{
			DestroyThis();

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