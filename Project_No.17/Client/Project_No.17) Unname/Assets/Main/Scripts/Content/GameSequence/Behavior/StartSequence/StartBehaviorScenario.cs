using BC.OdccBase;

using TFSystem;

using UnityEngine;
namespace TFContent
{
	public class StartBehaviorScenario : OdccBehaviorTarget
	{
		private ScenarioPlayer scenarioPlayer;

		protected override bool OnActionValid()
		{
			return ThisContainer.TryGetComponent<ScenarioPlayer>(out scenarioPlayer);
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(scenarioPlayer != null)
			{
				scenarioPlayer.scenarioObjectData = DataCarrier.GetSeparate<ScenarioObjectData>().GetData<ScenarioObjectData>();
				await scenarioPlayer.PlayToStop();
			}
		}

		protected override void OnActionEnd()
		{
		}
	}
}