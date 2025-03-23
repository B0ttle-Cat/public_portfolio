using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class MainMenuSaveSlotViewModel : SaveSlotViewModel
	{
		protected override void AwakeUIView(ref ViewItemSetter viewItemSetter)
		{
			base.AwakeUIView(ref viewItemSetter);

			saveGameButton.gameObject.SetActive(false);
		}

		protected override async Awaitable OnCloseSaveSlotButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.MainView);
			}
		}

		protected override async Awaitable OnSaveGameButton()
		{
			if(currentSelectSlotID.slotIndex < 0) return;
		}

		protected override async Awaitable OnStartGameButton()
		{
			if(currentSelectSlotID.slotIndex < 0) return;

			DataCarrier.SetData<SaveSlotIO.SlotID>("CurrentSlotID", currentSelectSlotID);
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.StartGame);
			}
		}
	}
}
