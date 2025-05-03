using System.Threading;

using UnityEngine;

namespace TFContent
{
	public class StartStateSequence : GameStateSequence
	{
		StartGameObjectDataLoad objectDataLoad = null;
		StartSequenceWorld startWorld = null;
		StartSequenceCamera startCamera = null;
		StartSequenceCharacter startCharacter = null;
		StartSequenceCommand startCommand = null;
		StartSequenceEpisode startEpisode = null;
		StartSequenceViewController viewController = null;

		protected override void Start()
		{
			FindGameSequenceItem(out objectDataLoad);
			FindGameSequenceItem(out startWorld);
			FindGameSequenceItem(out startCamera);
			FindGameSequenceItem(out startCharacter);
			FindGameSequenceItem(out startCommand);
			FindGameSequenceItem(out startEpisode);
			FindGameSequenceItem(out viewController);
		}
		protected override async Awaitable Update(CancellationToken token)
		{
			if(await NextWork(objectDataLoad)) return;
			if(await NextWork(startWorld)) return;
			if(await NextWork(startCamera)) return;
			if(await NextWork(startCharacter)) return;
			if(await NextWork(startCommand)) return;
			if(await NextWork(startEpisode)) return;
			if(await NextWork(viewController)) return;
		}

		protected override void End()
		{
			objectDataLoad = null;
			startWorld = null;
			startCamera = null;
			startCharacter = null;
			startCommand = null;
			startEpisode = null;
			viewController = null;
		}
	}
}
