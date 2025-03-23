namespace TFContent
{
	public partial class UnitCommandComponent
	{
		protected partial class MoveStateObject : UnitCommandStateObject
		{
			public MoveStateObject(UnitCommandComponent unitCommandComponent) : base(unitCommandComponent) { }

			bool passFirstFrame;
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
				passFirstFrame = true;
			}
			protected override void UpdateState()
			{
				UnitCommand.MovementPositionUseMoveTarget();

				if(passFirstFrame)
				{
					passFirstFrame = false;
				}
				else
				{
					if(MovementUpdater.HasMoveTarget() && MovementUpdater.IsArriveMoveTarget())
					{
						UnitCommand.CommandData.OnClearMove();
					}
				}
			}
		}
	}
}