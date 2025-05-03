using TFSystem;
using TFSystem.UI;

using UnityEngine;
using UnityEngine.UI;

namespace TFContent
{
	public class GamePlayMenuViewModel : UIViewModelComponent
	{
		[SerializeField] private Button closeMenuButton;
		[Space]
		[SerializeField] private Button restartCheckpointButton;
		[SerializeField] private Button restartGameButton;
		[Space]
		[SerializeField] private Button openSaveAndLoadButton;
		[Space]
		[SerializeField] private Button openGameSettingButton;
		[SerializeField] private Button exitGameButton;
		protected override void AwakeUIView(ref ViewItemSetter viewItemSetter)
		{
			closeMenuButton.onClick.AddListener(async () => await WaitOnAction(CloseMenuButton));

			restartCheckpointButton.onClick.AddListener(async () => await WaitOnAction(RestartCheckpointButton));
			restartGameButton.onClick.AddListener(async () => await WaitOnAction(RestartGameButton));

			openSaveAndLoadButton.onClick.AddListener(async () => await WaitOnAction(OpenSaveAndLoadButton));

			openGameSettingButton.onClick.AddListener(async () => await WaitOnAction(OpenGameSettingButton));
			exitGameButton.onClick.AddListener(async () => await WaitOnAction(ExitGameButton));
		}

		private async Awaitable CloseMenuButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<GamePlayViewState>>(out var view))
			{
				await view.OnCloseViewState(GamePlayViewState.OpenMenuView);
			}
		}
		private async Awaitable RestartCheckpointButton()
		{

		}
		private async Awaitable RestartGameButton()
		{

		}
		private async Awaitable OpenSaveAndLoadButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<GamePlayViewState>>(out var view))
			{
				await view.OnChangeViewState(GamePlayViewState.SaveLoadView);
			}
		}
		private async Awaitable OpenGameSettingButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<GamePlayViewState>>(out var view))
			{
				await view.OnChangeViewState(GamePlayViewState.SettingsView);
			}
		}
		private async Awaitable ExitGameButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<GamePlayViewState>>(out var view))
			{
				await view.OnChangeViewState(GamePlayViewState.ExitGameView);
			}
		}
	}
}
