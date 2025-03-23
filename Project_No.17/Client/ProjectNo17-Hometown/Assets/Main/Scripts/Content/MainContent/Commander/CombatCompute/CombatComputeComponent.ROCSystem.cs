//namespace TFContent
//{
//	public partial class CombatComputeComponent // ROCCombatSystem
//	{
//		public class ROCCombatSystem : CombatSystem
//		{
//			public override void SystemEnable()
//			{
//			}
//			public override void SystemDisable()
//			{
//			}
//			public override void SystemUpdate()
//			{
//				//var dataMapping = CombatCompute.CombatDataMapping;
//				//var rocMapping = CombatCompute.ROCMapping;
//				//foreach(var data in dataMapping)
//				//{
//				//	if(!rocMapping.TryGetValue(data.Key, out var rocProvider) || !rocProvider.isActiveAndEnabled) return;
//				//	if(!data.Value.TryGetData<CombatComputeData.IFFData>(out var iffData)) return;

//				//	if(rocProvider.RetargetUpdate(in iffData.allResultList))
//				//	{
//				//		rocProvider.TargetResearchUpdate(in rocMapping, in iffData.allResultList);
//				//	}
//				//}
//			}
//		}
//	}
//}