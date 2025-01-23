using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class GamePlaySystem : SystemState
	{
		protected override void AwakeOnSystem()
		{

		}
		protected override void DestroyOnSystems()
		{
		}

		protected override async Awaitable StartWaitSystem()
		{
		}

		protected override async Awaitable EndedWaitSystem()
		{

		}
		public override async Awaitable<bool> ChangeSceneState(ISceneController.SceneState mainMenuState)
		{
			return await base.ChangeSceneState(mainMenuState);
		}
	}
}
