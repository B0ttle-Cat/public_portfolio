using System.Collections.Generic;
using System.Linq;

using BC.Actuator;
using BC.Base;
using BC.ODCC;
namespace BC.AIProvider
{
	/// <summary>
	/// Rules of Combat
	/// </summary>
	public class ROCProvider : ComponentBehaviour//, IOdccUpdate
	{
		[Sirenix.OdinInspector.ReadOnly]
		public ActuatorProvider actuatorProvider;
		[Sirenix.OdinInspector.ReadOnly]
		public IFFProvider iffProvider;

		protected override void BaseValidate(in bool isPrefab = false)
		{
			if(actuatorProvider == null) actuatorProvider = GetComponent<ActuatorProvider>();
			if(iffProvider == null) iffProvider = GetComponent<IFFProvider>();
		}

		protected override void BaseAwake()
		{
			if(actuatorProvider == null) actuatorProvider = ThisContainer.GetComponent<ActuatorProvider>();
			if(iffProvider == null) iffProvider = GetComponent<IFFProvider>();
		}

		public bool ComputeSelectActuatorProviderModule(UnitPairsTarget[] allPairsValue, bool computeOnlyAIControl, out ActuatorModule resultActuator, out UnitPairsTarget resultPair)
		{
			bool thisIsAI = !iffProvider.IsUserControl;
			resultActuator = null;
			resultPair = default;
			if(!EventManager.TryGet<ICombatCompute>(out var iCombatCompute)) return false;

			Dictionary<ActuatorModule, List<int>> moduleTargetKeyValue = new Dictionary<ActuatorModule, List<int>>();
			int length = allPairsValue.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var canUseActuatorList = allPairsValue[i].canUseActuatorList;
				int length2 = canUseActuatorList.Count;
				for(int ii = 0 ; ii < length2 ; ii++)
				{
					var canUseActuator = canUseActuatorList[ii];
					if(canUseActuator == null) continue;
					if(!computeOnlyAIControl || thisIsAI || canUseActuator.Basic.UserUnitAI_CanUseAuto)
					{
						if(!moduleTargetKeyValue.TryGetValue(canUseActuator, out var targetPairsValue))
						{
							targetPairsValue = new List<int>();
							moduleTargetKeyValue.Add(canUseActuator, targetPairsValue);
						}
						targetPairsValue.Add(i);
					}
				}
			}
			float highPoint = float.MinValue;
			ActuatorModule highPointActuator = null;
			UnitPairsTarget highPointPairs = default;
			foreach(var moduleTargetItem in moduleTargetKeyValue)
			{
				var module = moduleTargetItem.Key;
				if(module == null) continue;
				var targetPairsValue = moduleTargetItem.Value.Select(i=>allPairsValue[i]).ToArray();

				module.ComputePriorityPoint(iCombatCompute, in targetPairsValue, out float[] points, out int highestIndex);
				if(highestIndex >= 0)
				{
					float point = points[highestIndex];
					if(highPoint < point)
					{
						highPoint = point;
						highPointActuator = module;
						highPointPairs = targetPairsValue[highestIndex];
					}
				}
			}
			resultActuator = highPointActuator;
			resultPair = highPointPairs;
			return resultActuator != null;
		}
	}
}