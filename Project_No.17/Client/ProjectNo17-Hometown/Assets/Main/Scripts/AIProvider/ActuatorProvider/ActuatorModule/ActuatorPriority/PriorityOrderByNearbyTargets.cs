namespace BC.AIProvider
{
	public partial class ActuatorPriorityOrder //.DistanceOrderBy
	{
		public class PriorityOrderByNearbyTargets : ActuatorPriorityOrderBy
		{
			public float radius = 1;
			protected override string Description => $"(구현 예정) 반경내({radius}) 목표물 수가 {(reverseOrder ? "적을" : "많을")} 수록 더 높은 우선순위(값)를 가져갑니다.";
			protected override float PriorityUpdate(ICombatCompute combatCompute, in UnitPairsTarget unitPair)
			{
				return 0;
			}
		}
	}
}
