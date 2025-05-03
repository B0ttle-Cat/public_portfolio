using System.Threading;

using UnityEngine;

namespace TFContent
{
	public class EndStateSequence : GameStateSequence
	{
		private EndSequenceViewController viewController;

		protected override void Start()
		{
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
		}

		protected override void End()
		{
			viewController = null;
		}
	}
}
