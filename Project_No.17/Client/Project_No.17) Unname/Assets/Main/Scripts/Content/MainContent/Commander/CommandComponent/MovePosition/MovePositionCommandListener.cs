using System.Collections.Generic;

using Pathfinding;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFContent
{
	public class MovePositionCommandListener : CommandListener<(NNInfo nnInfo, Vector3 look)>
	{
		private UnitCommandable unitCommandable;

		[SerializeField]
		private Queue<(NNInfo nnInfo, Vector3 dir)> movePositions;

		[ShowInInspector,ReadOnly]
		private bool isPathCanMove;
		[ShowInInspector,ReadOnly]
		private bool isAnimCanMove;

		protected override void BaseAwake()
		{
			unitCommandable = ThisContainer.GetComponent<UnitCommandable>();
			movePositions = new Queue<(NNInfo nnInfo, Vector3 dir)>();

			isPathCanMove = false;
			isAnimCanMove = false;
		}

		public override void CommandUpdate()
		{
			if(unitCommandable == null)
			{
				if(!ThisContainer.TryGetComponent<UnitCommandable>(out unitCommandable)) return;
			}
			FollowerEntity moveAI = unitCommandable.MoveAI;
			if(moveAI == null) return;

			isPathCanMove = UpdateMovePose(moveAI);

			float moveAnimSpeed = Mathf.Min(moveAI.velocity.magnitude, moveAI.desiredVelocity.magnitude);
			isAnimCanMove = UpdateMoveAnim(unitCommandable.CharacterAnim, isPathCanMove, moveAnimSpeed);

			moveAI.isStopped = !isPathCanMove || !isAnimCanMove;
		}
		private bool UpdateMovePose(FollowerEntity moveAI)
		{
			if(!moveAI.canMove || moveAI.isStopped)
			{
				return false;
			}
			else if(!moveAI.hasPath)
			{
				return DequeueDestination(moveAI);
			}
			else if(!moveAI.isStopped && (moveAI.reachedDestination||moveAI.reachedEndOfPath))
			{
				return DequeueDestination(moveAI);
			}
			else
			{
				return true;
			}
		}
		private bool UpdateMoveAnim(AnimatorController characterAnim, bool move, float velocity)
		{
			if(characterAnim == null)
			{
				characterAnim = ThisContainer.GetComponent<AnimatorController>();
				if(characterAnim == null) return false;
			}
			characterAnim.SetBool("IsMove", move);
			characterAnim.SetFloat("MoveSpeed", velocity);
			return characterAnim.CharacterCanMoveState();
		}
		private bool DequeueDestination(FollowerEntity MoveAI)
		{
			while(movePositions != null && movePositions.Count > 0 && movePositions.TryDequeue(out (NNInfo nnInfo, Vector3 look) target))
			{
				GraphNode currentNode = MoveAI.currentNode;
				NNInfo nnInfo= target.nnInfo;
				Vector3 look = target.look;
				if(!PathUtilities.IsPathPossible(currentNode, nnInfo.node)) continue;

				isPathCanMove = unitCommandable.OnMovePosition(nnInfo.position, look);
				return true;
			}
			return false;
		}

		public override void OnCommanding(in (NNInfo nnInfo, Vector3 look) commandValue, in bool inQueue = false)
		{
			if(unitCommandable == null)
			{
				if(!ThisContainer.TryGetComponent<UnitCommandable>(out unitCommandable)) return;
			}
			if(movePositions == null)
			{
				movePositions = new Queue<(NNInfo nnInfo, Vector3 look)>();
			}

			var moveAI = unitCommandable.MoveAI;
			if(moveAI == null) return;
			NNInfo nnInfo= commandValue.nnInfo;
			Vector3 look = commandValue.look;
			GraphNode currentNode = moveAI.currentNode;

			if(!PathUtilities.IsPathPossible(currentNode, nnInfo.node)) return;
			if(inQueue)
			{
				movePositions.Enqueue((nnInfo, look));
			}
			else
			{
				movePositions.Clear();
				isPathCanMove = unitCommandable.OnMovePosition(nnInfo.position, look);
			}
		}
	}
}