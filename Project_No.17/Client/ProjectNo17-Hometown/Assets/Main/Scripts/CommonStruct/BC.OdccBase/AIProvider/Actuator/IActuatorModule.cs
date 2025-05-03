using BC.ODCC;

namespace BC.OdccBase
{
	public interface IActuatorModule
	{
		public bool HasCalculateEffectType(ActuatorEffectType actuatorEffectType);
		public bool CalculateBooleanEffect(IAbilityProvider order, IAbilityProvider target, ActuatorEffectType actuatorEffectType, bool defaultValue = false);
		public float CalculateFloatEffect(IAbilityProvider order, IAbilityProvider target, ActuatorEffectType actuatorEffectType, float defaultValue = 0f);
		public bool TryCalculateFloatEffect(IAbilityProvider order, IAbilityProvider target, ActuatorEffectType actuatorEffectType, out float result);
	}

	public interface IInstanceObject
	{

	}

	public interface IActuatorPlayer : IOdccComponent
	{

	}
}