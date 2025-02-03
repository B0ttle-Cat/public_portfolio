using BC.ODCC;

using Unity.Collections;

using UnityEngine;
namespace TFContent
{
	public class AnimatorController : ComponentBehaviour//, IOdccUpdate
	{
		[SerializeField, ReadOnly]
		private Animator animator;
		public Animator Animator {
			get {
				if(animator == null)
				{
					animator = GetComponentInChildren<Animator>(true);
					if(animator == null)
					{
						animator = gameObject.AddComponent<Animator>();
					}
				}
				return animator;
			}
			private set => animator = value;
		}

		protected override void BaseValidate(in bool isPrefab = false)
		{
			animator = GetComponentInChildren<Animator>(true);
		}

		#region Parameter
		public float GetFloat(string name) => Animator.GetFloat(name);
		public float GetFloat(int id) => Animator.GetFloat(id);
		public void SetFloat(string name, float value) => Animator.SetFloat(name, value);
		public void SetFloat(string name, float value, float dampTime, float deltaTime) => Animator.SetFloat(name, value, dampTime, deltaTime);
		public void SetFloat(int id, float value) => Animator.SetFloat(id, value);
		public void SetFloat(int id, float value, float dampTime, float deltaTime) => Animator.SetFloat(id, value, dampTime, deltaTime);
		public bool GetBool(string name) => Animator.GetBool(name);
		public bool GetBool(int id) => Animator.GetBool(id);
		public void SetBool(string name, bool value) => Animator.SetBool(name, value);
		public void SetBool(int id, bool value) => Animator.SetBool(id, value);
		public int GetInteger(string name) => Animator.GetInteger(name);
		public int GetInteger(int id) => Animator.GetInteger(id);
		public void SetInteger(string name, int value) => Animator.SetInteger(name, value);
		public void SetInteger(int id, int value) => Animator.SetInteger(id, value);
		public void SetTrigger(string name) => Animator.SetTrigger(name);
		public void SetTrigger(int id) => Animator.SetTrigger(id);
		public void ResetTrigger(string name) => Animator.ResetTrigger(name);
		public void ResetTrigger(int id) => Animator.ResetTrigger(id);
		#endregion
		#region Layer
		public string GetLayerName(int layerIndex) => Animator.GetLayerName(layerIndex);
		public int GetLayerIndex(string layerName) => string.IsNullOrWhiteSpace(layerName) ? -1 : Animator.GetLayerIndex(layerName);
		public float GetLayerWeight(int layerIndex) => Animator.GetLayerWeight(layerIndex);
		public float GetLayerWeight(string layerName) => Animator.GetLayerWeight(GetLayerIndex(layerName));
		public void SetLayerWeight(int layerIndex, float weight) => Animator.SetLayerWeight(layerIndex, weight);
		public void SetLayerWeight(string layerName, float weight) => Animator.SetLayerWeight(GetLayerIndex(layerName), weight);
		#endregion
		#region PlayImmediately
		public void PlayImmediately(string stateName)
		{
			Animator.Play(stateName);
		}
		public void PlayImmediately(string stateName, int layerIndex)
		{
			Animator.Play(stateName, layerIndex);
		}
		public void PlayImmediately(string stateName, string layerName)
		{
			Animator.Play(stateName, GetLayerIndex(layerName));
		}
		#endregion

		#region Custom API Group
		public void SetMove(bool IsMove, float MoveSpeed)
		{
			SetBool(nameof(IsMove), IsMove);
			SetFloat(nameof(MoveSpeed), MoveSpeed);
		}

		public bool CharacterCanMoveState()
		{
			if(Animator == null) return true;

			if(Animator.GetCurrentAnimatorStateInfo(0).IsName("Incapacitate"))
			{
				return false;
			}
			else if(Animator.GetCurrentAnimatorStateInfo(0).IsName("FireIdle"))
			{
				return false;
			}
			return true;
		}
		#endregion
	}
}