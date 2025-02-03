using System.Collections.Generic;

using BC.AISensor;

using Unity.Entities;

using UnityEngine;

namespace TFContent
{
	public partial class CombatManagement // .SensorSystem
	{
		public struct SensorTag : IComponentData
		{
			public SensorTag(SensorProvider to)
			{

			}
		}
		[UpdateInGroup(typeof(SimulationSystemGroup))]
		[UpdateBefore(typeof(CombatIFFUpdate))]
		public partial struct CombatSensorUpdate : ISystem
		{
			public void OnUpdate(ref SystemState state)
			{
				CombatManagement manager = CombatManagement.Manager;
				var sensorMapping = manager.SensorMapping;
				var iffMapping = manager.IFFMapping;
				foreach(var (sensorTag, iffTag, entity) in SystemAPI.Query<RefRO<SensorTag>, RefRO<IFFTag>>().WithEntityAccess())
				{
					List<Collider> sensorResult = null;
					if(sensorMapping.TryGetValue(entity, out var sensor))
					{
						sensor.SensorUpdate();
						sensorResult = sensor.sensorResult;
					}
				}
			}
		}
	}
}
