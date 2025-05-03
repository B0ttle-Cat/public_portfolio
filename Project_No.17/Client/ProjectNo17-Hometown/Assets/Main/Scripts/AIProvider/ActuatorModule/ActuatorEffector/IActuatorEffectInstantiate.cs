using BC.ODCC;
using BC.OdccBase;

namespace BC.Actuator
{
	public interface IActuatorEffectInstantiate : IOdccObject
	{
		void OnInitActuatorEffect_CommonData(ObjectBehaviour orderObject, ReactionTargetInfo targetValue, ActuatorModule providerModule);
	}
	public interface IActuatorEffectInstantiate_Type : IActuatorEffectInstantiate
	{
		void OnInitActuatorEffect_TypeData(object[] typeDataList);
	}
	public interface IActuatorEffectInstantiate_VFX : IActuatorEffectInstantiate_Type
	{
	}
	public interface IActuatorEffectInstantiate_Projectile : IActuatorEffectInstantiate_Type
	{
	}
	public interface IActuatorEffectInstantiate_Summon : IActuatorEffectInstantiate_Type
	{
	}
}
