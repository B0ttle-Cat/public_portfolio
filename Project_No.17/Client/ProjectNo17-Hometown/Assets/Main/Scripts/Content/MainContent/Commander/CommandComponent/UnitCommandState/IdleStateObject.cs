namespace TFContent
{
	public partial class UnitCommandComponent
	{
		protected partial class IdleStateObject : UnitCommandStateObject
		{
			public IdleStateObject(UnitCommandComponent unitCommandComponent) : base(unitCommandComponent) { }

			public override UnitCommandData.UnitCommandStateType CheckNextStateType()
			{
				bool isDead = CommandData.IsDead;
				if(isDead) return UnitCommandData.UnitCommandStateType.Neutralize;

				bool hasMovePath = MovementUpdater.HasMoveTarget();
				if(hasMovePath) return UnitCommandData.UnitCommandStateType.Move;

				bool hasReactionTarget = UnitCommand.HasReactionTargetInAround();
				if(hasReactionTarget) return UnitCommandData.UnitCommandStateType.Combat;

				return UnitCommandData.UnitCommandStateType.Idle;
			}

			protected override void DisableState()
			{

			}

			protected override void EnableState()
			{
				MovementUpdater.OnMoveStop();
			}

			protected override void UpdateState()
			{

			}
		}
	}
}