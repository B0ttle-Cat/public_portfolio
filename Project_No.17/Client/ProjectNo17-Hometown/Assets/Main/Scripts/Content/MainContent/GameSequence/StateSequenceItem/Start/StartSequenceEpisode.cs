using TFSystem;

using UnityEngine;
namespace TFContent
{
	public class StartSequenceEpisode : GameSequenceItem
	{
		private EpisodePlayer episodePlayer;

		protected override bool OnActionValid()
		{
			return ThisContainer.TryGetComponent<EpisodePlayer>(out episodePlayer);
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(episodePlayer != null)
			{
				episodePlayer.chapterObjectData = DataCarrier.GetSeparate<InitGameContentLoader>().GetData<EpisodeObjectData>();
				await episodePlayer.PlayToStop();
			}
		}

		protected override void OnActionEnd()
		{
			episodePlayer = null;
			DestroyThis();
		}
	}
}