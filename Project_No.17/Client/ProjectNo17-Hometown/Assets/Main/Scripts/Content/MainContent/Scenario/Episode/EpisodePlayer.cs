using System;

using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;
namespace TFContent
{
	public class EpisodePlayer : ComponentBehaviour//, IOdccUpdate
	{
		[ReadOnly]
		public EpisodeObjectData chapterObjectData;
		private ChapterObject currentPlayObject;

		private void ChangeCurrentState(ChapterObject.ChapterState state)
		{
			if(currentPlayObject != null)
			{
				chapterObjectData.ChangeState(currentPlayObject.chapterName, state);
			}
		}


		public async void Play(Action action)
		{
			await PlayToStop();
			action?.Invoke();
		}
		public async Awaitable PlayToStop()
		{
			if(ThisContainer.TryGetChildObject<ChapterObject>(out var getPlayObject))
			{
				if(currentPlayObject != getPlayObject)
				{
					currentPlayObject.chapterSaveState.Event -= ChangeCurrentState;
					currentPlayObject = getPlayObject;
					currentPlayObject.chapterSaveState.Event += ChangeCurrentState;
				}
			}
			if(currentPlayObject == null) return;
			await currentPlayObject.Play();
		}
		public void Pause()
		{
			if(!ThisContainer.TryGetChildObject<ChapterObject>(out var getPlayObject))
			{
				if(currentPlayObject != getPlayObject)
				{
					currentPlayObject.chapterSaveState.Event -= ChangeCurrentState;
					currentPlayObject = getPlayObject;
					currentPlayObject.chapterSaveState.Event += ChangeCurrentState;
				}
			}
			if(currentPlayObject == null) return;
			currentPlayObject.Pause();
		}
		public void Resume()
		{
			if(!ThisContainer.TryGetChildObject<ChapterObject>(out var getPlayObject))
			{
				if(currentPlayObject != getPlayObject)
				{
					currentPlayObject.chapterSaveState.Event -= ChangeCurrentState;
					currentPlayObject = getPlayObject;
					currentPlayObject.chapterSaveState.Event += ChangeCurrentState;
				}
			}
			if(currentPlayObject == null) return;
			currentPlayObject.Resume();
		}
		public void Stop()
		{
			if(!ThisContainer.TryGetChildObject<ChapterObject>(out var getPlayObject))
			{
				if(currentPlayObject != getPlayObject)
				{
					currentPlayObject.chapterSaveState.Event -= ChangeCurrentState;
					currentPlayObject = getPlayObject;
					currentPlayObject.chapterSaveState.Event += ChangeCurrentState;
				}
			}
			if(currentPlayObject == null) return;
			currentPlayObject.Stop();
		}
	}
}