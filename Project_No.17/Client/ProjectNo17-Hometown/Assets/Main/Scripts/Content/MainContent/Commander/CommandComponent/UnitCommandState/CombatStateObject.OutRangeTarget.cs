using BC.AIProvider;

namespace TFContent
{
	public partial class UnitCommandComponent
	{
		protected partial class CombatStateObject
		{
			protected class OutRangeTarget : SubCombatState
			{
				public OutRangeTarget(CombatStateObject combatState, UnitCommandComponent unitCommandComponent) : base(combatState, unitCommandComponent) { }


				protected override void DisableState()
				{
				}

				protected override void EnableState()
				{

				}

				protected override void UpdateState()
				{
					var selectActuator = CommandData.SelectActuator;
					if(selectActuator == null) return;
					UnitPairsTarget selectPair = CommandData.SelectPair;
					UnitCommand.MovementPositionUseActuator(selectActuator, selectPair, selectPair.thisRadius);
				}
			}
		}
	}
}
