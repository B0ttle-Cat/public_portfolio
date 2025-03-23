using System;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.AIProvider
{
	public partial class ActuatorPriorityOrder//.ActuatorPriorityModule
	{
		[Serializable]
		public abstract class ActuatorPriorityOrderBy
		{
			[Title("@GetType().Name"), PropertyOrder(-9999)]
			[ShowInInspector, HideLabel, DisplayAsString, EnableGUI]
			protected abstract string Description { get; }
			[SerializeField, PropertyOrder(-9998), HorizontalGroup("Title", 0.3f), LabelText("Is Reverse"), ToggleLeft]
			protected bool reverseOrder = false;
			[SerializeField, PropertyOrder(-9998), HorizontalGroup("Title"), LabelText("NormalizeScale"), MinValue(0f)]
			protected float normalizeScale = 1f;

			public void OnPriorityInit()
			{
			}
			internal void OnPriorityUpdate(ICombatCompute combatCompute, in UnitPairsTarget[] unitPairsTarget, out float[] result)
			{
				float min = float.MaxValue;
				float max = float.MinValue;

				int length = unitPairsTarget.Length;
				result = new float[length];
				if(length == 1)
				{
					result[0] = 1;
					return;
				}

				for(int i = 0 ; i < length ; i++)
				{
					var unitPair = unitPairsTarget[i];
					float point = PriorityUpdate(combatCompute, in unitPair);
					if(min > point) min = point;
					if(max < point) max = point;

					result[i] = point;
				}

				if(reverseOrder)
				{
					for(int i = 0 ; i < length ; i++)
					{
						float point = result[i];
						float normalize = (point-min)/(max-min);
						result[i] = (1f - normalize) * normalizeScale;
					}
				}
				else
				{
					for(int i = 0 ; i < length ; i++)
					{
						float point = result[i];
						float normalize = (point-min)/(max-min);
						result[i] = normalize * normalizeScale;
					}
				}
			}

			protected abstract float PriorityUpdate(ICombatCompute combatCompute, in UnitPairsTarget unitPair);
		}
	}
}
