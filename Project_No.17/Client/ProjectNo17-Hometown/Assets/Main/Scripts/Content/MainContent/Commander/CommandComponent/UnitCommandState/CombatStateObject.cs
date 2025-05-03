using System;

using BC.Actuator;
using BC.AIProvider;
using BC.Base;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using UnityEngine;

namespace TFContent
{
	public partial class UnitCommandComponent //.CombatStateObject
	{
		protected partial class CombatStateObject : UnitCommandStateObject
		{
			public CombatStateObject(UnitCommandComponent unitCommandComponent) : base(unitCommandComponent)
			{

			}

			public override UnitCommandData.UnitCommandStateType CheckNextStateUpdate(in UnitCommandData.UnitCommandStateType prevState)
			{
				bool isDead = CommandData.IsDead;
				if(isDead) return UnitCommandData.UnitCommandStateType.Neutralize;

				if(CheckCancelActuatorNextState(out var nextState, out bool changeSubStateOnCancel))
				{
					return nextState;
				}

				if(subCombatState != null)
				{
					return CheckSubCombatNextStateUpdate(subCombatStateType, changeSubStateOnCancel ? CombatStateType.OnPlayActuatorCancel : subCombatState.CheckNextStateUpdate(in subCombatStateType));
				}
				return UnitCommandData.UnitCommandStateType.Idle;
			}

			private bool CheckCancelActuatorNextState(out UnitCommandData.UnitCommandStateType nextState, out bool changeSubStateToCancel)
			{
				nextState = UnitCommandData.UnitCommandStateType.Combat;
				changeSubStateToCancel = false;

				bool isOnPlayActuatorCancel = subCombatStateType == CombatStateType.OnPlayActuatorCancel;
				if(isOnPlayActuatorCancel)
				{
					// 이미 취소중인 상태임.
					return false;
				}

				bool hasPlayingActuator = CombatUpdater.IsPlayingActuator();
				if(!hasPlayingActuator)
				{
					if(MovementUpdater.HasMoveTarget())
					{
						nextState = UnitCommandData.UnitCommandStateType.Move;
						// 검사를 중단하고 nextState로 전환한다.
						return true;
					}
					// 다음 조건을 위해 검사를 계속 한다.
					return false;
				}
				// 실행중인 Actuator를 계속 실행할건지 아니면 중단하고 넘어갈 것인지 결정한다.

				bool isBlockingChangeStateInPlaying = CombatUpdater.IsBlockingChangeStateInPlaying();
				if(isBlockingChangeStateInPlaying)
				{
					// 중단할 수단이 있지만, Actuator에서 중단을 막음.
					return true;
				}

				bool hasMovePath = MovementUpdater.HasMoveTarget();
				/// bool someOtherCancelValue = true; // 추가적인 중단 요청이 생길경우 로직 추가하기

				bool tryChangeOtherState = hasMovePath;
				if(!tryChangeOtherState)
				{
					// Actuator를 중단시킬 수단이 없음. 계속 전투를 함.
					return true;
				}

				// 여기까지 진앱했으면 중단이 되어야 함.
				// 다만 즉시 중단될 것인지 아니면 약간의 지연이 있는 다음에 중지될 것인지 추가적인 진행이 필요한다.

				bool isCanImmediateCancel = CombatUpdater.IsImmediateCancel();

				if(!isCanImmediateCancel)
				{
					// 즉시 중단되지 않기에 상태를 바로 변경하지 않는다. 대신 CombatStateType 를 사용하여 Sub 상태를 전환하여 빠져나오도록 하기.
					changeSubStateToCancel = true;
					return false;
				}

				if(hasMovePath)
				{
					nextState = UnitCommandData.UnitCommandStateType.Move;
				}
				return true;
			}

