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

		private bool IsHideIf()
		{
			return abilityType.ToString().StartsWith('_');
		}
	}
}
