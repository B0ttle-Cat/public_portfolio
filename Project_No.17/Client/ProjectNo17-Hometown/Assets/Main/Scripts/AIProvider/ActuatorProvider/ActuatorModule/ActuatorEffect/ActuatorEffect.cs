using System.Collections.Generic;

using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.AIProvider
{
	public interface IActuatorEffectListener : IOdccComponent
	{
		public void OnEffectValue(ActuatorEffectType actuatorEffectType, float value);
	}


	public partial class ActuatorEffect : ScriptableObject
	{
		[TitleGroup("ActuatorEffect"), SerializeField, SerializeReference]
		private ActuatorEffectInfo[] EffectInfoList;
		public enum GetterType
		{
			Order,
			Target,
		}
		public bool OnActuatorEffect(AbilityProvider order, AbilityProvider target, ActuatorEffectType actuatorEffectType, out float effectResultValue)
		{
			effectResultValue = 0f;
			if(order == null || target == null) return false;

			var list = EffectInfoList;

			HashSet<ActuatorEffectType> isCalculatedType = new HashSet<ActuatorEffectType>();
			isCalculatedType.Add(actuatorEffectType);
			for(int i = 0 ; i < list.Length ; i++)
			{
				var effectInfo = list[i];
				if(effectInfo != null && effectInfo.EffectType == actuatorEffectType)
				{
					if(effectInfo.TryCalculate(GetAbility, GetActuatorEffectType, out effectResultValue))
					{
						return true;
					}
				}
			}
			return false;

			float GetAbility(GetterType getter, AbilityType ability)
			{
				float getterValue = getter switch {
					GetterType.Order => order.GetAbilityValue(ability),
					GetterType.Target => target.GetAbilityValue(ability),
					_ => 0f
				};
				float actuatorValue = 0f;
				return getterValue + actuatorValue;
			}

			float GetActuatorEffectType(ActuatorEffectType externalEffect)
			{
				if(isCalculatedType.Add(externalEffect))
				{
					for(int i = 0 ; i < list.Length ; i++)
					{
						var effectInfo = list[i];
						if(effectInfo != null && effectInfo.EffectType == externalEffect)
						{
							return effectInfo.Calculate(GetAbility, GetActuatorEffectType);
						}
					}
				}
				return 0;
			}
		}
	}
}
