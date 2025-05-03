using BC.Base;

using TFSystem;
using TFSystem.SaveIO;
using TFSystem.UI;

using UnityEngine;
using UnityEngine.UI;

namespace TFContent
{
	public class MainButtonViewModel : UIViewModelComponent
	{
		[SerializeField] private Button continueGameButton;
		[SerializeField] private Button startGameButton;
		[SerializeField] private Button saveSlotViewButton;
		[SerializeField] private Button stageSelectButton;
		[SerializeField] private Button collectionsButton;
		[SerializeField] private Button settingButton;
		[SerializeField] private Button exitGameButton;
		protected override void AwakeUIView(ref ViewItemSetter viewItemSetter)
		{
			continueGameButton.onClick.AddListener(async () => await WaitOnAction(OnContinueGameButton));
			startGameButton.onClick.AddListener(async () => await WaitOnAction(OnStartGameButton));
			saveSlotViewButton.onClick.AddListener(async () => await WaitOnAction(OnSaveSlotViewButton));
			stageSelectButton.onClick.AddListener(async () => await WaitOnAction(OnStageSelectButton));
			collectionsButton.onClick.AddListener(async () => await WaitOnAction(OnCollectionsButton));
			settingButton.onClick.AddListener(async () => await WaitOnAction(OnSettingButton));
			exitGameButton.onClick.AddListener(async () => await WaitOnAction(OnExitGameButton));

			MainButtonEnableCheck();

			void MainButtonEnableCheck()
			{
				CheckContinueSlot();
				CheckNeverEndingPlayMode();

				void CheckContinueSlot()
				{
					if(SaveMainIO.SaveKeyValue.TryGetData<SaveSlotIO.SlotID>(ConstString.MainContentKey_ContinueGamePlay, out var continueSlotID))
					{
						if(continueSlotID.slotIndex >= 0)
						{
							Text[] texts = continueGameButton.GetComponentsInChildren<Text>(true);
							texts[1].text = continueSlotID.saveTime;
							texts[2].text = continueSlotID.saveMessage;

							continueGameButton.gameObject.SetActive(true);
							return;
						}
					}
					continueGameButton.gameObject.SetActive(false);
				}

				void CheckNeverEndingPlayMode()
				{
					SaveMainIO.SaveKeyValue.TryGetData(ConstString.MainContentOpenState_NeverEndingPlay, out string ContentState);
					if(string.IsNullOrWhiteSpace(ContentState) || ContentState == ConstString.MainContentState_Close)
					{
						continueGameButton.interactable = false;
						return;
					}
					else if(ContentState == ConstString.MainContentState_Open)
					{
						continueGameButton.interactable = true;
					}
					else if(ContentState == ConstString.MainContentState_FirstOpen)
					{
						SaveMainIO.SaveKeyValue.SetData(ConstString.MainContentOpenState_NeverEndingPlay, ConstString.MainContentState_Open);
						continueGameButton.interactable = true;
					}
				}
			}
		}
		private async Awaitable OnStartGameButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				DataCarrier.DeleteData(ConstString.CurrentSlotID);
				DataCarrier.SetData(ConstString.CurrentEpisodeName, ConstString.EpisodeName_NewGame);
				await view.OnChangeViewState(MainMenuViewState.StartGame);
			}
		}

		private async Awaitable OnContinueGameButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				if(SaveMainIO.SaveKeyValue.TryGetData<SaveSlotIO.SlotID>(ConstString.MainContentKey_ContinueGamePlay, out var continueSlotID))
				{
					DataCarrier.SetData(ConstString.CurrentSlotID, continueSlotID.slotIndex);
					DataCarrier.SetData(ConstString.CurrentEpisodeName, continueSlotID.episodeName);
					await view.OnChangeViewState(MainMenuViewState.StartGame);
				}
			}
		}

		private async Awaitable OnSaveSlotViewButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.SaveSlotView);
			}
		}
		private async Awaitable OnCollectionsButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.CollectionsView);
			}
		}
		private async Awaitable OnStageSelectButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.StageSelectView);
			}
		}
		private async Awaitable OnSettingButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.Setting);
			}
		}

		private async Awaitable OnExitGameButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.ExitGame);
			}
		}
	}
}
