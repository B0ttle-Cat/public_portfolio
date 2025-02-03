using System;
using System.Collections.Generic;

using BC.Base;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFContent
{
	[CreateAssetMenu(fileName = "ScenarioObjectData", menuName = "BC/ScenarioObjectData")]
	public class ScenarioObjectData : ScriptableObject
	{
		[Serializable]
		public struct ScenarioKey
		{
			[HideLabel, ReadOnly, HorizontalGroup("ScenarioKey")]
			public string key;

			[HideLabel, HorizontalGroup("ScenarioKey")]
			public ScenarioObject.ScenarioState state;

			[HideLabel]
			public ResourcesKey<ScenarioObject> scenario;

			internal void OnValidate()
			{
				scenario.OnValidate();
				key = scenario.resourcesName;
			}
		}

		public List<ScenarioKey> scenarioObjectKey;

		public ScenarioKey? FindPlayingScenario()
		{
			foreach(var data in scenarioObjectKey)
			{
				if(data.state == ScenarioObject.ScenarioState.Playing)
				{
					return data;
				}
			}
			return null;
		}

		public ScenarioKey? FindNextScenario()
		{
			foreach(var data in scenarioObjectKey)
			{
				if(data.state == ScenarioObject.ScenarioState.Waiting)
				{
					return data;
				}
			}
			return null;
		}
		public void ChangeState(ScenarioObject scenarioObject)
		{
			if(scenarioObject == null) return;

			string scenarioName = scenarioObject.gameObject.name;
			ScenarioObject.ScenarioState state = scenarioObject.scenarioSaveState.Value;
			for(int i = 0 ; i<scenarioObjectKey.Count ; i++)
			{
				ScenarioKey data = scenarioObjectKey[i];
				if(data.key == scenarioName)
				{
					data.state = state;
					scenarioObjectKey[i] = data;
					return;
				}
			}
		}

		public void OnValidate()
		{
			int length = scenarioObjectKey == null ? 0 : scenarioObjectKey.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var data = scenarioObjectKey[i];
				data.OnValidate();
				scenarioObjectKey[i] = data;
			}
		}
	}
}
