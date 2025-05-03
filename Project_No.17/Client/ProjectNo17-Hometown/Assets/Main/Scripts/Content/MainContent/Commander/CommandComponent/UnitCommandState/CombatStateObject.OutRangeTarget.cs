using BC.AIProvider;

using TFSystem.SaveLoad;

namespace TFContent
{
	public partial class UnitCommandComponent //.OutRangeTarget
	{
		protected partial class CombatStateObject //.OutRangeTarget
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
