using System;

using BC.AIProvider;
using BC.Base;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFContent
{
	public partial class UnitCommandComponent
	{
		protected partial class CombatStateObject : UnitCommandStateObject
		{
			public CombatStateObject(UnitCommandComponent unitCommandComponent) : base(unitCommandComponent) { }

			public override UnitCommandData.UnitCommandStateType CheckNextStateType()
			{
				bool isDead = CommandData.IsDead;
				if(isDead) return UnitCommandData.UnitCommandStateType.Neutralize;

				if(CombatUpdater.IsActiveActuator())
				{
					return UnitCommandData.UnitCommandStateType.Combat;
				}

				bool hasMovePath = MovementUpdater.HasMoveTarget();
				if(hasMovePath) return UnitCommandData.UnitCommandStateType.Move;

				if(subCombatState != null)
				{
					CombatStateType prevSubCombatStateType = subCombatStateType;
					CombatStateType nextSubCombatStateType = subCombatState.CheckNextStateType();
					if(nextSubCombatStateType == CombatStateType.CloseCombatState)
					{
						return UnitCommandData.UnitCommandStateType.Idle;
					}
					else if(prevSubCombatStateType != nextSubCombatStateType)
					{
						SubCombatState nextSubCombatState = NextStateChange(nextSubCombatStateType);
						if(nextSubCombatState != null)
						{
							subCombatState.OnDisableState();
							subCombatStateType = nextSubCombatStateType;
							subCombatState = nextSubCombatState;
							subCombatState.OnEnableState();
						}

					}
					return UnitCommandData.UnitCommandStateType.Combat;
				}
				return UnitCommandData.UnitCommandStateType.Idle;
			}
			[ShowInInspector, ReadOnly]
			protected CombatStateType subCombatStateType;
			[SerializeReference, ShowInInspector, ReadOnly]
			protected SubCombatState subCombatState;

			protected override void DisableState()
			{
				if(subCombatState == null) return;
				CombatUpdater.OnActuatorCancel(true);
				subCombatState.OnDisableState();
				subCombatState = null;
			}
			protected override void EnableState()
			{
				subCombatState?.OnDisableState();
				subCombatStateType = CombatStateType.LostTarget;
				subCombatState = NextStateChange(CombatStateType.LostTarget);
				subCombatState.OnEnableState();

				CommandData.resetAutoActuatorTimeUpdate = -1;
			}
			protected override void UpdateState()
			{
				if(subCombatState == null) return;
				subCombatState.OnUpdateState();
			}
			protected virtual SubCombatState NextStateChange(in CombatStateType nextSubCombatStateType)
			{
				return nextSubCombatStateType switch {
					CombatStateType.OutActionRangeTarget => new OutRangeTarget(this, UnitCommand),
					CombatStateType.InActionRangeTarget => new InRangeTarget(this, UnitCommand),
					CombatStateType.LostTarget => new LostTarget(this, UnitCommand),
					_ => new LostTarget(this, UnitCommand),
				};
			}

			protected enum CombatStateType
			{
				OutActionRangeTarget, InActionRangeTarget, LostTarget,
				CloseCombatState,
			}
			[Serializable]
			protected abstract class SubCombatState : StateObject<CombatStateType>
			{
				protected UnitCommandComponent UnitCommand { get; private set; }
				protected UnitCommandData CommandData => UnitCommand.CommandData;
				protected MovementCommandUpdater MovementUpdater => UnitCommand.MovementUpdater;
				protected CombatCommandUpdater CombatUpdater => UnitCommand.CombatUpdater;
				protected CombatStateObject CombatState { get; private set; }
				public SubCombatState(CombatStateObject combatState, UnitCommandComponent unitCommandComponent)
				{
					UnitCommand = unitCommandComponent;
					CombatState = combatState;
				}
				public void OnDisableState()
				{
					if(UnitCommand == null) return;
					DisableState();
					UnitCommand = null;
				}
				public void OnEnableState()
				{
					if(UnitCommand == null) return;
					EnableState();
				}
				public void OnUpdateState()
				{
					if(UnitCommand == null) return;
					UpdateState();
				}

				public override CombatStateType CheckNextStateType()
				{
					if(!UnitCommand.HasReactionTargetInAround())
					{
						return CombatStateType.LostTarget;
					}
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
					else
					{
						return CombatStateType.LostTarget;
					}
				}
			}

		}
		private void MovementPositionUseActuator(ActuatorModule selectActuator, UnitPairsTarget selectPair, float insideDistanceOffset = 0f)
		{
			MovementPositionUseActuator(selectActuator, selectPair, CommandData.isEnterCombat, insideDistanceOffset);
		}
		private void MovementPositionUseActuator(ActuatorModule selectActuator, UnitPairsTarget selectPair, bool useMinMaxRange, float insideDistanceOffset = 0f)
		{
			Vector3 position = selectPair.thisPosition;
			Vector3 look = selectPair.directionToTarget;

			float distance = selectPair.spaceDistance;

			selectActuator.GetShapeRange(out float nearRange, out float farRange, out float minRange, out float maxRange);
			float NearRange = (useMinMaxRange ? minRange : nearRange) + insideDistanceOffset;
			float FarRange = (useMinMaxRange ? maxRange : farRange) - insideDistanceOffset;

			float stopDistance = MovementUpdater.StopDistance();

			if(distance < NearRange)
			{
				// 거리가 너무 가까운 경우
				distance = NearRange - distance;
				if(distance <= stopDistance)
				{
					distance = stopDistance * 2f;
				}

				position += selectPair.directionToTarget * -distance;
				MovementUpdater.OnMovePosition(position, look);
			}
			else if(distance > FarRange)
			{
				// 거리가 너무 면 경우
				distance = distance - FarRange;
				if(distance <= stopDistance)
				{
					distance = stopDistance * 2f;
				}

				position += selectPair.directionToTarget * distance;
				MovementUpdater.OnMovePosition(position, look);
			}
			else
			{
				MovementUpdater.OnMovePosition(position, look);
			}
		}
		private void MovementPositionUseMoveTarget()
		{
			if(MovementUpdater.HasMoveTarget())
			{
				if(CommandData.moveIsTeleport)
				{
					MovementUpdater.OnSetPosition(in CommandData.moveTargetPosition, CommandData.moveTargetLook);
				}
				else
				{
					MovementUpdater.OnMovePosition(in CommandData.moveTargetPosition, CommandData.moveTargetLook);
				}
			}
		}
		private void UpdateSelectActuator()
		{
			if(CommandData.HasManualActuator)
			{
				CommandData.resetAutoActuatorTimeUpdate = CommandData.resetAutoActuatorTimeMax;
			}
			else if(CommandData.HasAutoActuator)
			{
				if(CommandData.resetAutoActuatorTimeUpdate < 0)
				{
					SelectAutoActuatorModule();
					CommandData.resetAutoActuatorTimeUpdate = CommandData.resetAutoActuatorTimeMax;
				}
			}
			else
			{
				SelectAutoActuatorModule();
				CommandData.resetAutoActuatorTimeUpdate = CommandData.resetAutoActuatorTimeMax;
			}
		}
		private void SelectAutoActuatorModule()
		{
			if(CombatUpdater.ComputeSelectActuatorModule(out var selectActuator, out var selectPair))
			{
				CommandData.autoSelectActuator = selectActuator;
				CommandData.autoSelectPair = selectPair;
			}
			else
			{
				CommandData.autoSelectActuator = null;
				CommandData.autoSelectPair = default;
			}
		}
		private bool HasReactionTargetInAround()
		{
			return (CommandData?.inRangeReactionUnit?.Count ?? 0) > 0;
		}
		private bool HasActionTargetInRange()
		{
			return HasActionTargetInRange(CommandData.isEnterCombat);
		}
		private bool HasActionTargetInRange(bool useMinMaxRange)
		{
			ActuatorModule selectActuator = CommandData.SelectActuator;
			UnitPairsTarget selectUnitPair = CommandData.SelectPair;
			if(selectActuator == null) return false;

			if(!EventManager.TryGet<ICombatCompute>(out var result)) return false;

			float distance = selectUnitPair.spaceDistance;
			selectActuator.GetShapeRange(out float nearRange, out float farRange, out float minRange, out float maxRange);
			float NearRange = useMinMaxRange ? minRange : nearRange;
			float FarRange = useMinMaxRange ? maxRange : farRange;

			if(distance < NearRange)
			{
				// 거리가 너무 가까운 경우
				return false;
			}
			else if(distance > FarRange)
			{
				// 거리가 너무 면 경우
				return false;
			}
			return true;
		}
	}
}