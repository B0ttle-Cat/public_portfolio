using System;

using BC.Actuator;
using BC.FormulaItem;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

using Random = UnityEngine.Random;

namespace BC.FormulaExternalGroup.ActuatorEffector
{
	public class ActuatorFormulaAbility : FormulaItem_ExternalValue
	{
#if UNITY_EDITOR
		protected override string Type => "Ability";
		public override string ToString() => $"{getter switch { ActuatorFormulaGetterType.Order => "O", ActuatorFormulaGetterType.Target => "T", _ => "?" }}_{ability}";
#endif
		[HorizontalGroup("Item", width: 100), HideLabel]
		public ActuatorFormulaGetterType getter;
		[HorizontalGroup("Item"), HideLabel]
		public AbilityType ability;
		private enum ExpandOption
		{
			None,
			Scale,

			RangeMinMax,
			CurveMinMax,

			RangeOffset,
			CurveOffset,
		}
		[HorizontalGroup("Item" ,width: 70), HideLabel, SerializeField]
		private ExpandOption option;
		[HorizontalGroup("Item2"), ShowIf("@ShowIfScale"),HideLabel,SuffixLabel("Scale", overlay: true)]
		public float scale = 1;
		[HorizontalGroup("Item2"), ShowIf("@ShowIfRange"),HideLabel,SuffixLabel("MinScale", overlay: true)]
		public float min = 0;
		[HorizontalGroup("Item2"), ShowIf("@ShowIfRange"),HideLabel,SuffixLabel("MaxScale", overlay: true)]
		public float max = 0;
		[HorizontalGroup("Item2"), ShowIf("@ShowIfOffset"),HideLabel,SuffixLabel("Offset", overlay: true)]
		public float offset = 0;
		[HorizontalGroup("Item2"), ShowIf("@ShowIfCurve"),HideLabel]
		public AnimationCurve curve;
		private bool ShowIfScale => option == ExpandOption.Scale || ShowIfOffset;
		private bool ShowIfRange => option == ExpandOption.RangeMinMax || option == ExpandOption.CurveMinMax;
		private bool ShowIfCurve => option == ExpandOption.CurveMinMax || option == ExpandOption.CurveOffset;
		private bool ShowIfOffset => option == ExpandOption.RangeOffset || option == ExpandOption.CurveOffset;
		public override float GetExternalValue(params Delegate[] externalGetters)
		{
			float abilityValue = GetAbility();
			return option switch {
				ExpandOption.None => abilityValue,
				ExpandOption.Scale => abilityValue * scale,
				ExpandOption.RangeMinMax => abilityValue * Random.Range(min, max),
				ExpandOption.CurveMinMax => abilityValue * Mathf.Lerp(min, max, curve.Evaluate(Random.value)),
				ExpandOption.RangeOffset => abilityValue * Random.Range(scale-offset, scale+offset),
				ExpandOption.CurveOffset => abilityValue * Mathf.Lerp(scale-offset, scale+offset, curve.Evaluate(Random.value)),
				_ => abilityValue,
			};

			float GetAbility()
			{
				foreach(var funcGetter in externalGetters)
				{
					if(funcGetter is Func<ActuatorFormulaGetterType, AbilityType, float> func)
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