			private UnitCommandData.UnitCommandStateType CheckSubCombatNextStateUpdate(CombatStateType prevSubCombatStateType, CombatStateType nextSubCombatStateType)
			{
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


			[ShowInInspector, ReadOnly]
			protected CombatStateType subCombatStateType;
			[SerializeReference, ShowInInspector, ReadOnly]
			protected SubCombatState subCombatState;

			protected override void DisableState()
			{
				if(subCombatState == null) return;
				CombatUpdater.OnActuatorCancel(true);
				CombatUpdater.SetAimStateValue(0f);
				subCombatState.OnDisableState();
				subCombatState = null;
			}
			protected override void EnableState()
			{
				subCombatState?.OnDisableState();
				subCombatStateType = CombatStateType.LostTarget;
				subCombatState = NextStateChange(subCombatStateType);
				subCombatState.OnEnableState();

				CommandData.resetAutoActuatorTimeUpdate = -1;
				CombatUpdater.SetAimStateValue(0f);
			}
			protected override void UpdateState()
			{
				if(subCombatState == null) return;
				subCombatState.OnUpdateState();
				UpdateCombatAnimatorValue();
			}

			private void UpdateCombatAnimatorValue()
			{
				CombatUpdater.AimStateUpdate(subCombatStateType switch {
					CombatStateType.InRangeTarget => true,
					CombatStateType.OnPlayActuator => true,
					_ => false,
				});
			}

			public override void OnSave(ref SaveLoadData data)
			{
				data.SaveValue(nameof(subCombatStateType), subCombatStateType);

				if(subCombatState == null) data.SaveIData(nameof(subCombatState), subCombatState);
			}
			public override void OnLoad(in SaveLoadData data)
			{
				data.LoadValue(nameof(subCombatStateType), out subCombatStateType, subCombatStateType);
				subCombatState?.OnDisableState();
				subCombatState = NextStateChange(subCombatStateType);
				data.LoadIData(nameof(subCombatState), subCombatState);
			}
			protected virtual SubCombatState NextStateChange(in CombatStateType nextSubCombatStateType)
			{
				return nextSubCombatStateType switch {
					CombatStateType.OutRangeTarget => new OutRangeTarget(this, UnitCommand),
					CombatStateType.InRangeTarget => new InRangeTarget(this, UnitCommand),
					CombatStateType.LostTarget => new LostTarget(this, UnitCommand),
					CombatStateType.OnPlayActuator => new OnPlayActuator(this, UnitCommand),
					CombatStateType.OnPlayActuatorCancel => new OnPlayActuatorCancel(this, UnitCommand),
					_ => new LostTarget(this, UnitCommand),
				};
			}

			protected enum CombatStateType
			{
				OutRangeTarget = 0, InRangeTarget, LostTarget,
				OnPlayActuator, CloseCombatState,

				OnPlayActuatorCancel = 999,
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

				public override CombatStateType CheckNextStateUpdate(in CombatStateType prevState)
				{
					if(CheckIsLostState())
					{
						return CombatStateType.LostTarget;
					}

					if(CheckIsOutRangeTarget())
					{
						return CombatStateType.OutRangeTarget;
					}

					return CombatStateType.InRangeTarget;
				}
				protected bool CheckIsLostState()
				{
					if(!UnitCommand.HasReactionTargetInAround())
					{
						return true;
					}
					UnitCommand.UpdateSelectActuator();
					if(!CommandData.HasSelectActuator)
					{
						return true;
					}
					return false;
				}
				protected bool CheckIsOutRangeTarget()
				{
					if(!UnitCommand.HasActionTargetInRange())
					{
						return true;
					}
					return false;
				}
				protected bool CheckIsMovementOrLook()
				{
					if(MovementUpdater.IsMovement() || !MovementUpdater.IsLookTarget())
					{
						return true;
					}
					return false;
				}
				protected bool CheckIsOnPlayActuator()
				{
					if(CombatUpdater.IsPlayingActuator())
					{
						return true;
					}
					return false;
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