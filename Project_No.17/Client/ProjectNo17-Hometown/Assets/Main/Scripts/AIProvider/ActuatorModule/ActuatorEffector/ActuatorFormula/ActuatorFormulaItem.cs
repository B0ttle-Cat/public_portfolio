using System.Collections.Generic;
using System.Linq;

using BC.OdccBase;

using UnityEngine;

namespace BC.Actuator
{
	[CreateAssetMenu(fileName = "ActuatorFormulaItem", menuName = "BC/ActuatorModule/Other/FormulaItem")]
	public class ActuatorFormulaItem : ActuatorFormula
	{
		[SerializeField]
		private ActuatorFormula parent;

		[SerializeField]
		private ActuatorFormulaSystem[] actuatorFormulaItemList = new ActuatorFormulaSystem[0];

		internal override bool _HasFormulaItem(ActuatorEffectType effectType, HashSet<ActuatorFormula> duplicateCheck)
		{
			duplicateCheck ??= new HashSet<ActuatorFormula>();
			if(!duplicateCheck.Add(this)) return false;

			int length = actuatorFormulaItemList == null ? 0 : actuatorFormulaItemList.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var formula = actuatorFormulaItemList[i];
				if(formula != null && formula.EffectType == effectType)
				{
					return true;
				}
			}
			if(parent == null) return false;
			return parent._HasFormulaItem(effectType, duplicateCheck);
		}
		internal override ActuatorFormulaSystem _GetFormulaItem(ActuatorEffectType effectType, HashSet<ActuatorFormula> duplicateCheck)
		{
			duplicateCheck ??= new HashSet<ActuatorFormula>();
			if(!duplicateCheck.Add(this)) return null;

			int length = actuatorFormulaItemList == null ? 0 : actuatorFormulaItemList.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var formula = actuatorFormulaItemList[i];
				if(formula != null && formula.EffectType == effectType)
				{
					return formula;
				}
			}
			if(parent == null) return null;
			return parent._GetFormulaItem(effectType, duplicateCheck);
		}
		internal override IEnumerable<ActuatorFormulaSystem> _GetAllFormulaItemEnumerable(HashSet<ActuatorFormula> duplicateCheck)
		{
			duplicateCheck ??= new HashSet<ActuatorFormula>();
			if(!duplicateCheck.Add(this)) return null;

			List<ActuatorFormulaSystem> actuatorFormulaSystems = new List<ActuatorFormulaSystem>(actuatorFormulaItemList ?? new ActuatorFormulaSystem[0]);
			if(parent != null)
			{
				actuatorFormulaSystems.AddRange(parent._GetAllFormulaItemEnumerable(duplicateCheck));
			}
			return actuatorFormulaSystems
				.Where(f => f != null)
				.GroupBy(x => x.EffectType)
				.Select(g => g.First());
		}

		public bool TryCalculate(IAbilityProvider order, IAbilityProvider target, ActuatorEffectType actuatorEffectType, out bool result)
		{
			result = false;
			var formulaItem = GetFormulaItem(actuatorEffectType);
			if(formulaItem == null) return false;

			Dictionary<ActuatorEffectType, float> calculatedResult = new Dictionary<ActuatorEffectType,float>(){
				{ actuatorEffectType, 0f }
			};

			if(formulaItem.TryCalculate(GetAbilityGetter, GetOtherEffectTypeValue, out float floatResult))
			{
				result = floatResult is > (-0.5f) and < 0.5f;
				return true;
			}
			return false;

			float GetAbilityGetter(ActuatorFormulaGetterType getter, AbilityType ability)
			{
				return getter switch {
					ActuatorFormulaGetterType.Order => order.GetAbilityValue(ability),
					ActuatorFormulaGetterType.Target => target.GetAbilityValue(ability),
					_ => 0
				};
			}
			float GetOtherEffectTypeValue(ActuatorEffectType effectType)
			{
				if(calculatedResult.TryGetValue(effectType, out float value))
				{
					return value;
				}
				else
				{
					calculatedResult.Add(effectType, 0f);
					var formulaItem = GetFormulaItem(actuatorEffectType);
					if(formulaItem != null && formulaItem.TryCalculate(GetAbilityGetter, GetOtherEffectTypeValue, out value))
					{
						calculatedResult[effectType] = value;
						return value;
					}
				}
				return 0f;
			}
		}

		public bool TryCalculate(IAbilityProvider order, IAbilityProvider target, ActuatorEffectType actuatorEffectType, out float result)
		{
			result = 0f;
			var formulaItem = GetFormulaItem(actuatorEffectType);
			if(formulaItem == null) return false;

			Dictionary<ActuatorEffectType, float> calculatedResult = new Dictionary<ActuatorEffectType,float>(){
				{ actuatorEffectType, 0f }
			};

			return formulaItem.TryCalculate(GetAbilityGetter, GetOtherEffectTypeValue, out result);

			float GetAbilityGetter(ActuatorFormulaGetterType getter, AbilityType ability)
			{
				return getter switch {
					ActuatorFormulaGetterType.Order => order.GetAbilityValue(ability),
					ActuatorFormulaGetterType.Target => target.GetAbilityValue(ability),
					_ => 0
				};
			}
			float GetOtherEffectTypeValue(ActuatorEffectType effectType)
			{
				if(calculatedResult.TryGetValue(effectType, out float value))
				{
					return value;
				}
				else
				{
					calculatedResult.Add(effectType, 0f);
					var formulaItem = GetFormulaItem(actuatorEffectType);
					if(formulaItem != null && formulaItem.TryCalculate(GetAbilityGetter, GetOtherEffectTypeValue, out value))
					{
						calculatedResult[effectType] = value;
						return value;
					}
				}
				return 0f;
			}
		}
	}
}
