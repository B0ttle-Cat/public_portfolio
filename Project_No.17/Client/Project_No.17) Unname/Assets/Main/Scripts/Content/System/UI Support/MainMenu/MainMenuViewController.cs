using System;

using TFSystem;
using TFSystem.UI;

using UnityEngine;

namespace TFContent
{
	public enum MainMenuViewState
	{
		None = 0,
		MainView = 1,
		StartGame,
		SaveSlotView,
		CollectionsView,
		NeverEndingPlay,
		Setting,
		ExitGame,
	}
	public class MainMenuViewController : UIViewController<MainMenuViewState>
	{

		protected override void AwakeInController()
		{

		}

		protected override void DestroyInController()
		{

		}

		protected override void StartInController()
		{
		}

		protected override async Awaitable<MainMenuViewState> CheckChangeState(MainMenuViewState viewState)
		{
			try
			{
				if(viewState == MainMenuViewState.StartGame)
				{
					bool change = await ThisSystemState?.ChangeSceneState(ISceneController.SceneState.GameLoadState);
					viewState = change ? MainMenuViewState.None : MainMenuViewState.MainView;
				}
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
				return MainMenuViewState.None;
			}
			return viewState;
		}
	}
}
