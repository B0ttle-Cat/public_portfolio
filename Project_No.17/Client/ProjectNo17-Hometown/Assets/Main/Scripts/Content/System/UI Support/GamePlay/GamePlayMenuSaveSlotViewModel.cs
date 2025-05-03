using BC.Base;

using TFContent.SaveLoad;

using TFSystem;
using TFSystem.SaveIO;

using UnityEngine;

namespace TFContent
{
	public class GamePlayMenuSaveSlotViewModel : SaveSlotViewModel
	{
		[SerializeField]
		private GameObject saveBlockRaycast;
		protected override void AwakeUIView(ref ViewItemSetter viewItemSetter)
		{
			base.AwakeUIView(ref viewItemSetter);
		}

		protected override Awaitable OnShowUIView()
		{
			saveBlockRaycast.SetActive(false);
			return base.OnShowUIView();
		}


		protected override async Awaitable OnCloseSaveSlotButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<GamePlayViewState>>(out var view))
			{
				await view.OnChangeViewState(view.LastViewStateStack);
			}
		}

		protected override async Awaitable OnSaveGameButton()
		{
			if(currentSelectSlotID.slotIndex < 0) return;
			saveBlockRaycast.gameObject.SetActive(true);
			DataCarrier.DeleteData(ConstString.CurrentEpisodeName);
			DataCarrier.SetData<SaveSlotIO.SlotID>(ConstString.CurrentSlotID, currentSelectSlotID);
			if(ThisContainer.TryGetObject<IUIViewController<GamePlayViewState>>(out var view))
			{
				await SaveLoadGameManager.SaveGame(currentSelectSlotID.slotIndex, "", false);
			}
			saveBlockRaycast.gameObject.SetActive(false);
		}

		protected override async Awaitable OnStartGameButton()
		{
			if(currentSelectSlotID.slotIndex < 0) return;

			DataCarrier.SetData(ConstString.CurrentEpisodeName, currentSelectSlotID.episodeName);
			DataCarrier.SetData<SaveSlotIO.SlotID>(ConstString.CurrentSlotID, currentSelectSlotID);
			if(ThisContainer.TryGetObject<IUIViewController<GamePlayViewState>>(out var view))
			{
				await view.OnChangeViewState(GamePlayViewState.ReloadingGameView);
			}
		}
	}
}
