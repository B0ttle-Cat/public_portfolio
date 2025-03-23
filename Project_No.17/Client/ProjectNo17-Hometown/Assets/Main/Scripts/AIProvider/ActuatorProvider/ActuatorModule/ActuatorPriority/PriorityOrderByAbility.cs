using BC.OdccBase;

using UnityEngine;

namespace BC.AIProvider
{
	public partial class ActuatorPriorityOrder //.PriorityOrderByAbility
	{
		public class PriorityOrderByAbility : ActuatorPriorityOrderBy
		{
			protected override string Description => $"{TargetAbility} 값이 {(reverseOrder ? "작을" : "클")} 수록 더 높은 우선순위(값)를 가져갑니다.";

			[SerializeField]
			private AbilityType TargetAbility;

			protected override float PriorityUpdate(ICombatCompute combatCompute, in UnitPairsTarget unitPair)
			{
				return combatCompute.FindAbilityProvider(unitPair.targetIndex).GetAbilityValue(TargetAbility);
			}
		}
	}
}
