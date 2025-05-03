using System.Threading;

using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class GamePlaySystem : SystemState
	{
		private GamePlayViewController gamePlayViewController;
		private GameRootSequence gameRootSequence;

		protected override void AwakeOnSystem()
		{
			if(ThisContainer.TryGetComponent<GameRootSequence>(out gameRootSequence))
				gameRootSequence.AwakeOnSystem();

			if(ThisContainer.TryGetChildObject<GamePlayViewController>(out gamePlayViewController))
			{
				gamePlayViewController.gameObject.SetActive(true);
			}
		}
		protected override void DestroyOnSystems()
		{
			if(gameRootSequence != null || ThisContainer.TryGetComponent<GameRootSequence>(out gameRootSequence))
				gameRootSequence.DestroyOnSystems();

			if(gamePlayViewController != null || ThisContainer.TryGetChildObject<GamePlayViewController>(out gamePlayViewController))
			{
				gamePlayViewController.gameObject.SetActive(false);
				gamePlayViewController = null;
			}
		}

		protected override async Awaitable StartWaitSystem(CancellationToken token)
		{
			if(gameRootSequence != null || ThisContainer.TryGetComponent<GameRootSequence>(out gameRootSequence))
				await gameRootSequence.StartWaitSystem(token, OnEndPlayCallbackWithSelf);
		}

		private void OnEndPlayCallbackWithSelf(EndPlayReport endPlayReport)
		{

		}

		protected override async Awaitable EndedWaitSystem(CancellationToken token)
		{
			if(gameRootSequence != null || ThisContainer.TryGetComponent<GameRootSequence>(out gameRootSequence))
				await gameRootSequence.EndedWaitSystem(token);
		}
		public override async Awaitable<bool> ChangeSceneState(ISceneController.SceneState mainMenuState)
		{
			return await base.ChangeSceneState(mainMenuState);
		}
	}
}
