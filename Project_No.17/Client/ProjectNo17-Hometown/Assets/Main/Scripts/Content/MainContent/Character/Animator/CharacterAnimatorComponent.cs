using BC.CMC;
using BC.OdccBase;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using UnityEngine;

namespace TFContent
{
	public partial class CharacterAnimatorComponent : AnimatorComponent//, IOdccUpdate
	{
		[SerializeField, ReadOnly]
		private CharacterModelController modelController;
		#region Custom API Group
		protected override void BaseValidate(in bool isPrefab = false)
		{
			base.BaseValidate(isPrefab);
			if(modelController == null)
				modelController = GetComponentInChildren<CharacterModelController>();
		}

		protected override void BaseAwake()
		{
			base.BaseAwake();

			if(modelController == null)
				modelController = GetComponentInChildren<CharacterModelController>();
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
		public void SetAimProgress(float IsAim)
		{
			if(Animator.runtimeAnimatorController == null) return;
			SetFloat(nameof(IsAim), IsAim);
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

	public partial class CharacterAnimatorComponent : ISaveLoadTarget
	{
		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
			AnimatorSaveData animatorSaveData = SaveAnimatorState();
			data.SaveValue(nameof(animatorSaveData), JsonUtility.ToJson(animatorSaveData));
		}

		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			AnimatorSaveData animatorSaveData = null;
			if(data.LoadValue(nameof(animatorSaveData), out string loadJson))
			{
				animatorSaveData = JsonUtility.FromJson<AnimatorSaveData>(loadJson);
				if(animatorSaveData != null)
				{
					LoadAnimatorState(animatorSaveData);
				}
			}
		}
	}
}