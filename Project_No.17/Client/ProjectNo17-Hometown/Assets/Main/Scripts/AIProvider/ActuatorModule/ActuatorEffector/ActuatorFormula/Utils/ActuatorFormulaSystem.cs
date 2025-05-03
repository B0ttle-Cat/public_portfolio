
using System;

using BC.FormulaItem;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.Actuator
{
	[Serializable, HideReferenceObjectPicker]
	public class ActuatorFormulaSystem
	{
		public ActuatorEffectType EffectType;
		[InlineProperty, HideLabel, PropertySpace(0, 20)]
		public FormulaGroup FormulaSystem;
		public bool TryCalculate(
			Func<ActuatorFormulaGetterType, AbilityType, float> abilityGetter,
			Func<ActuatorEffectType, float> effectValueGetter,
			out float result)
		{
			try
			{
				return FormulaEvaluator.TryEvaluate(FormulaSystem, out result, abilityGetter, effectValueGetter);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
				result = 0;
				return false;
			}
		}
		public float Calculate(
			Func<ActuatorFormulaGetterType, AbilityType, float> abilityGetter,
			Func<ActuatorEffectType, float> effectValueGetter)
		{
			TryCalculate(abilityGetter, effectValueGetter, out float result);
			return result;
		}
	}
}
