using System;

using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

using static BC.AIProvider.ActuatorEffect;
using static BC.OdccBase.FormulaEvaluator;

using Random = UnityEngine.Random;

namespace BC.AIProvider
{
	public class ActuatorFormulaAbility : FormulaItem_ExternalValue
	{
#if UNITY_EDITOR
		protected override string Type => "Ability";
		public override string ToString() => $"{getter switch { ActuatorEffect.GetterType.Order => "O", ActuatorEffect.GetterType.Target => "T", _ => "?" }}_{ability}";
#endif
		[HorizontalGroup("Item", width: 100), HideLabel]
		public GetterType getter;
		[HorizontalGroup("Item"), HideLabel]
		public AbilityType ability;
		private enum ExpandOption
		{
			None,
			Scale,
			Range,
			Curve,
		}

		[HorizontalGroup("Item" ,width: 70), HideLabel, SerializeField]
		private ExpandOption option;
		[HorizontalGroup("Item2"), ShowIf("@ShowIfScale")]
		public float scale = 1;
		[HorizontalGroup("Item2"), ShowIf("@ShowIfRange")]
		public float min = 0;
		[HorizontalGroup("Item2"), ShowIf("@ShowIfRange")]
		public float max = 0;
		[HorizontalGroup("Item2"), ShowIf("@ShowIfCurve")]
		public AnimationCurve curve;
		private bool ShowIfScale => option == ExpandOption.Scale;
		private bool ShowIfRange => option == ExpandOption.Range || option == ExpandOption.Curve;
		private bool ShowIfCurve => option == ExpandOption.Curve;
		public override float GetExternalValue(params Delegate[] externalGetters)
		{
			float abilityValue = GetAbility();
			return option switch {
				ExpandOption.None => abilityValue,
				ExpandOption.Scale => abilityValue  * scale,
				ExpandOption.Range => abilityValue * Random.Range(min, max),
				ExpandOption.Curve => abilityValue * Mathf.Lerp(min, max, curve.Evaluate(Random.value)),
				_ => abilityValue,
			};

			float GetAbility()
			{
				foreach(var funcGetter in externalGetters)
				{
					if(funcGetter is Func<ActuatorEffect.GetterType, AbilityType, float> func)
					{
						if(func == null) return 0;
						return func.Invoke(getter, ability);
					}
				}
				return 0;
			}
		}
	}
}
