using BC.AIProvider;

using TFSystem.SaveLoad;

namespace TFContent
{
	public partial class UnitCommandComponent //.InRangeTarget
	{
		protected partial class CombatStateObject //.InRangeTarget
		{
			protected class InRangeTarget : SubCombatState
			{
				public InRangeTarget(CombatStateObject combatState, UnitCommandComponent unitCommandComponent) : base(combatState, unitCommandComponent) { }

				public override CombatStateType CheckNextStateUpdate(in CombatStateType prevState)
				{
					var nextState = base.CheckNextStateUpdate(in prevState);
					if(nextState != CombatStateType.InRangeTarget)
					{
						return nextState;
					}

					if(CheckIsMovementOrLook())
					{
						return nextState;
					}

					if(CombatUpdater.IsAiming)
					{
						return CombatStateType.OnPlayActuator;
					}

					return nextState;
				}

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

					UnitPairsTarget selectPair = CommandData.SelectPair;
					UnitCommand.MovementPositionUseActuator(selectActuator, selectPair);
				}

				public override void OnSave(ref SaveLoadData data)
				{

				}
				public override void OnLoad(in SaveLoadData data)
				{

				}
			}
		}
	}
}