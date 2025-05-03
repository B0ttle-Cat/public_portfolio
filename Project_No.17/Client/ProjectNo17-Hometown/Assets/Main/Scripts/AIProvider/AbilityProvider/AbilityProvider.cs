using System.Collections.Generic;

using BC.Actuator;
using BC.ODCC;
using BC.OdccBase;
using BC.Shape;

using Shapes;

using Sirenix.OdinInspector;

using UnityEngine;
namespace BC.AIProvider
{
	public class AbilityProvider : ComponentBehaviour, IAbilityProvider, IShapeRenderer_EditorScriptableObject
	{
#if UNITY_EDITOR
		[ShowInInspector]
		private bool showVisibleRange_InEditor { get; set; } = true;
#endif

		[SerializeField, InlineProperty]
		private AbilityStruct abilityStruct;
		public AbilityStruct AbilityStruct { get => abilityStruct; private set => abilityStruct = value; }

		protected override void BaseStart()
		{
			abilityStruct ??= new AbilityStruct();
		}
		[Button, PropertyOrder(-1)]
		public void AddMissingType()
		{
			abilityStruct ??= new AbilityStruct();
			abilityStruct.AddMissingType();
		}
		public void SetAbilityStruct(AbilityStruct abilityStruct)
		{
			AbilityStruct = abilityStruct;
		}

		public List<AbilityValue> GetAbilityValueList() => AbilityStruct.GetAbilityValueList;
		public bool HasAbility(AbilityType abilityType) => AbilityStruct.HasAbility(abilityType);
		public bool HasNotAbility(AbilityType abilityType) => AbilityStruct.HasNotAbility(abilityType);
		public float GetAbilityValue(AbilityType abilityType, float defaultValue = 0f) => AbilityStruct.TryGetAbility(abilityType, out float value) ? value : defaultValue;
		public bool TryGetAbilityValue(AbilityType abilityType, out float value) => AbilityStruct.TryGetAbility(abilityType, out value);
		public bool TrySetAbilityValue(AbilityType abilityType, in float value) => AbilityStruct.TrySetAbility(abilityType, in value);
		public void RemoveAbility(AbilityType abilityType) => AbilityStruct.RemoveAbility(abilityType);

#if UNITY_EDITOR
		void IShapeRenderer_EditorScriptableObject.Editor_OnDrawShapes()
		{
			if(showVisibleRange_InEditor)
			{
				if(ThisContainer != null && ThisContainer.TryGetComponent<ReactionTarget>(out var reactionTarget))
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