using BC.AISensor;
using BC.OdccBase;

using TFSystem;

using UnityEngine;
namespace TFContent
{
	public class StartBehaviorCommand : OdccBehaviorTarget
	{
		private CommandSystem commandSystem;
		protected override bool OnActionValid()
		{
			commandSystem = ThisContainer.GetObject<CommandSystem>();
			return commandSystem != null;
		}

		protected override async Awaitable OnActionUpdate()
		{
			commandSystem.CommandFlag = CommandSystem.CommandControllerFlag.UnitControl;

			if(commandSystem.ThisContainer.TryGetComponent<CombatManagement>(out var combatManagement))
			{
				combatManagement.iffMatchingInfoData = DataCarrier.GetSeparate<ScenarioObjectData>().GetData<IFFMatchingInfoData>();
			}
		}

		protected override void OnActionEnd()
		{
		}
	}
}