using System;

using Sirenix.OdinInspector;

namespace BC.OdccBase
{
	[Serializable, InlineProperty]
	public struct AbilityValue
	{
		[HorizontalGroup, HideLabel]
		public AbilityType abilityType;
		[HideIf("IsHideIf")]
		[HorizontalGroup(width: 0.3f), HideLabel]
		public float abilityValue;

		public AbilityType type { get => abilityType; set => abilityType = value; }
		public float value { get => abilityValue; set => abilityValue = value; }

		public AbilityValue(AbilityType abilityType, float abilityValue = 0) : this()
		{
			this.abilityType=abilityType;
			this.abilityValue=abilityValue;
		}

		private bool IsHideIf()
		{
			return abilityType.ToString().StartsWith('_');
		}
	}
}
