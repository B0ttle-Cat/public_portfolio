using TFSystem.SaveLoad;

namespace TFContent
{
	public partial class UnitCommandComponent //.IdleStateObject
	{
		protected partial class IdleStateObject : UnitCommandStateObject
		{
			public IdleStateObject(UnitCommandComponent unitCommandComponent) : base(unitCommandComponent) { }

			public override UnitCommandData.UnitCommandStateType CheckNextStateUpdate(in UnitCommandData.UnitCommandStateType prevState)
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

			public override void OnSave(ref SaveLoadData data)
			{
			}

			public override void OnLoad(in SaveLoadData data)
			{
			}
		}
	}
}