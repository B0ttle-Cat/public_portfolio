using BC.ODCC;
using BC.OdccBase;

using Pathfinding;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFContent
{
	public interface IMovePositionListener : IOdccComponent
	{
		public void OnMovePosition(in Vector3 position, in Vector3 look);
		public void OnSetPosition(in Vector3 position, in Vector3 look);
		public bool IsPathPossible(GraphNode node);
	}
	public class MovementCommandUpdater : CommandUpdater
	{
		private UnitCommandComponent unitCommand;
		private FollowerEntity moveAI;
		private CharacterAnimatorComponent characterAnimator;

		private bool isArriveMoveTarget;
		[ShowInInspector,ReadOnly]
		private float desiredVelocity;

		[ShowInInspector,ReadOnly]
		private bool animatorIsMove;
		[ShowInInspector,ReadOnly]
		private float animatorMoveSpeed;

		public UnitCommandComponent UnitCommand { get => unitCommand; set => unitCommand=value; }
		public FollowerEntity MoveAI { get => moveAI; private set => moveAI=value; }
		public CharacterAnimatorComponent CharacterAnimator { get => characterAnimator; private set => characterAnimator=value; }
		public bool AnimatorIsMove {
			get => animatorIsMove; set {
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
			get => animatorMoveSpeed; set {
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
			UnitCommand = ThisContainer.GetComponent<UnitCommandComponent>();
			MoveAI = ThisObject.GetComponentInChildren<FollowerEntity>();
			CharacterAnimator = ThisContainer.GetComponent<CharacterAnimatorComponent>();

			AnimatorIsMove = false;
			AnimatorMoveSpeed = 0f;
		}

		#region Input
		public void OnMovePosition(in Vector3 position, in Vector3 look)
		{
			if(MoveAI == null) return;
			Vector3 checkPosition = MoveAI.destination;
			//destinationFacingDirection 은 원래 private 이지만, public 으로 수정.
			Vector3 checkDirection = MoveAI.destinationFacingDirection;
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
		#endregion

		//#region Update
		public override void OnCommandUpdater()
		{
			if(MoveAI == null) return;

			desiredVelocity = MoveAI.desiredVelocity.magnitude;
			isArriveMoveTarget = MoveAI.reachedDestination|| MoveAI.reachedEndOfPath;

			AnimatorIsMove = IsMovement();
			AnimatorMoveSpeed = desiredVelocity;
		}

		public float StopDistance()
		{
			return MoveAI.stopDistance;
		}

		public bool IsArriveMoveTarget()
		{
			return isArriveMoveTarget;
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
			Vector3 lookTarget = MoveAI.destinationFacingDirection;
			return Vector3.Angle(look, lookTarget) < angleOffset;
		}
	}
}