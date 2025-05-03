using System.Threading;

using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFSystem
{
	public abstract class SystemState : ObjectBehaviour
	{
		[ShowInInspector, DisplayAsString, EnableGUI, PropertyOrder(-4), PropertySpace(0, 10)]
		public bool SystemIsReady { get; private set; } = false;
		public SceneState SceneState { get; private set; }

		private CancellationTokenSource attachSceneCTS;
		private CancellationTokenSource detachSceneCTS;
		private CancellationToken attachSceneCancellationToken;
		private CancellationToken detachSceneCancellationToken;

		sealed protected override void BaseAwake()
		{
			SystemIsReady = false;
			if(ApplicationController.AppController == null)
			{
#if UNITY_EDITOR
				// 강제로 ApplicationScene 열기
				UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoad;
				UnityEngine.SceneManagement.SceneManager.LoadScene(SceneController.ApplicationScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
				void SceneLoad(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode arg1)
				{
					if(scene.name != SceneController.ApplicationScene) return;

					UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoad;
					ApplicationController.AppController.EditorOnly_AppStartState = ISceneController.SceneState.NoneState;

					Debug.LogError($"\"{SceneController.ApplicationScene}\"씬을 강제로 로드하였습니다. 시작 씬을 확인해 주세요.");

					AwakeOnSystem();
					AttachSceneState(null);
				}
				return;
#else
				Debug.LogError($"{nameof(ApplicationController)}가 없습니다. 시작 씬이 \"{SceneController.ApplicationScene}\"이 맞는지 확인하세요.");
				return;
#endif
			}
			AwakeOnSystem();
		}
		sealed protected override void BaseDestroy()
		{
			DestroyOnSystems();
			SystemIsReady = false;
		}

		protected abstract void AwakeOnSystem();
		protected abstract void DestroyOnSystems();

		internal void AttachSceneState(SceneState sceneState)
		{
			Async(sceneState);
			async void Async(SceneState sceneState)
			{
				attachSceneCTS = new CancellationTokenSource();
				attachSceneCancellationToken = attachSceneCTS.Token;

				SceneState = sceneState;
				await StartWaitSystem(attachSceneCancellationToken);
				SystemIsReady = true;

				attachSceneCTS.Dispose();
				attachSceneCTS = null;
				attachSceneCancellationToken = CancellationToken.None;
			}
		}
		internal void DetachSceneState()
		{
			Async();
			async void Async()
			{
				detachSceneCTS = new CancellationTokenSource();
				detachSceneCancellationToken = detachSceneCTS.Token;

				if(SceneState == null) return;
				await EndedWaitSystem(detachSceneCancellationToken);
				SystemIsReady = false;
				SceneState = null;

				detachSceneCTS.Dispose();
				detachSceneCTS = null;
				detachSceneCancellationToken = CancellationToken.None;
			}
		}

		protected abstract Awaitable StartWaitSystem(CancellationToken cancellationToken);// { return; }
		protected abstract Awaitable EndedWaitSystem(CancellationToken cancellationToken);// { return; }

		public virtual async Awaitable<bool> ChangeSceneState(ISceneController.SceneState mainMenuState)
		{
			return await ApplicationController.SceneController.ChangeSceneState(mainMenuState);
		}
	}
}
