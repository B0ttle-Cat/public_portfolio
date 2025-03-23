using BC.AIProvider;
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
			commandSystem.CommandType = CommandSystem.CommandControllerType.DefaultCommand;

			if(commandSystem.ThisContainer.TryGetComponent<CombatComputeComponent>(out var combatManagement))
			{
				combatManagement.iffMatchingInfoData = DataCarrier.GetSeparate<GameContentLoader>().GetData<IFFMatchingInfoData>();
			}
		}

		protected override void OnActionEnd()
		{
		}
	}
}