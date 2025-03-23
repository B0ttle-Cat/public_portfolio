using BC.ODCC;
using BC.OdccBase;
using BC.Shape;

using Shapes;

using Sirenix.OdinInspector;

using UnityEngine;
namespace BC.AIProvider
{
	public class AbilityProvider : ComponentBehaviour, IAbilityStruct, IShapeRenderer_EditorScriptableObject
	{
#if UNITY_EDITOR
		[ShowInInspector]
		private bool showVisibleRange_InEditor { get; set; } = true;
#endif

		[ShowInInspector, InlineProperty]
		private AbilityStruct abilityStruct;
		public AbilityStruct AbilityStruct { get => abilityStruct; set => abilityStruct = value; }

		protected override void BaseStart()
		{
			abilityStruct ??= new AbilityStruct();
		}
		[Button, PropertyOrder(-1)]
		public void AddMissingType()
		{
			AbilityStruct.AddMissingType();
		}



		bool IAbilityStruct.HasAbility(AbilityType abilityType) => AbilityStruct.HasAbility(abilityType);
		bool IAbilityStruct.HasNotAbility(AbilityType abilityType) => AbilityStruct.HasNotAbility(abilityType);
		bool IAbilityStruct.TryGetAbility(AbilityType abilityType, out float value) => AbilityStruct.TryGetAbility(abilityType, out value);
		bool IAbilityStruct.TrySetAbility(AbilityType abilityType, in float value) => AbilityStruct.TrySetAbility(abilityType, in value);
		void IAbilityStruct.RemoveAbility(AbilityType abilityType) => AbilityStruct.RemoveAbility(abilityType);
		public AbilityStruct GetAbilityStruct() => AbilityStruct;

		public bool HasPlayAbility(AbilityType abilityType)
		{
			var allAbilityStruct = GetComponents<IAbilityStruct>();
			int length = allAbilityStruct.Length;
			for(int i = 0 ; i < length ; i++)
			{
				if(allAbilityStruct[i].HasAbility(abilityType))
				{
					return true;
				}
			}
			return false;
		}
		public bool HasNotPlayAbility(AbilityType abilityType)
		{
			return !HasPlayAbility(abilityType);
		}
		public float GetAbilityValue(AbilityType abilityType)
		{
			float value = 0f;
			var allAbilityStruct = GetComponents<IAbilityStruct>();
			int length = allAbilityStruct.Length;
			for(int i = 0 ; i < length ; i++)
			{
				if(allAbilityStruct[i].TryGetAbility(abilityType, out var _value))
				{
					value += _value;
				}
			}
			return value;
		}

#if UNITY_EDITOR
		void IShapeRenderer_EditorScriptableObject.Editor_OnDrawShapes()
		{
			if(showVisibleRange_InEditor)
			{
				if(ThisContainer.TryGetComponent<ReactionTarget>(out var reactionTarget))
				{
					Draw.UseDashes = false;
					Draw.Ring(reactionTarget.Position + Vector3.up * 0.01f, Vector3.up, GetVisibleRange() + reactionTarget.Radius, 0.05f, Color.black);
					Draw.Ring(reactionTarget.Position + Vector3.up * 0.01f, Vector3.up, GetReactionRange(false) + reactionTarget.Radius, 0.05f, Color.gray);
					Draw.UseDashes = true;
					Draw.Ring(reactionTarget.Position + Vector3.up * 0.01f, Vector3.up, GetReactionRange(true) + reactionTarget.Radius, 0.05f, Color.gray);
				}
			}
		}
#endif
		public float GetVisibleRange()
		{
			return GetAbilityValue(AbilityType.VisibleRange);
		}
		public float GetReactionRange(bool usingMaxRange)
		{
			if(ThisContainer.TryGetComponent<ActuatorProvider>(out var actuatorProvider))
			{
				float baseValue = GetAbilityValue(AbilityType.ReactionRange);
				float actuatorAbilityValue = actuatorProvider.FurthestReactionRange(true);
				return baseValue + actuatorAbilityValue;
			}
			else
			{
				return GetAbilityValue(AbilityType.ReactionRange);
			}
		}
	}
}