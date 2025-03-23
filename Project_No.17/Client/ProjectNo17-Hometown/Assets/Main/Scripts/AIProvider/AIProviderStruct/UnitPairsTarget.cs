using System.Collections.Generic;

using BC.ODCC;
using BC.OdccBase;

using UnityEngine;

namespace BC.AIProvider
{
	public struct UnitPairsTarget
	{
		public ObjectBehaviour thisObject;
		public ObjectBehaviour targetObject;
		public Vector3 thisPosition;
		public Vector3 targetPosition;
		public Quaternion thisRotation;
		public Quaternion targetRotation;

		public int thisTeamID;
		public int targetTeamID;

		public int thisUnitID;
		public int targetUnitID;

		public int thisIndex;
		public int targetIndex;

		public List<ActuatorModule> canUseActuatorList;
		public UnitRelationType relationType;
		public float spaceDistance;
		public Vector3 directionToTarget;

		public bool inRangeVisible;
		public bool inRangeReaction;

		public float thisRadius;
		public float targetRadius;

		public float distance => spaceDistance + thisRadius + targetRadius;


		public bool Refresh(ICombatCompute combatCompute)
		{
			if(combatCompute.GetUnitPairsValue(thisObject, targetObject, out UnitPairsTarget pairsResult))
			{
				thisPosition = pairsResult.thisPosition;
				thisRotation = pairsResult.thisRotation;
				targetPosition = pairsResult.targetPosition;
				targetRotation = pairsResult.targetRotation;

				thisTeamID = pairsResult.thisTeamID;
				targetTeamID = pairsResult.targetTeamID;

				thisUnitID= pairsResult.thisUnitID;
				targetUnitID = pairsResult.targetUnitID;

				thisIndex = pairsResult.thisIndex;
				targetIndex = pairsResult.targetIndex;

				canUseActuatorList = pairsResult.canUseActuatorList;
				relationType = pairsResult.relationType;
				spaceDistance = pairsResult.spaceDistance;
				directionToTarget = pairsResult.directionToTarget;

				inRangeVisible = pairsResult.inRangeVisible;
				inRangeReaction = pairsResult.inRangeReaction;

				thisRadius = pairsResult.thisRadius;
				targetRadius = pairsResult.targetRadius;
				return true;
			}
			return false;
		}
	}
}
