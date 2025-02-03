using System.Collections.Generic;

using BC.ODCC;

using Pathfinding;

using UnityEngine;
namespace TFContent
{
	public class UnitCommandable : ComponentBehaviour, IOdccUpdate
	{
		private FollowerEntity moveAI;
		private AnimatorController characteAnim;

		public FollowerEntity MoveAI { get => moveAI; private set => moveAI=value; }
		public AnimatorController CharacterAnim { get => characteAnim; private set => characteAnim=value; }

		protected override void BaseAwake()
		{
			MoveAI = ThisObject.GetComponentInChildren<FollowerEntity>();
			CharacterAnim = ThisContainer.GetComponent<AnimatorController>();
		}
		void IOdccUpdate.BaseUpdate()
		{
			List<CommandListener> commandListeners = ThisContainer.GetAllComponent<CommandListener>(c => c.isActiveAndEnabled);
			int length = commandListeners.Count;
			for(int i = 0 ; i < length ; i++)
			{
				commandListeners[i].CommandUpdate();
			}
		}

		public bool OnMovePosition(in Vector3 position, in Vector3 look)
		{
			if(MoveAI == null) return false;
			MoveAI.SetDestination(position, look);
			MoveAI.isStopped = false;
			return true;
		}
	}
}