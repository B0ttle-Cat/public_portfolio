using BC.AIProvider;

using TFSystem.SaveLoad;

namespace TFContent
{
	public partial class UnitCommandComponent //.OnPlayActuator
	{
		protected partial class CombatStateObject //.OnPlayActuator
		{
			protected class OnPlayActuator : SubCombatState
			{
				public OnPlayActuator(CombatStateObject combatState, UnitCommandComponent unitCommandComponent) : base(combatState, unitCommandComponent) { }

				public override CombatStateType CheckNextStateUpdate(in CombatStateType prevState)
				{
					if(CheckIsLostState())
					{
						return CombatStateType.LostTarget;
					}
					if(CheckIsOnPlayActuator())
					{
						return CombatStateType.OnPlayActuator;
					}
					if(CheckIsOutRangeTarget())
					{
						return CombatStateType.OutRangeTarget;
					}
					if(CheckIsMovementOrLook())
					{
						return CombatStateType.InRangeTarget;
					}

					UpdateOnActuatorActive();
					if(CheckIsOnPlayActuator())
					{
						return CombatStateType.OnPlayActuator;
					}
					return CombatStateType.LostTarget;
				}

				protected override void DisableState()
				{
				}

				protected override void EnableState()
				{
					var selectActuator = CommandData.SelectActuator;
					if(selectActuator == null) return;

					UnitPairsTarget selectPair = CommandData.SelectPair;
					CombatUpdater.OnActuatorActive(selectActuator, selectPair);
				}

				protected override void UpdateState()
				{

				}

				private void UpdateOnActuatorActive()
				{
					var selectActuator = CommandData.SelectActuator;
					if(selectActuator == null) return;

					UnitPairsTarget selectPair = CommandData.SelectPair;
					CombatUpdater.OnActuatorActive(selectActuator, selectPair);
				}

				public override void OnSave(ref SaveLoadData data)
				{

				}
				public override void OnLoad(in SaveLoadData data)
				{

				}
			}

			protected class OnPlayActuatorCancel : SubCombatState
			{
				public OnPlayActuatorCancel(CombatStateObject combatState, UnitCommandComponent unitCommandComponent) : base(combatState, unitCommandComponent)
				{
				}
				public override CombatStateType CheckNextStateUpdate(in CombatStateType prevState)
				{
					return CheckIsOnPlayActuator() ? CombatStateType.OnPlayActuatorCancel : CombatStateType.CloseCombatState;
				}

				protected override void EnableState()
				{
					CombatUpdater.OnActivityCancel_WithBlanding();
				}
				protected override void DisableState()
				{
				}
				protected override void UpdateState()
				{
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
