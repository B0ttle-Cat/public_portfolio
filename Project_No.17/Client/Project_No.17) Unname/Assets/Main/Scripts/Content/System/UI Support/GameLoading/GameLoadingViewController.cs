﻿using TFSystem.UI;

using UnityEngine;

namespace TFContent
{
	public enum GameLoadingViewState
	{
		None = 0,
		Loading = 1,
	}

	public class GameLoadingViewController : UIViewController<GameLoadingViewState>
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

		protected override async Awaitable<GameLoadingViewState> CheckChangeState(GameLoadingViewState viewState)
		{
			return viewState;
		}
	}
}
