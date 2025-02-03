using System;

using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class ScenarioPlayer : ComponentBehaviour//, IOdccUpdate
	{
		public ScenarioObjectData scenarioObjectData;
		private ScenarioObject currentPlayObject;

		private void ChangeCurrentState(ScenarioObject.ScenarioState satte)
		{
			scenarioObjectData.ChangeState(currentPlayObject);
		}


		public async void Play(Action action)
		{
			await PlayToStop();
			action?.Invoke();
		}
		public async Awaitable PlayToStop()
		{
			if(ThisContainer.TryGetChildObject<ScenarioObject>(out var getPlayObject))
			{
				if(currentPlayObject != getPlayObject)
				{
					currentPlayObject.scenarioSaveState.Event -= ChangeCurrentState;
					currentPlayObject = getPlayObject;
					currentPlayObject.scenarioSaveState.Event += ChangeCurrentState;
				}
			}
			if(currentPlayObject == null) return;
			await currentPlayObject.Play();
		}
		public void Pause()
		{
			if(!ThisContainer.TryGetChildObject<ScenarioObject>(out var getPlayObject))
			{
				if(currentPlayObject != getPlayObject)
				{
					currentPlayObject.scenarioSaveState.Event -= ChangeCurrentState;
					currentPlayObject = getPlayObject;
					currentPlayObject.scenarioSaveState.Event += ChangeCurrentState;
				}
			}
			if(currentPlayObject == null) return;
			currentPlayObject.Pause();
		}
		public void Resume()
		{
			if(!ThisContainer.TryGetChildObject<ScenarioObject>(out var getPlayObject))
			{
				if(currentPlayObject != getPlayObject)
				{
					currentPlayObject.scenarioSaveState.Event -= ChangeCurrentState;
					currentPlayObject = getPlayObject;
					currentPlayObject.scenarioSaveState.Event += ChangeCurrentState;
				}
			}
			if(currentPlayObject == null) return;
			currentPlayObject.Resume();
		}
		public void Stop()
		{
			if(!ThisContainer.TryGetChildObject<ScenarioObject>(out var getPlayObject))
			{
				if(currentPlayObject != getPlayObject)
				{
					currentPlayObject.scenarioSaveState.Event -= ChangeCurrentState;
					currentPlayObject = getPlayObject;
					currentPlayObject.scenarioSaveState.Event += ChangeCurrentState;
				}
			}
			if(currentPlayObject == null) return;
			currentPlayObject.Stop();
		}
	}
}