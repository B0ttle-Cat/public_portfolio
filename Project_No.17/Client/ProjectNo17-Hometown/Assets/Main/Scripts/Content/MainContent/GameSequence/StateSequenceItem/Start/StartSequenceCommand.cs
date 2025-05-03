using TFSystem;

using UnityEngine;
namespace TFContent
{
	public class StartSequenceCommand : GameSequenceItem
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
				combatManagement.iffMatchingInfoData = DataCarrier.GetSeparate<InitGameContentLoader>().GetData<IFFMatchingObjectData>();
			}
		}

		protected override void OnActionEnd()
		{
			DestroyThis();
		}
	}
}