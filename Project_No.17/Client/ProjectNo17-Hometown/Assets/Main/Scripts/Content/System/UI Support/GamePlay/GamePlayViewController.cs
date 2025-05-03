using BC.Base;

using TFSystem;
using TFSystem.UI;

using UnityEngine;
namespace TFContent
{
	public enum GamePlayViewState
	{
		None = 0,

		StartGameView = 1, EndGameView = 2,
		PlayGameView = 3, ExitGameView = 4,

		ReloadingGameView = 10,

		OpenMenuView = 20, SaveLoadView = 21, SettingsView = 22,
	}

	public class GamePlayViewController : UIViewController<GamePlayViewState>//, IOdccUpdate
	{
		IUIViewModel gamePlayTopMenuViewModel = null;
		private ObserverValue<float> timeSpeedControl = null;
		protected override void AwakeInController()
		{
			timeSpeedControl = null;
			if(ThisContainer.TryGetComponent<GamePlayTopMenuViewModel>(out var _gamePlayTopMenuViewModel))
			{
				gamePlayTopMenuViewModel = _gamePlayTopMenuViewModel;
			}
		}

		protected override void DestroyInController()
		{
			gamePlayTopMenuViewModel = null;
			if(timeSpeedControl != null)
			{
				timeSpeedControl.Event -= TimeSpeedControl;
				timeSpeedControl = null;
			}
		}

		protected override void StartInController()
		{
			if(gamePlayTopMenuViewModel != null && gamePlayTopMenuViewModel.TryGetEventHandle("timeSpeedControl", out UIEvent_OnChangeValue<float> uiEvent))
			{
				timeSpeedControl = uiEvent.onValueChanged;
				timeSpeedControl.Value = 1f;
				timeSpeedControl.Event += TimeSpeedControl;
			}
		}

		protected override async Awaitable<GamePlayViewState> CheckChangeState(GamePlayViewState viewState)
		{
			TimeSpeedControl(viewState, timeSpeedControl?.Value ?? 1f);
			return viewState;
		}

		private void TimeSpeedControl(float timeSpeed)
		{
			TimeSpeedControl(CurrentViewState, timeSpeed);
		}
		private void TimeSpeedControl(GamePlayViewState viewState, float timeSpeed)
		{
			if(timeSpeedControl == null)
			{
				Time.timeScale = 1f;
				return;
			}

			if(viewState == GamePlayViewState.OpenMenuView
			 || viewState == GamePlayViewState.SaveLoadView
			 || viewState == GamePlayViewState.SettingsView)
			{
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = timeSpeed;
			}
		}
	}
}