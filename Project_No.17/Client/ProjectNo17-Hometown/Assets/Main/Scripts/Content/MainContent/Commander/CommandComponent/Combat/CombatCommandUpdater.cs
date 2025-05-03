using BC.Actuator;
using BC.AIProvider;
using BC.ODCC;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using UnityEngine;

namespace TFContent
{
	public partial class CombatCommandUpdater : CommandUpdater
	{
		private UnitCommandComponent unitCommand;
		private ROCProvider rocProvider;
		private ActuatorProvider actuatorProvider;
		private ActuatorPlayer actuatorPlayer;
		private CharacterAnimatorComponent characterAnimator;

		public UnitCommandComponent UnitCommand => unitCommand;
		public ROCProvider RocProvider => rocProvider;
		public ActuatorProvider ActuatorProvider => actuatorProvider;
		public ActuatorPlayer ActuatorPlayer => actuatorPlayer;
		public CharacterAnimatorComponent CharacterAnimator => characterAnimator;

		private const float aimChangeSpeed = 1f/0.25f;

		[ShowInInspector,ReadOnly]
		private float animatorIsAim;
		public float AnimatorIsAim {
			get => animatorIsAim;
			set {
				//if(animatorIsAim!=value)
				{
					animatorIsAim = value;
					if(CharacterAnimator != null)
					{
						CharacterAnimator.SetAimProgress(value);
					}
				}
			}
		}
		public bool IsAiming => AnimatorIsAim > 0.999f;

		protected override void BaseAwake()
		{
			ThisContainer.TryGetComponent<UnitCommandComponent>(out unitCommand);
			ThisContainer.TryGetComponent<ROCProvider>(out rocProvider);
			ThisContainer.TryGetComponent<ActuatorProvider>(out actuatorProvider);
			ThisContainer.TryGetComponent<ActuatorPlayer>(out actuatorPlayer);
			ThisContainer.TryGetComponent<CharacterAnimatorComponent>(out characterAnimator);

			AnimatorIsAim = 0;
		}
		protected override void BaseStart()
		{
		}
		public override void OnCommandUpdater()
		{
			UpdateSelectPair();
		}
	}
	public partial class CombatCommandUpdater // Input 관련
	{
		public void OnActuatorCancel(bool clearSelect = false)
		{
			ActuatorPlayer.OnActivityCancel();
			if(clearSelect)
			{
				UnitCommand.CommandData.OnClearSelectActuator();
			}
		}
		public void OnActivityCancel_WithBlanding()
		{
			ActuatorPlayer.OnActivityCancel_WithBlanding();
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

			if(targetObject == null)
			{
				ActuatorProvider.OnActuatorPlay(actuator);
				ActuatorPlayer.OnActuatorPlay(actuator, targetPosition);
			}
			else
			{
				ActuatorProvider.OnActuatorPlay(actuator);
				ActuatorPlayer.OnActuatorPlay(actuator, targetObject);
			}
		}
		public void OnActuatorPreview(ActuatorModule actuator, Vector3 targetPosition)
		{
			if(actuator.HasHitShape)
			{
				actuator.OnDrawHitShape(ThisTransform.position, targetPosition, 1f);
			}
		}

	}
	public partial class CombatCommandUpdater // Update 관련
	{
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
	}
	public partial class CombatCommandUpdater // ActuatorModule 작업
	{
		public bool IsPlayingActuator()
		{
			return ActuatorPlayer.IsPlayingActuator();
		}
		public bool IsImmediateCancel()
		{
			var CommandData = UnitCommand.CommandData;
			if(!CommandData.HasSelectActuator) return true;

			var act = CommandData.SelectActuator;
			if(act == null || !act.HasBasic) return true;

			return act.Basic.IsImmediateCancel;
		}
		public bool IsBlockingChangeStateInPlaying()
		{
			var CommandData = UnitCommand.CommandData;
			if(!CommandData.HasSelectActuator) return true;

			var act = CommandData.SelectActuator;
			if(act == null || !act.HasBasic) return true;

			return !act.Basic.IsCanCancelInPlay;
		}

		public void AimStateUpdate(bool isAim)
		{
			float deltaSpeed = aimChangeSpeed * Time.deltaTime;
			float _AnimatorIsAim = AnimatorIsAim + (isAim ?  deltaSpeed : - deltaSpeed);
			if(_AnimatorIsAim < 0f) _AnimatorIsAim = 0f;
			else if(_AnimatorIsAim > 1f) _AnimatorIsAim = 1f;
			AnimatorIsAim = _AnimatorIsAim;
		}
		public void SetAimStateValue(float aimValue)
		{
			AnimatorIsAim = aimValue;
		}
		public bool ComputeSelectActuatorModule(out ActuatorModule resultActuator, out UnitPairsTarget resultPair)
		{
			resultActuator = null;
			resultPair = default;

			if(UnitCommand == null) return false;
			if(UnitCommand.CombatCompute == null) return false;

			var targetList = UnitCommand.CommandData?.inRangeReactionUnit;
			if((targetList?.Count ?? 0) == 0) return false;

			if(!UnitCommand.CombatCompute.GetUnitPairsValue(ThisObject, targetList, out UnitPairsTarget[] allPairsValue)) return false;

			return RocProvider.ComputeSelectActuatorProviderModule(allPairsValue, true, out resultActuator, out resultPair);
		}
	}

	public partial class CombatCommandUpdater : ISaveLoadTarget
	{
		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
			data.SaveValue(nameof(animatorIsAim), animatorIsAim);
		}

		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			if(data.LoadValue<float>(nameof(animatorIsAim), out var _animatorIsAim))
			{
				animatorIsAim = _animatorIsAim;
			}
		}
	}
}