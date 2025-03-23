namespace BC.OdccBase
{
	public partial class CharacterAnimatorComponent : AnimatorComponent//, IOdccUpdate
	{
		#region Custom API Group
		protected override void BaseAwake()
		{
			base.BaseAwake();
		}
		public void SetMovement(bool IsMove)
		{
			if(Animator.runtimeAnimatorController == null) return;
			SetBool(nameof(IsMove), IsMove);
		}
		public void SetMovement(float MoveSpeed)
		{
			if(Animator.runtimeAnimatorController == null) return;
			SetFloat(nameof(MoveSpeed), MoveSpeed);
		}
		public void SetMovement(bool IsMove, float MoveSpeed)
		{
			if(Animator.runtimeAnimatorController == null) return;
			SetBool(nameof(IsMove), IsMove);
			SetFloat(nameof(MoveSpeed), MoveSpeed);
		}

		public void OnNeutralize(bool IsNeutralize)
		{
			if(Animator.runtimeAnimatorController == null) return;
			SetBool(nameof(IsNeutralize), IsNeutralize);
		}

		internal void OnCancelAndIdle(bool CancelAndIdle = true)
		{
			if(Animator.runtimeAnimatorController == null) return;
			if(CancelAndIdle)
			{
				SetTrigger(nameof(CancelAndIdle));
			}
			else
			{
				ResetTrigger(nameof(CancelAndIdle));
			}
		}
		#endregion
	}
}