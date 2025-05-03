using BC.ODCC;

using Pathfinding;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using UnityEngine;

namespace TFContent
{
	public interface IMovePositionListener : IOdccComponent
	{
		public void OnMovePosition(in Vector3 position, in Vector3 look);
		public void OnSetPosition(in Vector3 position, in Vector3 look);
		public bool IsPathPossible(GraphNode node);
	}
	public partial class MovementCommandUpdater : CommandUpdater
	{
		private UnitCommandComponent unitCommand;
		private FollowerEntity moveAI;
		private CharacterAnimatorComponent characterAnimator;
		public UnitCommandComponent UnitCommand => unitCommand;
		public FollowerEntity MoveAI => moveAI;
		public CharacterAnimatorComponent CharacterAnimator => characterAnimator;


		private bool isAliveMoveTarget;
		[ShowInInspector,ReadOnly]
		private float desiredVelocity;

		[ShowInInspector,ReadOnly]
		private bool animatorIsMove;
		[ShowInInspector,ReadOnly]
		private float animatorMoveSpeed;


		public bool AnimatorIsMove {
			get => animatorIsMove;
			set {
				if(animatorIsMove!=value)
				{
					animatorIsMove = value;
					if(CharacterAnimator != null)
					{
						CharacterAnimator.SetMovement(value);
					}
				}
			}
		}
		public float AnimatorMoveSpeed {
			get => animatorMoveSpeed;
			set {
				if(Mathf.Abs(animatorMoveSpeed - value) > float.Epsilon)
				{
					animatorMoveSpeed = value;
					if(CharacterAnimator != null)
					{
						CharacterAnimator.SetMovement(value);
					}
				}
			}
		}

		protected override void BaseAwake()
		{
			ThisContainer.TryGetComponent<UnitCommandComponent>(out unitCommand);
			ThisContainer.TryGetComponent<CharacterAnimatorComponent>(out characterAnimator);
			moveAI = ThisObject.GetComponentInChildren<FollowerEntity>();

			AnimatorIsMove = false;
			AnimatorMoveSpeed = 0f;
		}
		public override void OnCommandUpdater()
		{
			if(MoveAI == null) return;

			desiredVelocity = MoveAI.desiredVelocity.magnitude;
			isAliveMoveTarget = MoveAI.reachedDestination|| MoveAI.reachedEndOfPath;

			AnimatorIsMove = IsMovement();
			AnimatorMoveSpeed = desiredVelocity;
		}

		public void OnMovePosition(in Vector3 position, in Vector3 look)
		{
			if(MoveAI == null) return;
			Vector3 checkPosition = MoveAI.destination;
			Vector3 checkDirection = MoveAI.DestinationFacingDirection;
			if(Vector3.Distance(checkPosition, position) <= 0.1f && Vector3.Angle(checkDirection, look) <= 0.5f)
			{
				return;
			}

			MoveAI.SetDestination(position, look);
			MoveAI.isStopped = false;
		}
		public void OnSetPosition(in Vector3 position, in Vector3 look, bool clearPath = true)
		{
			if(MoveAI == null) return;
			var lookRotation = Quaternion.LookRotation(look);

			MoveAI.Teleport(position, clearPath);
			MoveAI.rotation = lookRotation;

			MoveAI.velocity = Vector3.zero;
			MoveAI.rotationSpeed = 0f;
			MoveAI.isStopped = false;
		}
		internal void OnMoveStop(bool clearPath = true)
		{
			MoveAI.isStopped = true;
			MoveAI.Teleport(MoveAI.position, clearPath);
			MoveAI.rotation = MoveAI.rotation;
		}

		public float StopDistance()
		{
			return MoveAI.stopDistance;
		}
		public bool IsArriveMoveTarget()
		{
			return isAliveMoveTarget;
		}
		public bool IsPathPossible(GraphNode node)
		{
			if(MoveAI == null) return false;
			return PathUtilities.IsPathPossible(MoveAI.currentNode, node);
		}
		public bool HasMoveTarget()
		{
			return UnitCommand.CommandData.hasMoveTarget;
		}
		public bool IsMovement(float velocityOffset = float.Epsilon)
		{
			return desiredVelocity >= velocityOffset;
		}
		internal bool IsLookTarget(float angleOffset = 0.1f)
		{
			Vector3 look = MoveAI.rotation * Vector3.forward;
			Vector3 lookTarget = MoveAI.DestinationFacingDirection;
			return Vector3.Angle(look, lookTarget) < angleOffset;
		}
	}

	public partial class MovementCommandUpdater : ISaveLoadTarget
	{
		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
			data.SaveValue(nameof(isAliveMoveTarget), isAliveMoveTarget);
			data.SaveValue(nameof(desiredVelocity), desiredVelocity);
			data.SaveValue(nameof(animatorIsMove), animatorIsMove);
			data.SaveValue(nameof(animatorMoveSpeed), animatorMoveSpeed);
			data.SaveValue(nameof(moveAI), JsonUtility.ToJson(moveAI));
		}
		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			if(data.LoadValue<float>(nameof(isAliveMoveTarget), out var _isAliveMoveTarget))
			{
				desiredVelocity = _isAliveMoveTarget;
			}
			if(data.LoadValue<float>(nameof(desiredVelocity), out var _desiredVelocity))
			{
				desiredVelocity = _desiredVelocity;
			}
			if(data.LoadValue<bool>(nameof(animatorIsMove), out var _animatorIsMove))
			{
				AnimatorIsMove = _animatorIsMove;
			}
			if(data.LoadValue<float>(nameof(animatorMoveSpeed), out var _animatorMoveSpeed))
			{
				AnimatorMoveSpeed = _animatorMoveSpeed;
			}
			if(data.LoadValue(nameof(moveAI), out var _moveAI))
			{
				JsonUtility.FromJsonOverwrite(_moveAI, moveAI);
			}
		}
	}
}