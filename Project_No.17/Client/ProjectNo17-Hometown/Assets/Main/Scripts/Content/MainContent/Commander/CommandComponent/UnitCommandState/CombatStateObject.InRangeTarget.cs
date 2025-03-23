using BC.AIProvider;

namespace TFContent
{
	public partial class UnitCommandComponent
	{
		protected partial class CombatStateObject
		{
			protected class InRangeTarget : SubCombatState
			{
				public InRangeTarget(CombatStateObject combatState, UnitCommandComponent unitCommandComponent) : base(combatState, unitCommandComponent) { }

				protected override void DisableState()
				{
				}

				protected override void EnableState()
				{
					CommandData.isEnterCombat = true;
				}

				protected override void UpdateState()
				{
					UpdateSelectActuator();
				}

				private void UpdateSelectActuator()
				{
					var selectActuator = CommandData.SelectActuator;
					if(selectActuator == null) return;
					if(CombatUpdater.IsActiveActuator()) return;

					UnitPairsTarget selectPair = CommandData.SelectPair;
					UnitCommand.MovementPositionUseActuator(selectActuator, selectPair);

					if(MovementUpdater.IsMovement() || !MovementUpdater.IsLookTarget()) return;

					CombatUpdater.OnActuatorActive(selectActuator, selectPair);
				}
			}
		}
	}
}