using TFSystem.SaveLoad;

namespace TFContent
{
	public partial class UnitCommandComponent //.NeutralizeStateObject
	{
		protected partial class NeutralizeStateObject : UnitCommandStateObject
		{
			public NeutralizeStateObject(UnitCommandComponent unitCommandComponent) : base(unitCommandComponent) { }

			public override UnitCommandData.UnitCommandStateType CheckNextStateUpdate(in UnitCommandData.UnitCommandStateType prevState)
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

			public override void OnSave(ref SaveLoadData data)
			{
			}

			public override void OnLoad(in SaveLoadData data)
			{
			}
		}
	}
}