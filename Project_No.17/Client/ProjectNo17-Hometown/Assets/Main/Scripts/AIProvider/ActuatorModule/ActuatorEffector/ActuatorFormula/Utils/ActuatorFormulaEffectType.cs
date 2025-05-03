using System;

using BC.FormulaItem;
using BC.OdccBase;

using Sirenix.OdinInspector;

namespace BC.FormulaExternalGroup.ActuatorEffector
{
	public class ActuatorFormulaEffectType : FormulaItem_ExternalValue
	{
#if UNITY_EDITOR
		protected override string Type => "EffectType";
		public override string ToString() => $"Actuator({actuatorEffectType})";
#endif
		[HorizontalGroup("Item"), HideLabel]
		public ActuatorEffectType actuatorEffectType;

		public override float GetExternalValue(params Delegate[] externalGetters)
		{
			foreach(var funcGetter in externalGetters)
			{
				if(funcGetter is Func<ActuatorEffectType, float> func)
				{
					if(func == null) return 0;
					return func.Invoke(actuatorEffectType);
				}
			}
			return 0;
		}
	}
}
