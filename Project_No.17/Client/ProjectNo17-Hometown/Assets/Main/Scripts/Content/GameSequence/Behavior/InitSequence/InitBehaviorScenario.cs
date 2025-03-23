using BC.OdccBase;

using TFSystem;

using UnityEngine;
namespace TFContent
{
	public class InitBehaviorScenario : OdccBehaviorTarget
	{
		private ScenarioObjectData scenarioObjectData;

		protected override bool OnActionValid()
		{
			scenarioObjectData = DataCarrier.GetSeparate<ScenarioObjectData>().GetData<ScenarioObjectData>(scenarioObjectData);
			return scenarioObjectData != null;
		}

		protected override async Awaitable OnActionUpdate()
		{
			var scenario =  scenarioObjectData.FindPlayingScenario();
			if(scenario.HasValue)
			{
				ScenarioObject[] newScenarioObjects = await Object.InstantiateAsync<ScenarioObject>(scenario.Value.scenario.LoadAsset(), ThisTransform, Vector3.zero, Quaternion.identity);
				int length = newScenarioObjects.Length;
				for(int i = 0 ; i < length ; i++)
				{
					newScenarioObjects[i].gameObject.name = scenario.Value.key;
				}
			}
		}

		protected override void OnActionEnd()
		{
		}
	}
}