using System.Collections.Generic;
using System.Linq;

using BC.OdccBase;

using UnityEngine;

namespace BC.Actuator
{
	[CreateAssetMenu(fileName = "ActuatorFormulaGroup", menuName = "BC/ActuatorModule/Other/FormulaGroup")]
	public class ActuatorFormulaGroup : ActuatorFormula
	{
		[SerializeField]
		private ActuatorFormula[] items;
		internal override bool _HasFormulaItem(ActuatorEffectType effectType, HashSet<ActuatorFormula> duplicateCheck)
		{
			duplicateCheck ??= new HashSet<ActuatorFormula>();
			if(!duplicateCheck.Add(this)) return false;

			int length = items.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var formula = items[i];
				if(formula == null) continue;

				var find = formula._HasFormulaItem(effectType, duplicateCheck);
				if(find) return true;
			}

			return false;
		}
		internal override ActuatorFormulaSystem _GetFormulaItem(ActuatorEffectType effectType, HashSet<ActuatorFormula> duplicateCheck)
		{
			duplicateCheck ??= new HashSet<ActuatorFormula>();
			if(!duplicateCheck.Add(this)) return null;

			int length = items.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var formula = items[i];
				if(formula == null) continue;

				var find = formula._GetFormulaItem(effectType, duplicateCheck);
				if(find != null) return find;
			}

			return null;
		}
		internal override IEnumerable<ActuatorFormulaSystem> _GetAllFormulaItemEnumerable(HashSet<ActuatorFormula> duplicateCheck)
		{
			duplicateCheck ??= new HashSet<ActuatorFormula>();
			if(!duplicateCheck.Add(this)) return null;

			List<ActuatorFormulaSystem> actuatorFormulaSystems = new List<ActuatorFormulaSystem>();

			int length = items.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var formula = items[i];
				if(formula == null) continue;

				var find = formula._GetAllFormulaItemEnumerable(duplicateCheck);
				if(find == null || find.Count() == 0) continue;
				actuatorFormulaSystems.AddRange(find);
			}
			return actuatorFormulaSystems;
		}
	}
}
