using UnityEngine;

namespace BC.AISensor
{
	public class SensorManager : MonoBehaviour
	{
		public static SensorManager Sensor;

		public void Awake()
		{
			Sensor = this;
		}


	}
}
