using BC.Base;

using TFSystem;
using TFSystem.SaveIO;

using UnityEngine;

namespace TFContent
{
	public class MainMenuSaveSlotViewModel : SaveSlotViewModel
	{
		protected override void AwakeUIView(ref ViewItemSetter viewItemSetter)
		{
			base.AwakeUIView(ref viewItemSetter);

			// 메인 메뉴에서는 세이브를 할수 없음.
			saveGameButton.gameObject.SetActive(false);
		}

		protected override async Awaitable OnCloseSaveSlotButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				// 메인 메뉴에서 열린 저장화면을 닫을때는 무조건 MainView 로 이동.
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

			DataCarrier.SetData(ConstString.CurrentEpisodeName, currentSelectSlotID.episodeName);
			DataCarrier.SetData<SaveSlotIO.SlotID>(ConstString.CurrentSlotID, currentSelectSlotID);
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.StartGame);
			}
		}
	}
}
