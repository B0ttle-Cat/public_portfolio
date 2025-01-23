using System;
using System.Collections.Generic;

using BC.ODCC;

using UnityEngine;

namespace TFSystem
{
	public interface ISceneController : IOdccComponent
	{
		public enum SceneName
		{
			IntroScene          = 0,
			MainMenuScene       = 1,

			SaveGameLoadScene     = 2,

			GameLoadingScene    = 4,
			GamePlayScene       = 5,
			GamePlayUIScene     = 6,
		}
		[Flags]
		public enum SceneNameMask
		{
			Nothing = 0,
			IntroScene          = 1<<SceneName.IntroScene,
			MainMenuScene       = 1<<SceneName.MainMenuScene,
			SaveGameLoadScene   = 1<<SceneName.SaveGameLoadScene,
			GameLoadingScene    = 1<<SceneName.GameLoadingScene,
			GamePlayScene       = 1<<SceneName.GamePlayScene,
			GamePlayUIScene     = 1<<SceneName.GamePlayUIScene,
		}
		public enum SceneState
		{
			NoneState           = 0,
			MainMenuState       = 1,
			GameLoadState       = 2,
			GamePlayState       = 3,
		}
		[Flags]
		public enum SceneStateMask
		{
			AnyState = 0,
			NoneState           = 1 << SceneState.NoneState,
			MainMenuState       = 1 << SceneState.MainMenuState,
			GameLoadState       = 1 << SceneState.GameLoadState,
			GamePlayState       = 1 << SceneState.GamePlayState,
		}
		public SceneState CurrentState { get; }
		public Stack<SceneState> SceneChangeStack { get; }
		public Awaitable<bool> ChangeSceneState(SceneState nextState);
		public void ChangeSceneState(SceneState nextState, Action<bool, SceneState> callback);
	}
}
