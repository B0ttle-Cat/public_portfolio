using BC.AIProvider;
using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFContent
{
	public class CombatCommandUpdater : CommandUpdater
	{
		UnitCommandComponent unitCommand;
		private ROCProvider rocProvider;

		private ActuatorModule actuator;
		private Vector3 actuatorTargetPosition;
		private ObjectBehaviour actuatorTargetObject;

		private ActuatorModule.ActuatorPlayCallback actuatorPlayCallback;

		public UnitCommandComponent UnitCommand { get => unitCommand; set => unitCommand=value; }
		public ROCProvider RocProvider { get => rocProvider; set => rocProvider=value; }
		[ShowInInspector, ReadOnly]
		public ActuatorModule Actuator { get => actuator; set => actuator=value; }
		[ShowInInspector, ReadOnly]
		public Vector3 ActuatorTargetPosition { get => actuatorTargetPosition; set => actuatorTargetPosition=value; }
		[ShowInInspector, ReadOnly]
		public ObjectBehaviour ActuatorTargetObject { get => actuatorTargetObject; set => actuatorTargetObject=value; }

		protected override void BaseAwake()
		{
			UnitCommand = ThisContainer.GetComponent<UnitCommandComponent>();
			RocProvider = ThisContainer.GetComponent<ROCProvider>();
			actuatorPlayCallback = new ActuatorModule.ActuatorPlayCallback() {
				onEnterCasting = OnEnterCasting,
				onEnterAction = OnEnterAction,
				onEnterCooldown = OnEnterCooldown,
				onEndActuator = OnEndActuator
			};
		}
		protected override void BaseStart()
		{
		}

		#region Input
		public void OnActuatorCancel(bool clearSelect = false)
		{
			if(Actuator != null)
			{
				// 취소 시키기
				Actuator.OnActivityCancel();
				Actuator = null;
			}
			ActuatorTargetPosition = Vector3.zero;
			ActuatorTargetObject = null;

			if(clearSelect)
			{
				UnitCommand.CommandData.OnClearSelectActuator();
			}
		}
		public void OnActuatorActive(ActuatorModule actuator, Vector3 targetPosition)
		{
			OnActuatorActive(actuator, targetPosition, null);
		}
		public void OnActuatorActive(ActuatorModule actuator, ObjectBehaviour targetObject)
		{
			TryGetTargetPos(targetObject, out Vector3 targetPosition);
			OnActuatorActive(actuator, targetPosition, targetObject: actuator.Target.isNonTarget ? null : targetObject);
		}
		public void OnActuatorActive(ActuatorModule actuator, UnitPairsTarget unitPairsTarget)
		{
			OnActuatorActive(actuator, unitPairsTarget.targetPosition, unitPairsTarget.targetObject);
		}
		private void OnActuatorActive(ActuatorModule actuator, Vector3 targetPosition, ObjectBehaviour targetObject)
		{
			if(actuator == null)
			{
				OnActuatorCancel();
				return;
			}
			if(Actuator != null && Actuator != actuator)
			{
				Actuator.OnActivityCancel();
			}
			Actuator = actuator;
			ActuatorTargetPosition = targetPosition;
			ActuatorTargetObject = targetObject;
			if(ActuatorTargetObject != null && ActuatorTargetObject.ThisContainer.TryGetComponent<ReactionTarget>(out var attackingTarget))
			{
				ActuatorTargetPosition = attackingTarget.ReactionPosition();
			}
			Actuator.OnActivity(ThisObject, ActuatorTargetPosition, ActuatorTargetObject, actuatorPlayCallback);
		}
		public void OnActuatorPreview(ActuatorModule actuator, Vector3 targetPosition)
		{
			if(actuator.HasHitShape)
			{
				actuator.OnDrawHitShape(ThisTransform.position, targetPosition, 1f);
			}
		}
		public bool IsActiveActuator()
		{
			return Actuator != null && Actuator.IsActivity;
		}
		public bool IsPlayActuator()
		{
			return Actuator != null && Actuator.IsPlaying;
		}
		#endregion

		#region Update
		public override void OnCommandUpdater()
		{
			UpdateSelectPair();
		}
		private void OnEnterCasting(ActuatorModule module)
		{
		}
		private void OnEnterAction(ActuatorModule module)
		{
		}
		private void OnEnterCooldown(ActuatorModule module)
		{
		}
		private void OnEndActuator(ActuatorModule module, bool result)
		{
			//UnitCommand.OnActuatorCancel();
		}
		private void UpdateSelectPair()
		{
			var CommandData = UnitCommand.CommandData;

			if(CommandData.HasManualActuator)
			{
				CommandData.manualSelectPair.Refresh(UnitCommand.CombatCompute);
				if(!CommandData.autoSelectPair.canUseActuatorList.Contains(CommandData.manualSelectActuator))
				{
					CommandData.manualSelectActuator = null;
				}
			}
			if(CommandData.HasAutoActuator)
			{
				CommandData.autoSelectPair.Refresh(UnitCommand.CombatCompute);
				if(!CommandData.autoSelectPair.canUseActuatorList.Contains(CommandData.autoSelectActuator))
				{
					CommandData.autoSelectActuator = null;
				}
				if(CommandData.HasAutoActuator)
				{
					CommandData.resetAutoActuatorTimeUpdate -= Time.deltaTime;
				}
			}
		}
		public bool TryGetTargetPos(ObjectBehaviour targetObject, out Vector3 targetPosition)
		{
			targetPosition = Vector3.zero;
			if(targetObject == null) return false;

			targetPosition = targetObject.ThisContainer.TryGetComponent<ReactionTarget>(out ReactionTarget attackingTarget)
				? attackingTarget.ReactionPosition()
				: targetObject.ThisTransform.position;
			return true;
		}
		#endregion

		public bool ComputeSelectActuatorModule(out ActuatorModule resultActuator, out UnitPairsTarget resultPair)
		{
			resultActuator = null;
			resultPair = default;

			if(UnitCommand == null) return false;
			if(UnitCommand?.CombatCompute == null) return false;

			var targetList = UnitCommand.CommandData?.inRangeReactionUnit;
			if((targetList?.Count ?? 0) == 0) return false;

			if(!UnitCommand.CombatCompute.GetUnitPairsValue(ThisObject, targetList, out UnitPairsTarget[] allPairsValue)) return false;

			return RocProvider.ComputeSelectActuatorModule(allPairsValue, true, out resultActuator, out resultPair);
		}
	}
}