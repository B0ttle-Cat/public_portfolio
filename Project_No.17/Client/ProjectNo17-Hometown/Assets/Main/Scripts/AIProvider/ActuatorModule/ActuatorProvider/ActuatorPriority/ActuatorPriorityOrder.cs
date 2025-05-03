using System;
using System.Collections.Generic;

using BC.AIProvider;
using BC.FormulaItem;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.Actuator
{
	public partial class ActuatorPriorityOrder : ScriptableObject
	{
		[InfoBox("결과 값이 클수록 우선순위가 높은 것으로 간주합니다")]
		[SerializeField, SerializeReference, HideReferenceObjectPicker]
		[ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
		internal ActuatorPriorityOrderBy[] priorityOrderByList = new ActuatorPriorityOrderBy[0];

		[InlineProperty, HideLabel, PropertySpace(20, 20)]
		public FormulaGroup FormulaSystem;

		public void PriorityInit()
		{
			int length = priorityOrderByList.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var modules = priorityOrderByList[i];
				if(modules == null) continue;
				modules.OnPriorityInit();
			}
		}
		public void PriorityUpdate(ICombatCompute combatCompute, in UnitPairsTarget[] unitPairsTarget, out Dictionary<int, float[]> priorityPoint)
		{
			int length = priorityOrderByList.Length;
			priorityPoint = new Dictionary<int, float[]>();
			for(int i = 0 ; i < length ; i++)
			{
				var modules = priorityOrderByList[i];
				if(modules == null)
				{
					priorityPoint.Add(i, new float[unitPairsTarget.Length]);
					continue;
				}
				modules.OnPriorityUpdate(combatCompute, in unitPairsTarget, out float[] result);
				priorityPoint.Add(i, result);
			}
		}
		public void PriorityPoint(ICombatCompute combatCompute, in UnitPairsTarget[] unitPairsTarget, Dictionary<int, float[]> priorityPoint, out float[] points, out int highestIndex)
		{
			highestIndex = -1;
			float highestPoint = float.MinValue;
			int length = unitPairsTarget.Length;
			points = new float[length];
			for(int i = 0 ; i < length ; i++)
			{
				var unitPairs = unitPairsTarget[i];

				int index = i;

				IAbilityProvider order = combatCompute.FindAbilityProvider(unitPairs.thisIndex);
				IAbilityProvider target = combatCompute.FindAbilityProvider(unitPairs.targetIndex);

				Func<ActuatorFormulaGetterType, AbilityType, float> _getAbility = GetAbility;
				Func<int, float> _priorityFormulaIndex = PriorityFormulaIndex;

				if(FormulaEvaluator.TryEvaluate(FormulaSystem, out float point, _priorityFormulaIndex, _getAbility))
				{
					if(highestPoint < point)
					{
						highestPoint = point;
						highestIndex = i;
					}
					points[i] = point;
				}
				else
				{
					points[i] = float.NegativeInfinity;
				}

				float PriorityFormulaIndex(int formulaIndex) => priorityPoint[formulaIndex][index];
				float GetAbility(ActuatorFormulaGetterType getter, AbilityType ability) => getter switch {
					ActuatorFormulaGetterType.Order => order.GetAbilityValue(ability),
					ActuatorFormulaGetterType.Target => target.GetAbilityValue(ability),
					_ => 0
				};
			}
		}
		internal void ComputePriorityPoint(ICombatCompute combatCompute, in UnitPairsTarget[] unitPairsTarget, out float[] result, out int highestIndex)
		{
			PriorityInit();
			PriorityUpdate(combatCompute, in unitPairsTarget, out var priorityPoint);
			PriorityPoint(combatCompute, in unitPairsTarget, priorityPoint, out result, out highestIndex);
		}


	}
}
