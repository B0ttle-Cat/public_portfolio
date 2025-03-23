using System;

using BC.OdccBase;

using Sirenix.OdinInspector;

namespace BC.AIProvider
{
	public partial class ActuatorPriorityOrder
	{
		[Serializable]
		private class ActuatorPriorityFormulaIndex : FormulaEvaluator.FormulaItem_ExternalValue
		{
#if UNITY_EDITOR
			protected override string Type => "Priority";
			public override string ToString() => $"Priority({priorityOrderByIndex})";
			private ValueDropdownList<int> EditorOrderByList()
			{
				ValueDropdownList<int> list = new ValueDropdownList<int>();
				if(UnityEditor.Selection.activeObject is not ActuatorPriorityOrder thisPriority) return list;

				int length = thisPriority.priorityOrderByList.Length;
				for(int i = 0 ; i < length ; i++)
				{
					var orderBy = thisPriority.priorityOrderByList[i];
					if(orderBy != null)
					{
						list.Add($"{i}: {orderBy.GetType().Name}", i);
					}
					else
					{
						list.Add(null, i);
					}
				}

				return list;
			}
#endif
			[HorizontalGroup("Item"), LabelText("OrderByIndex"), LabelWidth(100)]
			[ValueDropdown("EditorOrderByList")]
			public int priorityOrderByIndex;

			public override float GetExternalValue(params Delegate[] externalGetters)
			{
				return _GetExternalValue(externalGetters) ? 1f : 0f;
				bool _GetExternalValue(params Delegate[] externalGetters)
				{
					foreach(var funcGetter in externalGetters)
					{
						if(funcGetter is Func<int, float> func)
						{
							if(func == null) return false;
							return func.Invoke(priorityOrderByIndex) > 0f;
						}
					}
					return false;
				}
			}
		}
	}
}
