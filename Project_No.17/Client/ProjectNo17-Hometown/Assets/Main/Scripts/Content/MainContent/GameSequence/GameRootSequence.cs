using System;
using System.Threading;

using BC.Base;
using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;
namespace TFContent
{
	public class GameRootSequence : ComponentBehaviour//, IOdccUpdate
	{
		[SerializeReference,ReadOnly]
		private InitStateSequence initStateSequence;
		[SerializeReference,ReadOnly]
		private StartStateSequence startStateSequence;
		[SerializeReference,ReadOnly]
		private PlayStateSequence playStateSequence;
		[SerializeReference,ReadOnly]
		private EndStateSequence endStateSequence;
		[SerializeReference,ReadOnly]
		private DisposeStateSequence disposeStateSequence;

		private CancellationTokenSource playCTS;
		private Action<EndPlayReport> onCallbackEndPlay;
		internal void AwakeOnSystem()
		{
			initStateSequence ??= new InitStateSequence();
			startStateSequence ??= new StartStateSequence();
			playStateSequence ??= new PlayStateSequence();
			endStateSequence ??= new EndStateSequence();
			disposeStateSequence ??= new DisposeStateSequence();
		}
		internal void DestroyOnSystems()
		{
		}
		internal async Awaitable StartWaitSystem(CancellationToken token, Action<EndPlayReport> callbackEndPlay)
		{
			initStateSequence ??= new InitStateSequence();
			initStateSequence.OnStart();
			await initStateSequence.OnUpdate(token);
			initStateSequence.OnEnd();
			initStateSequence = null;

			startStateSequence ??= new StartStateSequence();
			startStateSequence.OnStart();
			await startStateSequence.OnUpdate(token);
			startStateSequence.OnEnd();
			startStateSequence = null;

			onCallbackEndPlay = callbackEndPlay;
			OnPlayGame();
		}
		private async void OnPlayGame()
		{
			playCTS = new CancellationTokenSource();
			CancellationToken playToken = playCTS.Token;

			playStateSequence ??= new PlayStateSequence();
			playStateSequence.OnStart();
			await playStateSequence.OnUpdate(playToken);
			playStateSequence.GetEndPlayReport(out EndPlayReport endPlayReport);
			playStateSequence.OnEnd();
			playStateSequence = null;

			playCTS?.Dispose();
			playCTS = null;

			onCallbackEndPlay?.Invoke(endPlayReport);
			onCallbackEndPlay = null;
		}
		internal async Awaitable EndedWaitSystem(CancellationToken token)
		{
			initStateSequence?.OnEnd();
			startStateSequence?.OnEnd();
			initStateSequence = null;
			startStateSequence = null;

			if(playCTS != null && playStateSequence != null)
			{
				playCTS.Cancel();
				onCallbackEndPlay = null;
				await AwaitableUtility.WaitFalse(() => playStateSequence.IsPlay, token);
				playStateSequence = null;
			}

			endStateSequence ??= new EndStateSequence();
			endStateSequence.OnStart();
			await endStateSequence.OnUpdate(token);
			endStateSequence.OnEnd();
			endStateSequence = null;

			disposeStateSequence ??= new DisposeStateSequence();
			disposeStateSequence.OnStart();
			await disposeStateSequence.OnUpdate(token);
			disposeStateSequence.OnEnd();
			disposeStateSequence = null;
		}
		private async void OnStopGame()
		{

		}
	}
}