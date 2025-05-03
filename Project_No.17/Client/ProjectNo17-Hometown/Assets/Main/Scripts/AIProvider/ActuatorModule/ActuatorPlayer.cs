using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

using Debug = UnityEngine.Debug;
namespace BC.Actuator
{
	public partial class ActuatorPlayer : ComponentBehaviour, IActuatorPlayer//, IOdccUpdate
	{
		[SerializeField]
		private ActuatorModule playActuatorModule;

		private ReactionTargetInfo targetValue;
	}
	public partial class ActuatorPlayer // Commander API
	{
		protected override void BaseDestroy()
		{
			if(actuatorClipEvent != null)
			{
				actuatorClipEvent.actuatorPlayer = null;
				actuatorClipEvent = null;
			}
		}

		public void OnActuatorPlay(ActuatorModule actuatorModule, Vector3 Target)
		{
			OnActuatorPlay(actuatorModule, new ReactionTargetInfo(Target));
		}
		public void OnActuatorPlay(ActuatorModule actuatorModule, ObjectBehaviour Target)
		{
			ReactionTargetInfo target = new ReactionTargetInfo(Target);
			if(actuatorModule.Target.isNonTarget)
			{
				OnActuatorPlay(actuatorModule, new ReactionTargetInfo(target.Position));
				target.Dispose();
			}
			else
			{
				OnActuatorPlay(actuatorModule, target);
			}
		}
		private void OnActuatorPlay(ActuatorModule actuatorModule, ReactionTargetInfo target)
		{
			if(actuatorModule == null)
			{
				OnActivityCancel();
				return;
			}
			if(IsPlayingActuator())
			{
				OnActivityCancel();
			}
			playActuatorModule = actuatorModule;
			targetValue = target;
			if(playActuatorModule == null)
			{
				Debug.LogError("ActuatorModule is null");
				return;
			}

			OnInitPlayer();
			OnStartPlayer();
		}
		public void OnActivityCancel()
		{
			RevertOrderParameters();
			DestroyGraph();

			animatorPlayState = ActuatorAnimatorPlayState.None;
			playActuatorModule = null;
			targetValue.Dispose();
		}
		public void OnActivityCancel_WithBlanding()
		{
			if(animatorPlayState == ActuatorAnimatorPlayState.None || animatorPlayState == ActuatorAnimatorPlayState.Ended) return;
			animatorPlayState = ActuatorAnimatorPlayState.Exit;
		}
		public bool IsPlayingActuator()
		{
			if(playActuatorModule != null) return true;
			if(animatorPlayState != ActuatorAnimatorPlayState.None) return true;
			if(graph.IsValid() && graph.IsPlaying()) return true;
			return false;
		}
	}

	public partial class ActuatorPlayer // Actuator OnInit
	{
		private const string EnterActuatorPlay = "EnterActuatorPlayer";
		//private const string ExitActuatorPlay = "ExitActuatorPlayer";
		private AnimatorComponent animatorComponent;
		private ActuatorClipEvent actuatorClipEvent;
		private Animator thisAnimator;
		private void OnInitPlayer()
		{
			OnInitAnimator();
		}
		private void OnInitAnimator()
		{
			animatorPlayState = ActuatorAnimatorPlayState.None;

			if(!playActuatorModule.HasAnimator) return;
			if(animatorComponent == null && !ThisContainer.TryGetComponent<AnimatorComponent>(out animatorComponent)) return;
			thisAnimator = animatorComponent.Animator;
			if(thisAnimator == null) return;

			if(actuatorClipEvent == null)
			{
				if(animatorComponent.Animator.TryGetComponent<ActuatorClipEvent>(out actuatorClipEvent))
				{
					actuatorClipEvent.actuatorPlayer = this;
				}
				else
				{
					actuatorClipEvent = animatorComponent.Animator.gameObject.AddComponent<ActuatorClipEvent>();
					actuatorClipEvent.actuatorPlayer = this;
				}
			}

			ActuatorPlayAnimator.OrderParameters[] orderParameters = playActuatorModule.Animator.orderEnterParameters;
			int length = orderParameters == null ? 0 : orderParameters.Length;
			for(int i = 0 ; i < length ; i++)
			{
				SetAnimatorParameters(animatorComponent, orderParameters[i]);
			}
			changeOrderParameters = true;
		}

