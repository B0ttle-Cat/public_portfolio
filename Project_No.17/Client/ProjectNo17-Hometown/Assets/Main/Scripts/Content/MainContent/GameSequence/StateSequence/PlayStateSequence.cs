using System.Collections.Generic;
using System.Threading;

using UnityEngine;

namespace TFContent
{
	public class PlayStateSequence : GameStateSequence
	{
		private PlaySequenceViewController viewController;

		private Queue<GamePlayChain> playChainRecord;
		public GamePlayChain currentPlayChain;
		public EndPlayReport endPlayReport;
		protected override void Start()
		{
			playChainRecord = new Queue<GamePlayChain>();
			currentPlayChain = null;
			endPlayReport = null;

			FindGameSequenceItem(out viewController);
			//FindGameSequenceItem<InitSequenceWorld>(out var initWorld);
			//FindGameSequenceItem<InitSequenceCamera>(out var initCamera);
			//FindGameSequenceItem<InitSequenceCharacter>(out var initCharacter);
			//FindGameSequenceItem<InitSequenceCommand>(out var initCommand);
			//FindGameSequenceItem<InitSequenceChapter>(out var initChapter);
		}
		protected override async Awaitable Update(CancellationToken token)
		{
			if(await NextWork(viewController)) return;

			endPlayReport = await WaitEndGamePlay(token);
		}

		private async Awaitable<EndPlayReport> WaitEndGamePlay(CancellationToken token)
		{
			//종료하기 위한 조건을 여기에...
			do
			{
				if(token.IsCancellationRequested)
				{
					// Token 에 의해 동료됨
					// 게임을 마무리하지 않고 수동으로 종료한 케이스
					return null;
				}

				// 종료 조건이 없음. 즉 사용자가 수동으로 종료하기 전 까지는 계속 진행함.
				if(currentPlayChain == null)
				{
					await Awaitable.NextFrameAsync();
					continue;
				}

				if(currentPlayChain is GamePlayExitChain)
				{
					// 종료 체인에 도달한 경우 게임 종료
					break;
				}

				if(currentPlayChain is GamePlayStepChain stepChain)
				{
					if(stepChain.exitCondition == null || stepChain.exitCondition())
					{
						// 다음 체인으로 이동
						var nextChain = stepChain.nextPlayChain;
						int nextChainCount = nextChain == null ? 0 : nextChain.Count;
						if(nextChainCount == 0)
						{
							// 다음 체인이 존재하지 않음 => 종료 조건을 만족한 것으로 간주
							break;
						}
						bool NextIsNull = true;
						for(int i = 0 ; i < nextChainCount ; i++)
						{
							if(nextChain[i] != null)
							{
								NextIsNull = false;
								if((nextChain[i].enterCondition == null || nextChain[i].enterCondition()))
								{
									playChainRecord.Enqueue(currentPlayChain);
									currentPlayChain = nextChain[i];
									break;
								}
							}
						}
						if(NextIsNull)
						{
							// Null이 아닌 다음 체인이 없음 => 종료 조건을 만족한 것으로 간주
							break;
						}
					}
				}
				await Awaitable.NextFrameAsync();
			}
			while(true);

			return new EndPlayReport();
		}

		protected override void End()
		{
			viewController = null;

			endPlayReport = null;
			if(playChainRecord != null)
			{
				foreach(var chain in playChainRecord)
				{
					chain.Dispose();
				}
				playChainRecord.Clear();
				playChainRecord = null;
			}
			if(currentPlayChain != null)
			{
				currentPlayChain.Dispose();
				currentPlayChain = null;
			}
		}

		internal void GetEndPlayReport(out EndPlayReport endPlayReport)
		{
			endPlayReport = this.endPlayReport;
		}
	}
}
