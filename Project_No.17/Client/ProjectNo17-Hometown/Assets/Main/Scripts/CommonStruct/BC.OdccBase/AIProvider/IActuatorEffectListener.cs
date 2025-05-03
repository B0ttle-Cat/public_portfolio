using BC.ODCC;
namespace BC.OdccBase
{
	public interface IActuatorEffectListener : IOdccComponent
	{
		public void OnEffectValue(ActuatorEffectType actuatorEffectType, float value);
		public void OnEffectValue(IProjectileObject projectileObject, ActuatorEffectType actuatorEffectType, float value);
	}
}