using TFSystem;

using UnityEngine;
namespace TFContent
{
	public class InitSequenceEpisode : GameSequenceItem
	{
		private EpisodeObjectData episodeObjectData;

		protected override bool OnActionValid()
		{
			episodeObjectData = DataCarrier.GetSeparate<InitGameContentLoader>().GetData<EpisodeObjectData>(episodeObjectData);
			return episodeObjectData != null;
		}

		protected override async Awaitable OnActionUpdate()
		{
			await episodeObjectData.LoadAllObject(ThisTransform);
		}

		protected override void OnActionEnd()
		{
			episodeObjectData = null;
			DestroyThis();
		}
	}
}