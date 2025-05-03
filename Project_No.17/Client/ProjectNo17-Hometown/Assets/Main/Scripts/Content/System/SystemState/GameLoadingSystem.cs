using System.Threading;

using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class GameLoadingSystem : SystemState
	{
		[SerializeField]
		private IUIViewController<GameLoadingViewState> loadingUI;

		protected override void AwakeOnSystem()
		{
			ThisContainer.TryGetChildObject(out loadingUI);
		}
		protected override async Awaitable StartWaitSystem(CancellationToken token)
		{
			if(loadingUI == null) return;
			await loadingUI.OnChangeViewState(GameLoadingViewState.Loading);
		}
		protected override async Awaitable EndedWaitSystem(CancellationToken token)
		{
			if(loadingUI == null) return;
			await loadingUI.OnChangeViewState(GameLoadingViewState.None);
		}
		protected override void DestroyOnSystems()
		{
			loadingUI = null;
		}
	}
}
