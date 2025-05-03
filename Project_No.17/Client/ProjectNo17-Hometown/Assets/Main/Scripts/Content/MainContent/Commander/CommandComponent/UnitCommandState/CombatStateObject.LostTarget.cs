using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using UnityEngine;
namespace TFContent
{
	public partial class UnitCommandComponent //.LostTarget
	{
		protected partial class CombatStateObject //.LostTarget
		{
			protected class LostTarget : SubCombatState
			{
				[ShowInInspector]
				float lostTargetTrackingTime = 3f;
				[ShowInInspector]
				float updateTime = 0f;
				public LostTarget(CombatStateObject combatState, UnitCommandComponent unitCommandComponent) : base(combatState, unitCommandComponent) { }

				public override CombatStateType CheckNextStateUpdate(in CombatStateType prevState)
				{
					if(UnitCommand.HasReactionTargetInAround())
					{
						UnitCommand.UpdateSelectActuator();
						if(CommandData.HasSelectActuator)
						{
							if(UnitCommand.HasActionTargetInRange())
							{
								return CombatStateType.InRangeTarget;
							}
							else
							{
								return CombatStateType.OutRangeTarget;
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

				public override void OnSave(ref SaveLoadData data)
				{
					data.SaveValue(nameof(lostTargetTrackingTime), lostTargetTrackingTime);
					data.SaveValue(nameof(updateTime), updateTime);
				}
				public override void OnLoad(in SaveLoadData data)
				{
					data.LoadValue(nameof(lostTargetTrackingTime), out lostTargetTrackingTime, 3f);
					data.LoadValue(nameof(updateTime), out updateTime, 0f);
				}
			}
		}
	}
}