//namespace TFContent
//{
//	public partial class CombatComputeComponent // SensorCombatSystem
//	{
//		public class SensorCombatSystem : CombatSystem
//		{
//			public override void SystemEnable()
//			{
//			}
//			public override void SystemDisable()
//			{
//			}

//			public override void SystemUpdate()
//			{
//				var dataMapping = CombatCompute.CombatDataMapping;
//				var sensorMapping = CombatCompute.SensorMapping;

//				foreach(var data in dataMapping)
//				{
//					if(!sensorMapping.TryGetValue(data.Key, out var sensor)) continue;
//					if(!data.Value.TryGetData<CombatComputeData.SensorData>(out var sensorData)) continue;
//					sensor.SensorUpdate();
//					sensorData.sensorResult = sensor.sensorResult;
//				}
//			}
//		}
//	}
//}