using System.Collections.Generic;
using System.Linq;

using BC.Actuator;
using BC.ODCC;
using BC.OdccBase;

namespace BC.AIProvider
{
	public interface ICombatCompute : IOdccComponent
	{
		public int FindObjectIndex(ObjectBehaviour target);
		public ObjectBehaviour FindObjectBehaviour(int index);
		public AbilityProvider FindAbilityProvider(int index);
		public ActuatorProvider FindActuatorProvider(int index);
		public IFFProvider FindIFFProvider(int index);
		public ReactionTarget FindReactionTarget(int index);
		public AbilityProvider FindAbilityProvider(ObjectBehaviour target) => FindAbilityProvider(FindObjectIndex(target));
		public ActuatorProvider FindActuatorProvider(ObjectBehaviour target) => FindActuatorProvider(FindObjectIndex(target));
		public IFFProvider FindIFFProvider(ObjectBehaviour target) => FindIFFProvider(FindObjectIndex(target));
		public ReactionTarget FindReactionTarget(ObjectBehaviour target) => FindReactionTarget(FindObjectIndex(target));

		public UnitRelationType GetRelationType(int thisUnitIndex, int targetUnitIndex);
		public bool IsVisibleTarget(int thisUnitIndex, int targetUnitIndex);
		public bool IsReactionTarget(int thisUnitIndex, int targetUnitIndex);
		public bool GetUnitPairsValue(int thisUnitIndex, int targetUnitIndex, out UnitPairsTarget pairsResult);
		public bool GetUnitPairsValue(int thisUnitIndex, int[] targetUnitIndexs, out UnitPairsTarget[] pairsResult);
		public bool GetUnitAllPairsValue(int thisUnitIndex, out UnitPairsTarget[] allPairsResult);
		public UnitRelationType GetRelationType(ObjectBehaviour thisUnit, ObjectBehaviour targetUnit)
		{
			int thisUnitIndex = FindObjectIndex(thisUnit);
			if(thisUnitIndex < 0) return UnitRelationType.Neutral;
			int targetUnitIndex = FindObjectIndex(targetUnit);
			if(targetUnitIndex < 0) return UnitRelationType.Neutral;

			return GetRelationType(thisUnitIndex, targetUnitIndex);
		}
		public bool IsVisibleTarget(ObjectBehaviour thisUnit, ObjectBehaviour targetUnit)
		{
			int thisUnitIndex = FindObjectIndex(thisUnit);
			if(thisUnitIndex < 0) return false;
			int targetUnitIndex = FindObjectIndex(targetUnit);
			if(targetUnitIndex < 0) return false;

			return IsVisibleTarget(thisUnitIndex, targetUnitIndex);
		}
		public bool IsReactionTarget(ObjectBehaviour thisUnit, ObjectBehaviour targetUnit)
		{
			int thisUnitIndex = FindObjectIndex(thisUnit);
			if(thisUnitIndex < 0) return false;
			int targetUnitIndex = FindObjectIndex(targetUnit);
			if(targetUnitIndex < 0) return false;

			return IsReactionTarget(thisUnitIndex, targetUnitIndex);
		}
		public bool GetUnitPairsValue(ObjectBehaviour thisUnit, ObjectBehaviour targetUnit, out UnitPairsTarget unitPairsTarget)
		{
			unitPairsTarget = default;
			int thisUnitIndex = FindObjectIndex(thisUnit);
			if(thisUnitIndex < 0) return false;
			int targetUnitIndex = FindObjectIndex(targetUnit);
			if(targetUnitIndex < 0) return false;

			return GetUnitPairsValue(thisUnitIndex, targetUnitIndex, out unitPairsTarget);
		}
		public bool GetUnitPairsValue(ObjectBehaviour thisUnit, List<ObjectBehaviour> targetUnits, out UnitPairsTarget[] pairsResult)
		{
			pairsResult = null;
			int thisUnitIndex = FindObjectIndex(thisUnit);
			if(thisUnitIndex < 0) return false;
			int[] targetUnitIndexs = targetUnits.Select(i=>FindObjectIndex(i)).ToArray();
			if(targetUnitIndexs == null || targetUnitIndexs.Length == 0) return false;

			return GetUnitPairsValue(thisUnitIndex, targetUnitIndexs, out pairsResult);
		}
		public bool GetUnitAllPairsValue(ObjectBehaviour thisUnit, out UnitPairsTarget[] allPairsResult)
		{
			allPairsResult = null;
			int thisUnitIndex = FindObjectIndex(thisUnit);
			if(thisUnitIndex < 0) return false;

			return GetUnitAllPairsValue(thisUnitIndex, out allPairsResult);
		}
	}
}
