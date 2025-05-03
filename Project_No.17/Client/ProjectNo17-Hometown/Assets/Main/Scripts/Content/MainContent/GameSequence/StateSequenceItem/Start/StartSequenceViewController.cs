using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class StartSequenceViewController : GameSequenceItem
	{
		private IUIViewController<GamePlayViewState> viewController;

		protected override bool OnActionValid()
		{
			if(ThisContainer.TryGetObject<GamePlayViewController>(out var _viewController))
			{
				viewController = _viewController;
				return true;
			}
			else
			{
				viewController = null;
				return false;
			}
		}

		protected override async Awaitable OnActionUpdate()
		{
			await viewController.OnChangeViewState(GamePlayViewState.StartGameView);
		}

		protected override void OnActionEnd()
		{
			viewController = null;
			DestroyThis();
		}
	}
}
