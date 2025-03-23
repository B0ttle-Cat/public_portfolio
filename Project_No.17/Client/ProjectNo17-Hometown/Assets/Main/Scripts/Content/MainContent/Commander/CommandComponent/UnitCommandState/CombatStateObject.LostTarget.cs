using Sirenix.OdinInspector;

using UnityEngine;
namespace TFContent
{
	public partial class UnitCommandComponent
	{
		protected partial class CombatStateObject
		{
			protected class LostTarget : SubCombatState
			{
				[ShowInInspector]
				float lostTargetTrackingTime = 3f;
				[ShowInInspector]
				float updateTime;
				public LostTarget(CombatStateObject combatState, UnitCommandComponent unitCommandComponent) : base(combatState, unitCommandComponent) { }

				public override CombatStateType CheckNextStateType()
				{
					if(UnitCommand.HasReactionTargetInAround())
					{
						UnitCommand.UpdateSelectActuator();
						if(CommandData.HasSelectActuator)
						{
							if(UnitCommand.HasActionTargetInRange())
							{
								return CombatStateType.InActionRangeTarget;
							}
							else
							{
								return CombatStateType.OutActionRangeTarget;
							}
						}
					}

					return updateTime < 0f ? CombatStateType.CloseCombatState : CombatStateType.LostTarget;
				}

				protected override void DisableState()
				{
				}

				protected override void EnableState()
				{
					updateTime = lostTargetTrackingTime;
					CommandData.isEnterCombat = false;
				}

				protected override void UpdateState()
				{
					updateTime -= Time.deltaTime;
				}
			}
		}
	}
}