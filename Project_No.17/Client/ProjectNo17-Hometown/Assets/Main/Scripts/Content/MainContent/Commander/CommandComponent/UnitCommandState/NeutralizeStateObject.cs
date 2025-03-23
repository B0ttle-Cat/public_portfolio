using BC.OdccBase;

namespace TFContent
{
	public partial class UnitCommandComponent
	{
		protected partial class NeutralizeStateObject : UnitCommandStateObject
		{
			public NeutralizeStateObject(UnitCommandComponent unitCommandComponent) : base(unitCommandComponent) { }

			public override UnitCommandData.UnitCommandStateType CheckNextStateType()
			{
				return UnitCommandData.UnitCommandStateType.Neutralize;
			}

			protected override void DisableState()
			{
				if(UnitCommand.ThisContainer.TryGetComponent<CharacterAnimatorComponent>(out var characterAnimator))
				{
					characterAnimator.OnCancelAndIdle(false);
					characterAnimator.OnNeutralize(false);
				}
			}
			protected override void EnableState()
			{
				if(UnitCommand.ThisContainer.TryGetComponent<CharacterAnimatorComponent>(out var characterAnimator))
				{
					characterAnimator.OnCancelAndIdle();
					characterAnimator.OnNeutralize(true);
				}
			}
			protected override void UpdateState()
			{

			}
		}
	}
}