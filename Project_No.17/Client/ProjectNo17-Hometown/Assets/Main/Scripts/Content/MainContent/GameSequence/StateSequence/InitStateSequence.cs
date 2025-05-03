using System.Threading;

using UnityEngine;

namespace TFContent
{
	public class InitStateSequence : GameStateSequence
	{
		InitGameContentLoader contentLoader = null;
		InitSequenceWorld initWorld = null;
		InitSequenceCamera initCamera = null;
		InitSequenceCharacter initCharacter = null;
		InitSequenceCommand initCommand = null;
		InitSequenceEpisode initEpisode = null;
		InitSequenceViewController viewController = null;
		protected override void Start()
		{
			FindGameSequenceItem(out contentLoader);
			FindGameSequenceItem(out initWorld);
			FindGameSequenceItem(out initCamera);
			FindGameSequenceItem(out initCharacter);
			FindGameSequenceItem(out initCommand);
			FindGameSequenceItem(out initEpisode);
			FindGameSequenceItem(out viewController);
		}
		protected override async Awaitable Update(CancellationToken token)
		{
			if(await NextWork(contentLoader)) return;
			if(await NextWork(initWorld)) return;
			if(await NextWork(initCamera)) return;
			if(await NextWork(initCharacter)) return;
			if(await NextWork(initCommand)) return;
			if(await NextWork(initEpisode)) return;
			if(await NextWork(viewController)) return;
		}
		protected override void End()
		{
			contentLoader = null;
			initWorld = null;
			initCamera = null;
			initCharacter = null;
			initCommand = null;
			initEpisode = null;
			viewController = null;
		}
	}
}