		void SetAnimatorParameters(AnimatorComponent animatorComponent, ActuatorPlayAnimator.IAnimatorParameter iAnimatorParameter)
		{
			switch(iAnimatorParameter.Type)
			{
				case AnimatorControllerParameterType.Float:
					animatorComponent.SetFloat(iAnimatorParameter.Name, iAnimatorParameter.FloatValue);
					break;
				case AnimatorControllerParameterType.Int:
					animatorComponent.SetInteger(iAnimatorParameter.Name, iAnimatorParameter.IntValue);
					break;
				case AnimatorControllerParameterType.Bool:
					animatorComponent.SetBool(iAnimatorParameter.Name, iAnimatorParameter.BoolValue);
					break;
				case AnimatorControllerParameterType.Trigger:
					animatorComponent.SetTrigger(iAnimatorParameter.Name, iAnimatorParameter.BoolValue);
					break;
			}
		}
		void SetAnimatorParameters(AnimatorControllerPlayable playable, ActuatorPlayAnimator.IAnimatorParameter iAnimatorParameter)
		{
			switch(iAnimatorParameter.Type)
			{
				case AnimatorControllerParameterType.Float:
					playable.SetFloat(iAnimatorParameter.Name, iAnimatorParameter.FloatValue);
					break;
				case AnimatorControllerParameterType.Int:
					playable.SetInteger(iAnimatorParameter.Name, iAnimatorParameter.IntValue);
					break;
				case AnimatorControllerParameterType.Bool:
					playable.SetBool(iAnimatorParameter.Name, iAnimatorParameter.BoolValue);
					break;
				case AnimatorControllerParameterType.Trigger:
					if(iAnimatorParameter.BoolValue) playable.SetTrigger(iAnimatorParameter.Name);
					else playable.ResetTrigger(iAnimatorParameter.Name);
					break;
			}
		}
	}
	public partial class ActuatorPlayer // Actuator OnStart
	{
		private RuntimeAnimatorController orderAnimatorController;
		private RuntimeAnimatorController actuatorAnimatorController;
		private PlayableGraph graph;
		private AnimatorControllerPlayable orderPlayable;
		private AnimatorControllerPlayable actuatorPlayable;
		private AnimationLayerMixerPlayable mixer;

