using System;
using System.Collections.Generic;
using System.Threading;

using BC.Base;
using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

using static BC.AIProvider.ActuatorBasic;

namespace BC.AIProvider
{
	public partial class ActuatorModule//.Player
	{
		[TitleGroup("Actuator Player", subtitle: "사용시 실행되는 콘텐츠 플레이어", Alignment = TitleAlignments.Split)]
		[SerializeField, InlineEditor, HideLabel, PropertyOrder(-99)]
		private ActuatorPlayer Player;
		[SerializeField, ReadOnly, PropertyOrder(-99)]
		private bool isActivity;
		[SerializeField, ReadOnly, PropertyOrder(-99)]
		private bool isPlaying;
		[ShowInInspector, ReadOnly, PropertyOrder(-90)]
		private string PlayingLog { get; set; }
		public bool HasPlayer => Player != null;
		public bool IsActivity { get => HasPlayer && isActivity; private set => isActivity = value; }
		public bool IsPlaying { get => IsActivity && isPlaying; private set => isPlaying = value; }
		private Action CancelAwaitable { get; set; }

		public struct ActuatorTargetParameter
		{
			public ObjectBehaviour TargetObject;
			public Vector3 TargetPosition;

			public ActuatorTargetParameter(ObjectBehaviour targetObject)
			{
				TargetObject=targetObject;
				TargetPosition=default;
			}
			public ActuatorTargetParameter(Vector3 targetPosition)
			{
				TargetObject=null;
				TargetPosition=targetPosition;
			}
		}
		public class ActuatorPlayCallback
		{
			public Action<ActuatorModule> onEnterCasting;
			public Action<ActuatorModule> onEnterAction;
			public Action<ActuatorModule> onEnterCooldown;
			public Action<ActuatorModule, bool> onEndActuator;
			public void OnEnterCasting(ActuatorModule actuator)
			{
				onEnterCasting?.Invoke(actuator);
			}
			public void OnEnterAction(ActuatorModule actuator)
			{
				onEnterAction?.Invoke(actuator);
			}
			public void OnEnterCooldown(ActuatorModule actuator)
			{
				onEnterCooldown?.Invoke(actuator);
			}
			public void OnEndActuator(ActuatorModule actuator, bool result)
			{
				actuator.IsActivity = false;
				actuator.IsPlaying = false;
				onEndActuator?.Invoke(actuator, result);
			}
		}
		public void OnActivity(ObjectBehaviour Order, Vector3 targetPosition, ObjectBehaviour targetObject, ActuatorPlayCallback actuatorPlayCallback = null)
		{
			if(actuatorPlayCallback == null) actuatorPlayCallback = new ActuatorPlayCallback();
			if(Basic == null || Order == null)
			{
				actuatorPlayCallback.OnEndActuator(this, false);
				return;
			}

			if(Target.isNonTarget || targetObject == null)
			{
				IsPlaying = true;
				IsActivity = true;

				OnActivity(Order, new ActuatorTargetParameter(targetPosition), actuatorPlayCallback);
			}
			else
			{
				IsPlaying = true;
				IsActivity = true;
				OnActivity(Order, new ActuatorTargetParameter(targetObject), actuatorPlayCallback);
			}

		}
		[ButtonGroup("Test Cancel"), PropertyOrder(-98)]
		[EnableIf("@IsPlaying")]
		public void OnActivityCancel()
		{
			IsPlaying = false;
			CancelAwaitable?.Invoke();
		}
		private async void OnActivity(ObjectBehaviour Order, ActuatorTargetParameter Target, ActuatorPlayCallback actuatorPlayCallback)
		{
			PlayingLog = "Play";
			if(actuatorPlayCallback == null) actuatorPlayCallback = new ActuatorPlayCallback();
			if(Player == null)
			{
				actuatorPlayCallback.OnEndActuator(this, false);
				return;
			}

			bool next = true;

			int shieldCost = Basic.shieldCost;
			int materialCost = Basic.materialCost;
			int batteryCost = Basic.batteryCost;
			int ammoCost = Basic.ammoCost;

			if(!IsCostValidation(Order))
			{
				actuatorPlayCallback.OnEndActuator(this, false);
				return;
			}
			Awaitable<bool> awaitPlayer = null;
			Awaitable waitAnimPlayEnd = null;
			Awaitable waitTimePlayEnd = null;
			Awaitable<bool> parallelWaitAll = null;
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			CancellationToken cancellationToken = cancellationTokenSource .Token;
			CancelAwaitable = () => {
				if(cancellationTokenSource != null)
				{
					cancellationTokenSource.Cancel();
					cancellationTokenSource.Dispose();
					cancellationTokenSource = null;
				}
			};

			var activePlayer = ScriptableObject.Instantiate(Player);

			awaitPlayer = activePlayer.OnStart(cancellationToken, Order, this, Target, () => !CheckIsPlaying());

			next = await awaitPlayer;

			if(!CheckIsPlaying())
			{
				PlayingLog = "Player.OnStart False";
				actuatorPlayCallback.OnEndActuator(this, false);
				Destroy(activePlayer);
				return;
			}

			AnimationClip originalCastingClip = Basic.originalCastingClip;
			AnimationClip originalActionClip = Basic.originalActionClip;
			AnimationClip originalCooldownClip = Basic.originalCooldownClip;
			AnimationClip castingClip = Basic.castingClip;
			AnimationClip actionClip = Basic.actionClip;
			AnimationClip cooldownClip = Basic.cooldownClip;
			float castingTime = Basic.castingTime;
			float actionTime = Basic.actionTime;
			float cooldownTime = Basic.cooldownTime;
			TimingOfPayment shieldCostPay = Basic.shieldCostPay;
			TimingOfPayment materialCostPay = Basic.materialCostPay;
			TimingOfPayment batteryCostPay =Basic.batteryCostPay;
			TimingOfPayment ammoCostPay = Basic.ammoCostPay;
			int castingStateValue = 0;
			int actionStateValue = 1;
			int cooldownStateValue = 2;
			int cancelAndIdleHash = 0;
			int startActuatorHash = 0;
			int actuatorStateHash = 0;
			int castingStateNameHash  = 0;
			int actionStateNameHash   = 0;
			int cooldownStateNameHash = 0;
			bool playAnim = false;
			if(Order.ThisContainer.TryGetComponent<AnimatorComponent>(out var animatorComponent))
			{
				cancelAndIdleHash = animatorComponent.StringToHash("CancelAndIdle");
				startActuatorHash = animatorComponent.StringToHash("StartActuator");
				actuatorStateHash = animatorComponent.StringToHash("ActuatorState");

				castingStateNameHash  = animatorComponent.StringToHash("Base Layer.ActuatorPlayer.Casting");
				actionStateNameHash   = animatorComponent.StringToHash("Base Layer.ActuatorPlayer.Action");
				cooldownStateNameHash = animatorComponent.StringToHash("Base Layer.ActuatorPlayer.Cooldown");
			}

			if(CheckIsPlaying())
			{
				PlayingLog = "AutoCostPay.StartCastingTime";
				next = AutoCostPay(Order, TimingOfPayment.StartCastingTime);
			}
			if(CheckIsPlaying())
			{
				PlayingLog = "Player.OnCasting";
				actuatorPlayCallback.OnEnterCasting(this);

				waitAnimPlayEnd = WaitAnimationPlay(cancellationToken, animatorComponent, startActuatorHash, actuatorStateHash, cancelAndIdleHash, castingClip, castingStateValue, castingStateNameHash);
				waitTimePlayEnd = WaitActuatorTime(cancellationToken, castingTime);
				awaitPlayer = activePlayer.OnCasting();
				if(!playAnim) playAnim = waitAnimPlayEnd != null;

				parallelWaitAll = AwaitableUtility.ParallelWaitAll<bool>(cancellationToken, awaitPlayer, waitAnimPlayEnd, waitTimePlayEnd);
				next = await parallelWaitAll;
			}
			if(CheckIsPlaying())
			{
				PlayingLog = "AutoCostPay.StartActionTime";
				next = AutoCostPay(Order, TimingOfPayment.StartActionTime);
			}
			if(CheckIsPlaying())
			{
				PlayingLog = "Player.OnAction";
				actuatorPlayCallback.OnEnterAction(this);

				waitAnimPlayEnd = WaitAnimationPlay(cancellationToken, animatorComponent, startActuatorHash, actuatorStateHash, cancelAndIdleHash, actionClip, actionStateValue, actionStateNameHash);
				waitTimePlayEnd = WaitActuatorTime(cancellationToken, actionTime);
				awaitPlayer = activePlayer.OnAction();
				if(!playAnim) playAnim = waitAnimPlayEnd != null;

				parallelWaitAll = AwaitableUtility.ParallelWaitAll<bool>(cancellationToken, awaitPlayer, waitAnimPlayEnd, waitTimePlayEnd);
				next = await parallelWaitAll;
			}

			if(CheckIsPlaying())
			{
				PlayingLog = "AutoCostPay.StartCooldownTime";
				next = AutoCostPay(Order, TimingOfPayment.StartCooldownTime);
			}
			if(CheckIsPlaying())
			{
				PlayingLog = "Player.OnCooldown";
				actuatorPlayCallback.onEnterCooldown(this);

				waitAnimPlayEnd = WaitAnimationPlay(cancellationToken, animatorComponent, startActuatorHash, actuatorStateHash, cancelAndIdleHash, cooldownClip, cooldownStateValue, cooldownStateNameHash);
				waitTimePlayEnd = WaitActuatorTime(cancellationToken, cooldownTime);
				awaitPlayer = activePlayer.OnCooldown();
				if(!playAnim) playAnim = waitAnimPlayEnd != null;

				parallelWaitAll = AwaitableUtility.ParallelWaitAll<bool>(cancellationToken, awaitPlayer, waitAnimPlayEnd, waitTimePlayEnd);
				next = await parallelWaitAll;
			}
			if(CheckIsPlaying())
			{
				PlayingLog = "AutoCostPay.EndedCooldownTime";
				next = AutoCostPay(Order, TimingOfPayment.EndedCooldownTime);
			}
			bool result = CheckIsPlaying();

			if(result)
			{
				EndedAnimation(animatorComponent, startActuatorHash, actuatorStateHash, cancelAndIdleHash);
				activePlayer.EndedActuator();
				PlayingLog = "Play EndedActuator";
			}
			else
			{
				CancelAnimation(animatorComponent, startActuatorHash, actuatorStateHash, cancelAndIdleHash);
				activePlayer.CancelActuator();
				PlayingLog = "Play CancelActuator";
			}
			await Awaitable.NextFrameAsync();
			CancelAwaitable.Invoke();
			CancelAwaitable = null;
			actuatorPlayCallback.OnEndActuator(this, result);


			bool CheckIsPlaying()
			{
				return next && IsPlaying && cancellationToken.CanBeCanceled && Order != null  && Order.DisableCancelToken.CanBeCanceled && Order.DestroyCancelToken.CanBeCanceled;
			}
			bool IsCostValidation(ObjectBehaviour Order)
			{
				if(!Order.ThisContainer.TryGetComponent<AbilityProvider>(out var ability)) return false;

				int shieldPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.ShieldPoint));
				int materialPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.MaterialPoint));
				int batteryPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.BatteryPoint));
				int ammoCount = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.AmmoCount));

				if(shieldPoint - shieldCost < 1) return false;
				if(materialPoint - materialCost < 0) return false;
				if(batteryPoint - batteryCost < 0) return false;
				if(ammoCount - ammoCost < 0) return false;

				return true;
			}
			bool AutoCostPay(ObjectBehaviour Order, TimingOfPayment timingOfPayment)
			{
				if(!Order.ThisContainer.TryGetComponent<AbilityProvider>(out var ability)) return false;

				if(shieldCostPay == timingOfPayment)
				{
					int shieldPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.ShieldPoint));
					if(shieldPoint - shieldCost < 1) return false;
				}
				if(materialCostPay == timingOfPayment)
				{
					int materialPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.MaterialPoint));
					if(materialPoint - materialPoint < 0) return false;
				}
				if(batteryCostPay == timingOfPayment)
				{
					int batteryPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.BatteryPoint));
					if(batteryPoint - batteryPoint < 0) return false;
				}
				if(ammoCostPay == timingOfPayment)
				{
					int ammoCount = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.AmmoCount));
					if(ammoCount - ammoCost < 0) return false;
				}

				/// 이 곳에서 실제 비용 지불을 해야 함

				return true;
			}
			async Awaitable WaitActuatorTime(CancellationToken cancellationToken, float waitTime)
			{
				if(waitTime<=0) return;
				await Awaitable.WaitForSecondsAsync(waitTime, cancellationToken);
			}
			void StartAnimation(AnimatorComponent animatorComponent, int startActuatorHash, int cancelAndIdleHash)
			{
				if(animatorComponent == null) return;

				animatorComponent.SetBool(startActuatorHash, true);
				animatorComponent.ResetTrigger(cancelAndIdleHash);

				List<KeyValuePair<AnimationClip, AnimationClip>> animationKeyValue = new List<KeyValuePair<AnimationClip, AnimationClip>>(){
				new (originalCastingClip,castingClip),
				new (originalActionClip,actionClip),
				new (originalCooldownClip,cooldownClip),
			};
				animatorComponent.OverrideAnimationClipList(animationKeyValue);
			}
			void EndedAnimation(AnimatorComponent animatorComponent, int startActuatorHash, int actuatorStateHash, int cancelAndIdleHash)
			{
				if(animatorComponent == null) return;

				animatorComponent.SetBool(startActuatorHash, false);
				animatorComponent.SetInteger(actuatorStateHash, 0);
				animatorComponent.ResetTrigger(cancelAndIdleHash);
			}
			void CancelAnimation(AnimatorComponent animatorComponent, int startActuatorHash, int actuatorStateHash, int cancelAndIdleHash)
			{
				if(animatorComponent == null) return;

				animatorComponent.SetBool(startActuatorHash, false);
				animatorComponent.SetInteger(actuatorStateHash, 0);
				animatorComponent.SetTrigger(cancelAndIdleHash);
			}
			async Awaitable WaitAnimationPlay(CancellationToken cancellationToken, AnimatorComponent animatorComponent, int startActuatorHash, int actuatorStateHash, int cancelAndIdleHash, AnimationClip animationClip, int actuatorStateValue, int waitStateNameHash)
			{
				if(animatorComponent != null && animationClip != null)
				{
					StartAnimation(animatorComponent, startActuatorHash, cancelAndIdleHash);

					animatorComponent.SetInteger(actuatorStateHash, actuatorStateValue);
					await animatorComponent.WaitAnimatorStateExit(cancellationToken, waitStateNameHash, 0, 1f, animationClip.length);
				}
				else
				{
					waitAnimPlayEnd = null;
				}
			}
		}
	}
}
