using System.Threading;

using UnityEngine;

namespace TFContent
{
	public class DisposeStateSequence : GameStateSequence
	{
		protected override void Start()
		{
			//FindGameSequenceItem<InitSequenceWorld>(out var initWorld);
			//FindGameSequenceItem<InitSequenceCamera>(out var initCamera);
			//FindGameSequenceItem<InitSequenceCharacter>(out var initCharacter);
			//FindGameSequenceItem<InitSequenceCommand>(out var initCommand);
			//FindGameSequenceItem<InitSequenceChapter>(out var initChapter);
		}

		protected override async Awaitable Update(CancellationToken token)
		{
		}

		protected override void End()
		{
		}
	}
}