		private float enterBlandTime;
		private float exitBlandTime;
		public void OnStartPlayer()
		{
			OnStartAnimator();
		}
		private void OnStartAnimator()
		{
			if(!playActuatorModule.HasAnimator) return;
			if(animatorComponent == null) return;
			if(thisAnimator == null) return;

			orderAnimatorController = thisAnimator.runtimeAnimatorController;
			actuatorAnimatorController = playActuatorModule.Animator.playAnimatorController;
			if(orderAnimatorController == null) return;
			if(actuatorAnimatorController == null) return;

			enterBlandTime = playActuatorModule.Animator.enterBlandTime;
			exitBlandTime = playActuatorModule.Animator.exitBlandTime;
			animatorPlayState = ActuatorAnimatorPlayState.None;
			playingBland = 0f;

			StartGraph();
		}
		private void DestroyGraph()
		{
			if(graph.IsValid() && graph.IsPlaying() && mixer.IsValid())
			{
				//mixer.SetInputWeight(0, 1f);
				mixer.SetInputWeight(1, 0f);
			}

			if(orderPlayable.IsValid()) orderPlayable.Destroy();
			if(actuatorPlayable.IsValid()) actuatorPlayable.Destroy();
			if(mixer.IsValid()) mixer.Destroy();
			if(graph.IsValid()) graph.Destroy();
		}
		public void StartGraph()
		{
			DestroyGraph();

			animatorPlayState = ActuatorAnimatorPlayState.Enter;
			playingBland = 0f;

			graph = PlayableGraph.Create("CustomActuatorGraph");
			graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

			var output = AnimationPlayableOutput.Create(graph, "Animation", thisAnimator);

			orderPlayable = AnimatorControllerPlayable.Create(graph, orderAnimatorController);
			actuatorPlayable = AnimatorControllerPlayable.Create(graph, actuatorAnimatorController);

			var initParameters = playActuatorModule.Animator.initParameters;
			int length = initParameters == null ? 0 : initParameters.Length;
			for(int i = 0 ; i < length ; i++)
			{
				SetAnimatorParameters(actuatorPlayable, initParameters[i]);
			}

			mixer = AnimationLayerMixerPlayable.Create(graph, 2);
			graph.Connect(orderPlayable, 0, mixer, 0);
			graph.Connect(actuatorPlayable, 0, mixer, 1);

			mixer.SetInputWeight(0, 1f); // A 컨트롤러
			mixer.SetInputWeight(1, 0f); // B~D 컨트롤러 (겹쳐서 재생)

			output.SetSourcePlayable(mixer);
			graph.Play();
		}
	}

	public partial class ActuatorPlayer // Update
		: IOdccUpdate
	{
		private enum ActuatorAnimatorPlayState
		{
			None, Enter, Stay, Exit, Ended
		}
		[ShowInInspector,ReadOnly]
		private ActuatorAnimatorPlayState animatorPlayState;
		[ShowInInspector]
		private float playingBland;
		private bool changeOrderParameters;
		void IOdccUpdate.BaseUpdate()
		{
			if(animatorPlayState == ActuatorAnimatorPlayState.None) return;
			if(animatorPlayState == ActuatorAnimatorPlayState.Enter)
			{
				OnEnterAnimator();
			}
			else if(animatorPlayState == ActuatorAnimatorPlayState.Stay)
			{
				OnStayAnimator();
			}
			else if(animatorPlayState == ActuatorAnimatorPlayState.Exit)
			{
				RevertOrderParameters();
				OnExitAnimator();
			}
			else if(animatorPlayState == ActuatorAnimatorPlayState.Ended)
			{
				RevertOrderParameters();
				OnActivityCancel();
			}
		}

		private void OnEnterAnimator()
		{
			if(CheckIsExit())
			{
				animatorPlayState = ActuatorAnimatorPlayState.Exit;
				return;
			}

			float _playingBland = playingBland;
			if(enterBlandTime <= 0f)
			{
				_playingBland = 1f;
			}
			else
			{
				_playingBland += Time.deltaTime / enterBlandTime;
			}

			if(_playingBland >= 1f)
			{
				playingBland = 1f;
				animatorPlayState = ActuatorAnimatorPlayState.Stay;
			}
			else
			{
				playingBland = _playingBland;
			}

			if(graph.IsValid() && graph.IsPlaying() && mixer.IsValid())
			{
				//mixer.SetInputWeight(0, 1f-playingBland); // A 컨트롤러
				mixer.SetInputWeight(1, playingBland); // B~D 컨트롤러 (겹쳐서 재생)
			}
		}
		private void OnExitAnimator()
		{
			float _playingBland = playingBland;
			if(exitBlandTime <= 0f)
			{
				_playingBland = 0f;
			}
			else
			{
				_playingBland -= Time.deltaTime / exitBlandTime;
			}

			if(_playingBland <= 0f)
			{
				playingBland = 0f;
				animatorPlayState = ActuatorAnimatorPlayState.Ended;
			}
			else
			{
				playingBland = _playingBland;
			}

			if(graph.IsValid() && graph.IsPlaying() && mixer.IsValid())
			{
				//mixer.SetInputWeight(0, 1f-playingBland); // A 컨트롤러
				mixer.SetInputWeight(1, playingBland); // B~D 컨트롤러 (겹쳐서 재생)
			}
			else
			{
				animatorPlayState = ActuatorAnimatorPlayState.Ended;
			}
		}
		private void OnStayAnimator()
		{
			if(CheckIsExit())
			{
				animatorPlayState = ActuatorAnimatorPlayState.Exit;
			}
		}
		private bool CheckIsExit()
		{
			if(playActuatorModule == null || !graph.IsValid() || !graph.IsPlaying() || !actuatorPlayable.IsValid())
			{
				return true;
			}

			var exitState = actuatorPlayable.GetCurrentAnimatorStateInfo(0);
			if(exitState.IsName("Exit"))
			{
				float clipLength = exitState.length;
				if(clipLength <= 0f) return true;

				float normalizedTime = exitState.normalizedTime;
				if(normalizedTime >= 0.999f) return true;

				float currentTime = normalizedTime * clipLength;
				float timeRemaining = clipLength - currentTime;

				return timeRemaining <= exitBlandTime;
			}
			return false;
		}
		private void RevertOrderParameters()
		{
			if(!changeOrderParameters) return;
			changeOrderParameters = false;
			ActuatorPlayAnimator.OrderParameters[] orderParameters = playActuatorModule.Animator.orderExitParameters;
			int length = orderParameters == null ? 0 : orderParameters.Length;
			for(int i = 0 ; i < length ; i++)
			{
				SetAnimatorParameters(animatorComponent, orderParameters[i]);
			}

			thisAnimator.Update(0);
		}
	}

