using BC.ODCC;

using UnityEngine;
namespace BC.AIProvider
{
	public interface IROCSelectActuator : IOdccComponent
	{
		public void OnActuatorActive(ActuatorModule actuator, ObjectBehaviour targetObject);
		public void OnActuatorActive(ActuatorModule actuator, Vector3 targetPosition);
	}
}