using System;

using BC.ODCC;
using BC.OdccBase;

using UnityEngine;

namespace BC.Actuator
{
	public class ActuatorClipEvent : ComponentBehaviour
		, IAnimatorStateCheckListener
		, IMachineStateCheckListener
	{
		public ActuatorPlayer actuatorPlayer { get; set; }

		protected override void BaseAwake()
		{
			FindActuatorPlayer();
		}
		protected override void BaseDestroy()
		{
			actuatorPlayer = null;
		}

		private bool HasActuatorPlayer()
		{
			return actuatorPlayer != null;
		}
		private bool FindActuatorPlayer()
		{
			if(HasActuatorPlayer()) return true;

			IOdccContainer iOdccContainer = GetComponentInParent<IOdccContainer>();
			if(iOdccContainer == null || iOdccContainer.ThisContainer == null) return false;

			if(iOdccContainer.ThisContainer.TryGetComponent<ActuatorPlayer>(out var _actuatorPlayer))
			{
				actuatorPlayer = _actuatorPlayer;
			}
			else
			{
				actuatorPlayer = null;
			}
			return actuatorPlayer != null;
		}
		private void _OnActuatorTrigger(IStateMachineListener.AnimationEventLabel animationEvent)
		{
			if(!HasActuatorPlayer()) return;
			actuatorPlayer.OnActuatorTrigger(animationEvent);
		}
		private void _OnActuatorTrigger(AnimationEvent animationEvent)
		{
			if(animationEvent == null) return;
			_OnActuatorTrigger(new IStateMachineListener.AnimationEventLabel(animationEvent));
		}

		public void OnActuatorTrigger(AnimationEvent animationEvent)
		{
			_OnActuatorTrigger(animationEvent);
		}
		public void ActuatorTrigger(AnimationEvent animationEvent)
		{
			_OnActuatorTrigger(animationEvent);
		}
		public void OnActuator(AnimationEvent animationEvent)
		{
			_OnActuatorTrigger(animationEvent);
		}
		public void Actuator(AnimationEvent animationEvent)
		{
			_OnActuatorTrigger(animationEvent);
		}
		public void OnTrigger(AnimationEvent animationEvent)
		{
			_OnActuatorTrigger(animationEvent);
		}
		public void Trigger(AnimationEvent animationEvent)
		{
			_OnActuatorTrigger(animationEvent);
		}
		public void OnAction(AnimationEvent animationEvent)
		{
			_OnActuatorTrigger(animationEvent);
		}
		public void Action(AnimationEvent animationEvent)
		{
			_OnActuatorTrigger(animationEvent);
		}
		public void OnEvent(AnimationEvent animationEvent)
		{
			_OnActuatorTrigger(animationEvent);
		}
		public void Event(AnimationEvent animationEvent)
		{
			_OnActuatorTrigger(animationEvent);
		}

		void IAnimatorStateCheckListener.OnAnimatorStateEnter(AnimatorStateInfo stateInfo, IStateMachineListener.AnimationEventLabel animationEvent)
		{
			if(animationEvent == null || !IsEventKeyEnabled(animationEvent.functionName)) return;
			_OnActuatorTrigger(animationEvent);
		}

		void IAnimatorStateCheckListener.OnAnimatorStateExit(AnimatorStateInfo stateInfo, IStateMachineListener.AnimationEventLabel animationEvent)
		{
			if(animationEvent == null || !IsEventKeyEnabled(animationEvent.functionName)) return;
			_OnActuatorTrigger(animationEvent);
		}

		void IMachineStateCheckListener.OnMachineStateEnter(int stateMachinePathHash, IStateMachineListener.AnimationEventLabel animationEvent)
		{
			if(animationEvent == null || !IsEventKeyEnabled(animationEvent.functionName)) return;
			_OnActuatorTrigger(animationEvent);
		}

		void IMachineStateCheckListener.OnMachineStateExit(int stateMachinePathHash, IStateMachineListener.AnimationEventLabel animationEvent)
		{
			if(animationEvent == null || !IsEventKeyEnabled(animationEvent.functionName)) return;
			_OnActuatorTrigger(animationEvent);
		}

		private bool IsEventKeyEnabled(string eventKey)
		{
			if(string.IsNullOrWhiteSpace(eventKey))
				return false;

			return Enum.TryParse<IStateMachineListener.EventFunctionNameList>(eventKey, out _);
		}
	}
}