	public partial class ActuatorPlayer // OnActuatorTrigger => Instantiate
	{
		internal void OnActuatorTrigger(IStateMachineListener.AnimationEventLabel animationEvent)
		{
			if(animationEvent == null) return;
			TriggerOnEffector(animationEvent);
		}
		private void TriggerOnEffector(IStateMachineListener.AnimationEventLabel animationEvent)
		{
			if(playActuatorModule == null) return;
			var onTriggerEffectorObject =  playActuatorModule.Instance.GetEffectorObject(animationEvent.intParameter);

			int length = onTriggerEffectorObject == null ? 0 : onTriggerEffectorObject.Count;
			for(int i = 0 ; i < length ; i++)
			{
				playActuatorModule.Instance.OnInstantiateEffectorObject(onTriggerEffectorObject[i], ThisObject, targetValue, InstantiateCallback);
			}
		}
		private bool InstantiateCallback(ActuatorInstance.InstanceObject effectorObject, ObjectBehaviour newObject, ActuatorInstance.InstanceType effectorType)
		{
			bool isValid = effectorType switch
			{
				ActuatorInstance.InstanceType.Projectile => ProjectileInstantiate(newObject),
				ActuatorInstance.InstanceType.VFX => VFXInstantiate(newObject),
				ActuatorInstance.InstanceType.Summon => SummonInstantiate(newObject),
				_ => NoneInstantiate(newObject),
			};

			return isValid;

			bool NoneInstantiate(ObjectBehaviour newObject)
			{
				return newObject != null;
			}
			bool ProjectileInstantiate(ObjectBehaviour newObject)
			{
				if(newObject.ThisContainer.TryGetObject(out IProjectileObject projectileObject))
				{
					if(projectileObject.Init(ThisObject, playActuatorModule, effectorObject, targetValue))
					{
						projectileObject.GameObject.SetActive(true);
						projectileObject.OnFire();

						return true;
					}
				}
				return false;
			}
			bool VFXInstantiate(ObjectBehaviour newObject)
			{
				return newObject != null;
			}
			bool SummonInstantiate(ObjectBehaviour newObject)
			{
				return newObject != null;
			}
		}
	}
}
