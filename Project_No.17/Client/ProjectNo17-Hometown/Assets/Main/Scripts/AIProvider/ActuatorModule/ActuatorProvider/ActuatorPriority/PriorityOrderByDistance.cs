using BC.AIProvider;

namespace BC.Actuator
{
	public partial class ActuatorPriorityOrder //.PriorityOrderByDistance
	{
		public class PriorityOrderByDistance : ActuatorPriorityOrderBy
		{
			protected override string Description => $"거리가 {(reverseOrder ? "가까울" : "멀")} 수록 더 높은 우선순위(값)를 가져갑니다.";
			protected override float PriorityUpdate(ICombatCompute combatCompute, in UnitPairsTarget unitPair)
			{
				return unitPair.spaceDistance;
			}
		}
	}
}
