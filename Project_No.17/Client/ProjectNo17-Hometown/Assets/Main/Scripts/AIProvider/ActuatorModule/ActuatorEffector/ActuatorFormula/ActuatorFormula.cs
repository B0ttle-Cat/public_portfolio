using System.Collections.Generic;
using System.Linq;

using BC.OdccBase;

using UnityEngine;

namespace BC.Actuator
{
	public abstract class ActuatorFormula : ScriptableObject
	{
		internal abstract bool _HasFormulaItem(ActuatorEffectType effectType, HashSet<ActuatorFormula> duplicateCheck);
		internal abstract ActuatorFormulaSystem _GetFormulaItem(ActuatorEffectType effectType, HashSet<ActuatorFormula> duplicateCheck);
		internal abstract IEnumerable<ActuatorFormulaSystem> _GetAllFormulaItemEnumerable(HashSet<ActuatorFormula> duplicateCheck);

		public bool HasFormulaItem(ActuatorEffectType effectType) => _HasFormulaItem(effectType, null);
		public ActuatorFormulaSystem GetFormulaItem(ActuatorEffectType effectType) => _GetFormulaItem(effectType, null);
		public IEnumerable<ActuatorFormulaSystem> GetAllFormulaItemEnumerable() => _GetAllFormulaItemEnumerable(null);
		public List<ActuatorFormulaSystem> GetAllFormulaItemList() => GetAllFormulaItemEnumerable().ToList();
		public ActuatorFormulaSystem[] GetAllFormulaItemArray() => GetAllFormulaItemEnumerable().ToArray();
	}
}
