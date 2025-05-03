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
		StageSelectView,
		Setting,
		ExitGame,
	}
	public class MainMenuViewController : UIViewController<MainMenuViewState>
	{

		protected override void AwakeInController()
		{
			Application.wantsToQuit += () => {
				return true;
			};
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
				else if(viewState == MainMenuViewState.ExitGame)
				{
#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false;
#else
					Application.Quit();
#endif
					viewState = LastViewStateStack;
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
