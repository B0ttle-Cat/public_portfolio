using BC.Base;

using Sirenix.OdinInspector;

using TFSystem;
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
		[SerializeField] private Button neverEndingPlayMode;
		[SerializeField] private Button collectionsButton;
		[SerializeField] private Button settingButton;
		[SerializeField] private Button exitGameButton;
		[SerializeField, ReadOnly]
		private bool onClick;

		protected override void AwakeUIView(ref ViewItemSetter viewItemSetter)
		{
			onClick = false;
			continueGameButton.onClick.AddListener(async () => await BlockOnClick(OnContinueGameButton()));
			startGameButton.onClick.AddListener(async () => await BlockOnClick(OnStartGameButton()));
			saveSlotViewButton.onClick.AddListener(async () => await BlockOnClick(OnSaveSlotViewButton()));
			neverEndingPlayMode.onClick.AddListener(async () => await BlockOnClick(OnNeverEndingPlayMode()));
			collectionsButton.onClick.AddListener(async () => await BlockOnClick(OnCollectionsButton()));
			settingButton.onClick.AddListener(async () => await BlockOnClick(OnSettingButton()));
			exitGameButton.onClick.AddListener(async () => await BlockOnClick(OnExitGameButton()));

			MainButtonEnableCheck();

			void MainButtonEnableCheck()
			{
				CheckContinueSlot();
				CheckNeverEndingPlayMode();

				void CheckContinueSlot()
				{
					string continueSlotData = MainSaveIO.MainSave[MainSaveIO.ContinueSlot];
					if(continueSlotData.IsNotNullOrWhiteSpace())
					{
						var continueSlotID = JsonUtility.FromJson<SaveSlotIO.SlotID>(continueSlotData);
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
					string ContentState = MainSaveIO.MainSave[MainSaveIO.ContentState_NeverEndingPlay];
					if(string.IsNullOrWhiteSpace(ContentState) || ContentState == MainSaveIO.ContentState_Close)
					{
						continueGameButton.interactable = false;
						return;
					}
					else if(ContentState == MainSaveIO.ContentState_Open)
					{
						continueGameButton.interactable = true;
					}
					else if(ContentState == MainSaveIO.ContentState_FirstOpen)
					{
						MainSaveIO.MainSave[MainSaveIO.ContentState_NeverEndingPlay] = MainSaveIO.ContentState_Open;
						continueGameButton.interactable = true;
					}
				}
			}

			async Awaitable BlockOnClick(Awaitable awaitable)
			{
				if(onClick) return;
				onClick = true;
				await awaitable;
				onClick = false;
			}
		}
		private async Awaitable OnStartGameButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.StartGame);
			}
		}

		private async Awaitable OnContinueGameButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.StartGame);
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
		private async Awaitable OnNeverEndingPlayMode()
		{
			if(ThisContainer.TryGetObject<IUIViewController<MainMenuViewState>>(out var view))
			{
				await view.OnChangeViewState(MainMenuViewState.NeverEndingPlay);
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
