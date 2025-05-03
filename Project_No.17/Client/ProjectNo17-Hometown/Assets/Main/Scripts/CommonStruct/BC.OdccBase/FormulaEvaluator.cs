using System;
using System.Collections.Generic;

using BC.FormulaItem.Branch;
using BC.FormulaItem.BreakReturn;
using BC.FormulaItem.ClampOrNormalize;
using BC.FormulaItem.Random;
using BC.FormulaItem.Value;

using Sirenix.OdinInspector;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace BC.FormulaItem
{
	public static class FormulaEvaluator
	{
		public static bool TryBoolEvaluate(FormulaGroup group, Func<float, bool> floatCondition, out bool resultBool, params Delegate[] externalGetters)
		{
			resultBool = TryEvaluate(group, out float resultFloat, externalGetters);
			if(!resultBool) return false;
			resultBool = floatCondition?.Invoke(resultFloat) ?? resultFloat != 0f;
			return true;
		}


		public static bool TryEvaluate(FormulaGroup group, out float result, params Delegate[] externalGetters)
		{
			bool isReturn = false;
			return TryEvaluateFormulaGroup(ref isReturn, group, out result, externalGetters);
		}

		/// <summary>
		/// 피연산자(FormulaValue)와 FormulaGroup, FormulaItem_Branch를 평가합니다.
		/// </summary>
		private static bool TryEvaluateFormulaValue(ref bool isReturn, FormulaItem token, in float current, out float result, params Delegate[] externalGetters)
		{
			result = 0;
			if(isReturn) return true;
			if(token == null) return false;

			if(token is FormulaGroup subGroup)
			{
				return TryEvaluateFormulaGroup(ref isReturn, subGroup, out result, externalGetters);
			}
			else if(token is FormulaItem_Value value)
			{
				if(value is FormulaValue fValue)
				{
					result = fValue.value;
					return true;
				}
				else if(value is FormulaValue_Zero)
				{
					result = 0f;
					return true;
				}
				else if(value is FormulaValue_One)
				{
					result = 1f;
					return true;
				}
				else if(value is FormulaSelf)
				{
					result = current;
					return true;
				}
				else if(value is FormulaValue_Bool fBool)
				{
					result = fBool.value ? 1f : 0f;
					return true;
				}
				else if(value is FormulaSelf_Scale fScale)
				{
					result = fScale.Scale(in result);
					return true;
				}
				else if(value is FormulaRandom fRandom)
				{
					if(!TryEvaluateFormulaValue(ref isReturn, fRandom.min, in current, out float minVal, externalGetters)) return false;
					if(!TryEvaluateFormulaValue(ref isReturn, fRandom.max, in current, out float maxVal, externalGetters)) return false;

					result = fRandom.curve == null
						? UnityEngine.Random.Range(minVal, maxVal)
						: Mathf.Lerp(minVal, maxVal, fRandom.curve.Evaluate(UnityEngine.Random.value));

					return true;
				}
				else if(value is FormulaRandom01 fRandom01)
				{
					result = fRandom01.curve == null ? UnityEngine.Random.value : fRandom01.curve.Evaluate(UnityEngine.Random.value);
					return true;
				}
				else if(value is FormulaItem_ExternalValue fExternal)
				{
					try
					{
						result = fExternal.GetExternalValue(externalGetters);
						return true;
					}
					catch(Exception ex)
					{
						result = 0f;
						Debug.LogException(ex);
						return false;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// FormulaGroup 전체를 평가합니다.
		/// 구조가 올바르지 않으면 false를 반환합니다.
		/// </summary>

		private static bool TryEvaluateFormulaGroup(ref bool isReturn, FormulaGroup group, out float result, params Delegate[] externalGetters)
		{
			result = 0;
			if(isReturn) return true;
			if(group == null || group.formulas == null || group.formulas.Count == 0) return false;

			List<FormulaItem> formulas = new List<FormulaItem>(group.formulas);

			int i = 0;

			while(!isReturn && i < formulas.Count)
			{
				FormulaItem token = formulas[i];

				if(TryEvaluateFormulaValue(ref isReturn, token, in result, out float formulaValue, externalGetters))
				{
					result = formulaValue;
					i++;
				}
				else if(token is FormulaOperator op)
				{
					i++;
					if(i >= formulas.Count) return false;

					// 연산자 뒤에는 피연산자 또는 분기처리가 와야 함.
					FormulaItem nextToken = formulas[i];
					if(nextToken is FormulaItem_Branch branch)
					{
						if(!TryEvaluateFormulaBranch(ref isReturn, branch, in result, out var branchItem, externalGetters)) return false;
						if(branchItem != null) formulas.Insert(i, branchItem);
					}
					else if(TryEvaluateFormulaValue(ref isReturn, nextToken, in result, out float operandValue, externalGetters))
					{
						result = op.Operator switch {
							FormulaOperator.OperatorType.Assignment => operandValue,
							FormulaOperator.OperatorType.Addition => result + operandValue,
							FormulaOperator.OperatorType.Subtraction => result - operandValue,
							FormulaOperator.OperatorType.Multiplication => result * operandValue,
							FormulaOperator.OperatorType.Division => operandValue == 0 ? result : result / operandValue,
							FormulaOperator.OperatorType.Modulo => operandValue == 0 ? result : result % operandValue,
							FormulaOperator.OperatorType.OR => OR(result, operandValue),
							FormulaOperator.OperatorType.And => And(result, operandValue),
							//FormulaOperator.OperatorType.Not => Not(result),
							_ => result
						};
						float NormalizeBool(float _value)
						{
							return _value < 0f ? 0f : 1f;
						}
						float And(float _value1, float _value2)
						{
							_value1 = NormalizeBool(_value1);
							_value2 = NormalizeBool(_value2);
							return _value1 * _value2;
						}
						float OR(float _value1, float _value2)
						{
							_value1 = NormalizeBool(_value1);
							_value2 = NormalizeBool(_value2);
							return _value1 + _value2 == 0f ? 0f : 1f;
						}
					}
					else
					{
						return false;
					}
					i++;
				}
				else if(token is FormulaItem_Function function)
				{
					i++;
					if(!TryEvaluateFormulaFunction(ref isReturn, function, in result, out float functionResult, externalGetters)) return false;
					result = functionResult;
				}
				else if(token is FormulaItem_Branch branch)
				{
					i++;
					if(!TryEvaluateFormulaBranch(ref isReturn, branch, in result, out var branchItem, externalGetters)) return false;
					if(branchItem != null) formulas.Insert(i, branchItem);
				}
				else if(token is Break)
				{
					if(token is BreakValue breakValue)
					{
						if(TryEvaluateFormulaValue(ref isReturn, token, in result, out float changeValue, externalGetters)) return false;
						result = changeValue;
					}
					break;
				}
				else if(token is Return)
				{
					if(token is ReturnValue breakValue)
					{
						if(TryEvaluateFormulaValue(ref isReturn, token, in result, out float changeValue, externalGetters)) return false;
						result = changeValue;
					}
					isReturn = true;
					break;
				}
				else
				{
					return false;
				}
			}

			return true;
		}
		private static bool TryEvaluateFormulaBranch(ref bool isReturn, FormulaItem_Branch branch, in float current, out FormulaItem branchItem, params Delegate[] externalGetters)
		{
			branchItem = null;
			if(branch is FormulaConditionBool conditionBool)
			{
				if(!TryEvaluateFormulaValue(ref isReturn, conditionBool.Bool, in current, out float conditionValue, externalGetters)) return false;
				branchItem = conditionBool.ConditionResult(conditionValue);
				return true;
			}
			else if(branch is FormulaConditionSelf conditionSelf)
			{
				if(!TryEvaluateFormulaValue(ref isReturn, conditionSelf.C, in current, out float conditionValue, externalGetters)) return false;
				branchItem = conditionSelf.ConditionResult(current, conditionValue);
				return true;
			}
			else if(branch is FormulaConditionAB conditionAB)
			{
				if(!TryEvaluateFormulaValue(ref isReturn, conditionAB.A, in current, out float comparisonValueA, externalGetters)) return false;
				if(!TryEvaluateFormulaValue(ref isReturn, conditionAB.B, in current, out float comparisonValueB, externalGetters)) return false;
				branchItem = conditionAB.ComparisonResult(comparisonValueA, comparisonValueB);
				return true;
			}
			return false;
		}
		private static bool TryEvaluateFormulaFunction(ref bool isReturn, FormulaItem_Function function, in float current, out float result, params Delegate[] externalGetters)
		{
			result = current;
			if(function is FormulaClamp clamp)
			{
				// Clamp 내부의 min, max 평가
				if(!TryEvaluateFormulaValue(ref isReturn, clamp.min, in current, out float minVal, externalGetters)) return false;
				if(!TryEvaluateFormulaValue(ref isReturn, clamp.max, in current, out float maxVal, externalGetters)) return false;
				result = Mathf.Clamp(result, minVal, maxVal);
				return true;
			}
			else if(function is FormulaClampMin min)
			{
				// Clamp 내부의 min, max 평가
				if(!TryEvaluateFormulaValue(ref isReturn, min.min, in current, out float minVal, externalGetters)) return false;
				result = Mathf.Clamp(result, minVal, float.PositiveInfinity);
				return true;
			}
			else if(function is FormulaClampMax max)
			{
				// Clamp 내부의 min, max 평가	
				if(!TryEvaluateFormulaValue(ref isReturn, max.max, in current, out float maxVal, externalGetters)) return false;
				result = Mathf.Clamp(result, float.NegativeInfinity, maxVal);
				return true;
			}
			else if(function is FormulaClamp01 clamp01)
			{
				result = Mathf.Clamp(result, 0f, 1f);
				return true;
			}
			else if(function is FormulaNormalize normalize)
			{
				if(!TryEvaluateFormulaValue(ref isReturn, normalize.min, in current, out float minVal, externalGetters)) return false;
				if(!TryEvaluateFormulaValue(ref isReturn, normalize.max, in current, out float maxVal, externalGetters)) return false;
				float deltaMax = maxVal - minVal;
				float deltaCur = current - minVal;
				result = Mathf.Abs(deltaMax) < float.Epsilon ? 1f : deltaCur/deltaMax;
				return true;
			}
			else if(function is FormulaNormalizeClamp normalize01)
			{
				if(!TryEvaluateFormulaValue(ref isReturn, normalize01.min, in current, out float minVal, externalGetters)) return false;
				if(!TryEvaluateFormulaValue(ref isReturn, normalize01.max, in current, out float maxVal, externalGetters)) return false;
				float deltaMax = maxVal - minVal;
				float deltaCur = current - minVal;
				result = Mathf.Clamp01(Mathf.Abs(deltaMax) < float.Epsilon ? 1f : deltaCur/deltaMax);

				return true;
			}
			return false;
		}
	}

	[Serializable, HideReferenceObjectPicker, InlineProperty]
	public abstract class FormulaItem
	{
#if UNITY_EDITOR
		[ShowInInspector, HideLabel, HorizontalGroup("Item", width: 58), PropertyOrder(-999)]
		[HideIf("IsHide")]
		protected virtual string Type => GetType().Name;
		private bool IsHide => Type == nameof(FormulaGroup) || string.IsNullOrWhiteSpace(Type);
		public override string ToString() => "";
#endif
	}
	[Serializable, HideReferenceObjectPicker, InlineProperty]
	public class FormulaGroup : FormulaItem
	{
#if UNITY_EDITOR
		public override string ToString() => $"({string.Join(" ", formulas)})";
#endif
		[SerializeReference, HorizontalGroup("Item"), LabelText("FormulaGroup")]
		public List<FormulaItem> formulas = new List<FormulaItem>();
	}
	public class FormulaOperator : FormulaItem
	{
#if UNITY_EDITOR
		protected override string Type => "Operator";
		public override string ToString() => $"{Operator switch { OperatorType.Assignment => "＝", OperatorType.Addition => "＋", OperatorType.Subtraction => "－", OperatorType.Multiplication => "×", OperatorType.Division => "÷", _ => "?", }}";
#endif
		public enum OperatorType
		{
			[InspectorName(" ＝	값변경")]  Assignment,
			[InspectorName(" ＋	더히기")]  Addition,
			[InspectorName(" －	빼기")]    Subtraction,
			[InspectorName(" ×	곱하기")]  Multiplication,
			[InspectorName(" ÷	나누기")]  Division,
			[InspectorName(" %	나머지")]  Modulo,

			[InspectorName(" ||	OR")]  OR,
			[InspectorName(" &&	And")]  And,
		}
		[HideLabel,HorizontalGroup("Item")]
		public OperatorType Operator;
	}

	[Serializable, HideReferenceObjectPicker, InlineProperty]
	public abstract class FormulaItem_Function : FormulaItem
	{
		//	[ShowInInspector, HideLabel, HorizontalGroup("Item", width: 12), PropertyOrder(-1000)]
		//	public bool enable = true;
	};
	[Serializable, HideReferenceObjectPicker, InlineProperty]
	public abstract class FormulaItem_Value : FormulaItem { };
	public abstract class FormulaItem_ExternalValue : FormulaItem_Value
	{
		public abstract float GetExternalValue(params Delegate[] externalGetters);
	}
	[Serializable, HideReferenceObjectPicker, InlineProperty]
	public abstract class FormulaItem_Branch : FormulaItem
	{

	}

}
namespace BC.FormulaItem.BreakReturn
{
	public class Break : FormulaItem
	{
#if UNITY_EDITOR
		protected override string Type => "Break";
		public override string ToString() => "Break";
#endif
	}
	public class Return : FormulaItem
	{
#if UNITY_EDITOR
		protected override string Type => "Return";
		public override string ToString() => "Return";
#endif
	}
	public class BreakValue : Return
	{
		[HorizontalGroup("Item"), LabelWidth(50), SerializeReference]
		public FormulaItem_Value value;
	}
	public class ReturnValue : Return
	{
		[HorizontalGroup("Item"), LabelWidth(50), SerializeReference]
		public FormulaItem_Value value;
	}
}
namespace BC.FormulaItem.ClampOrNormalize
{
	public class FormulaClamp : FormulaItem_Function
	{
#if UNITY_EDITOR
		protected override string Type => "Clamp";
		public override string ToString() => $"⇒ Clamp[{min},{max}]";
#endif
		[HorizontalGroup("Item"), VerticalGroup("Item/MinMax"), LabelText("Min"), LabelWidth(25), SerializeReference]
		public FormulaItem min;
		[HorizontalGroup("Item"), VerticalGroup("Item/MinMax"), LabelText("Max"), LabelWidth(25), SerializeReference]
		public FormulaItem max;
	}
	public class FormulaClampMin : FormulaItem_Function
	{
#if UNITY_EDITOR
		protected override string Type => "Clamp";
		public override string ToString() => $"⇒ ClampMin[{min}]";
#endif
		[HorizontalGroup("Item"), VerticalGroup("Item/Min"), LabelText("Min"), LabelWidth(25), SerializeReference]
		public FormulaItem min;
	}
	public class FormulaClampMax : FormulaItem_Function
	{
#if UNITY_EDITOR
		protected override string Type => "Clamp";
		public override string ToString() => $"⇒ ClampMax[{max}]";
#endif
		[HorizontalGroup("Item"), VerticalGroup("Item/Max"), LabelText("Max"), LabelWidth(25), SerializeReference]
		public FormulaItem max;
	}
	public class FormulaClamp01 : FormulaItem_Function
	{
#if UNITY_EDITOR
		protected override string Type => "Clamp01";
		public override string ToString() => $"⇒ Clamp[0,1]";
#endif
	}
	public class FormulaNormalize : FormulaItem_Function
	{
#if UNITY_EDITOR
		protected override string Type => "Normal";
		public override string ToString() => $"⇒ Normalize[{min},{max}]";
#endif
		[HorizontalGroup("Item"), VerticalGroup("Item/MinMax"), LabelText("Min"), LabelWidth(25), SerializeReference]
		public FormulaItem min;
		[HorizontalGroup("Item"), VerticalGroup("Item/MinMax"), LabelText("Max"), LabelWidth(25), SerializeReference]
		public FormulaItem max;
	}
	public class FormulaNormalizeClamp : FormulaItem_Function
	{
#if UNITY_EDITOR
		protected override string Type => "Normal01";
		public override string ToString() => $"⇒ Normalize[{min},{max}]";
#endif
		[HorizontalGroup("Item"), VerticalGroup("Item/MinMax"), LabelText("Min"), LabelWidth(25), SerializeReference]
		public FormulaItem min;
		[HorizontalGroup("Item"), VerticalGroup("Item/MinMax"), LabelText("Max"), LabelWidth(25), SerializeReference]
		public FormulaItem max;
	}
}
namespace BC.FormulaItem.Value
{
	#region FormulaItem_Value
	public class FormulaValue_Zero : FormulaItem_Value
	{
#if UNITY_EDITOR
		protected override string Type => "Zero";
		public override string ToString() => $"{0}";
#endif
	}
	public class FormulaValue_One : FormulaItem_Value
	{
#if UNITY_EDITOR
		protected override string Type => "One";
		public override string ToString() => $"{1}";
#endif
	}
	public class FormulaValue : FormulaItem_Value
	{
#if UNITY_EDITOR
		protected override string Type => "Value";
		public override string ToString() => $"{value}";
#endif
		[HideLabel, HorizontalGroup("Item")]
		public float value;
	}
	public class FormulaSelf : FormulaItem_Value
	{
#if UNITY_EDITOR
		protected override string Type => "Self";
		public override string ToString() => $"Self";
#endif
	}
	public class FormulaValue_Bool : FormulaItem_Value
	{
#if UNITY_EDITOR
		protected override string Type => "";
		public override string ToString() => $"{value}";
#endif
		[HideLabel, HorizontalGroup("Item")]
		public bool value;
	}

	public class FormulaSelf_Scale : FormulaItem_Value
	{
#if UNITY_EDITOR
		protected override string Type => "Scale";
		public override string ToString() => $"Scale_{option}";
#endif
		private enum ExpandOption
		{
			None,
			Scale,

			RangeMinMax,
			CurveMinMax,

			RangeOffset,
			CurveOffset,
		}
		[HorizontalGroup("Item" ,width: 70), HideLabel, SerializeField]
		private ExpandOption option;
		[HorizontalGroup("Item"), ShowIf("@ShowIfScale"),HideLabel,SuffixLabel("Scale", overlay: true)]
		public float scale = 1;
		[HorizontalGroup("Item"), ShowIf("@ShowIfRange"),HideLabel,SuffixLabel("MinScale", overlay: true)]
		public float min = 0;
		[HorizontalGroup("Item"), ShowIf("@ShowIfRange"),HideLabel,SuffixLabel("MaxScale", overlay: true)]
		public float max = 0;
		[HorizontalGroup("Item"), ShowIf("@ShowIfOffset"),HideLabel,SuffixLabel("Offset", overlay: true)]
		public float offset = 0;
		[HorizontalGroup("Item"), ShowIf("@ShowIfCurve"),HideLabel]
		public AnimationCurve curve;
		private bool ShowIfScale => option == ExpandOption.Scale || ShowIfOffset;
		private bool ShowIfRange => option == ExpandOption.RangeMinMax || option == ExpandOption.CurveMinMax;
		private bool ShowIfCurve => option == ExpandOption.CurveMinMax || option == ExpandOption.CurveOffset;
		private bool ShowIfOffset => option == ExpandOption.RangeOffset || option == ExpandOption.CurveOffset;
		internal float Scale(in float result)
		{
			return option switch {
				ExpandOption.None => result,
				ExpandOption.Scale => result * scale,
				ExpandOption.RangeMinMax => result * UnityEngine.Random.Range(min, max),
				ExpandOption.CurveMinMax => result * Mathf.Lerp(min, max, curve.Evaluate(UnityEngine.Random.value)),
				ExpandOption.RangeOffset => result * UnityEngine.Random.Range(scale-offset, scale+offset),
				ExpandOption.CurveOffset => result * Mathf.Lerp(scale-offset, scale+offset, curve.Evaluate(UnityEngine.Random.value)),
				_ => result,
			};
		}
	}
}
namespace BC.FormulaItem.Random
{
	public class FormulaRandom : FormulaItem_Value
	{
#if UNITY_EDITOR
		protected override string Type => "Random";
		public override string ToString() => $"R";
#endif
		[HorizontalGroup("Item"), VerticalGroup("Item/MinMax"), LabelText("Min"), LabelWidth(25), SerializeReference]
		public FormulaItem min;
		[HorizontalGroup("Item"), VerticalGroup("Item/MinMax"), LabelText("Max"), LabelWidth(25), SerializeReference]
		public FormulaItem max;
		[HorizontalGroup("Item"), VerticalGroup("Item/MinMax"), HideLabel, SerializeReference, HideReferenceObjectPicker]
		public AnimationCurve curve = new AnimationCurve(new Keyframe(0,0,1f,1f),new Keyframe(1,1,1f,1f));
	}
	public class FormulaRandom01 : FormulaItem_Value
	{
#if UNITY_EDITOR
		protected override string Type => "Random";
		public override string ToString() => $"R(0~1)";
#endif
		[HorizontalGroup("Item"), HideLabel, SerializeReference, HideReferenceObjectPicker]
		public AnimationCurve curve = new AnimationCurve(new Keyframe(0,0,1f,1f),new Keyframe(1,1,1f,1f));
	}
}
namespace BC.FormulaItem.Branch
{
	public class FormulaConditionBool : FormulaItem_Branch
	{
#if UNITY_EDITOR
		protected override string Type => "Condition";
		public override string ToString() => $"Condition[{Bool} => {True}, {False}]";
#endif
		[HorizontalGroup("Item"), HorizontalGroup("Item/A/A"), LabelWidth(35), SerializeReference]
		public FormulaItem Bool;
		[HorizontalGroup("Item"), VerticalGroup(groupId: "Item/A"), LabelWidth(35), SerializeField, SerializeReference]
		public FormulaItem True;
		[HorizontalGroup("Item"), VerticalGroup("Item/A"), LabelWidth(35), SerializeField, SerializeReference]
		public FormulaItem False;

		public FormulaItem ConditionResult(float Value01)
		{
			return (Value01 is > (-float.Epsilon) and < float.Epsilon) ? True : False;
		}
	}
	public class FormulaConditionSelf : FormulaItem_Branch
	{
#if UNITY_EDITOR
		protected override string Type => "Condition";
		public override string ToString() => $"Condition[{conditionOperator} {C} => {True}, {False}]";
#endif
		[HorizontalGroup("Item"), VerticalGroup("Item/A"), HorizontalGroup("Item/A/A",width:70), HideLabel, SerializeField]
		private ConditionOperator conditionOperator;
		[HorizontalGroup("Item"), HorizontalGroup("Item/A/A"), HideLabel, SerializeReference]
		public FormulaItem C;
		private enum ConditionOperator
		{
			[InspectorName("Self == ")] Equal,              // ==
			[InspectorName("Self != ")] NotEqual,           // !=
			[InspectorName("Self > ")] GreaterThan,        // >
			[InspectorName("Self < ")] LessThan,           // <
			[InspectorName("Self >= ")] GreaterThanOrEqual, // >=
			[InspectorName("Self <= ")] LessThanOrEqual     // <=
		}
		[HorizontalGroup("Item"), VerticalGroup(groupId: "Item/A"), LabelWidth(35), SerializeField, SerializeReference]
		public FormulaItem True;
		[HorizontalGroup("Item"), VerticalGroup("Item/A"), LabelWidth(35), SerializeField, SerializeReference]
		public FormulaItem False;

		public FormulaItem ConditionResult(float ValueA, float ValueB)
		{
			bool isEqual = ValueA == ValueB || (Mathf.Abs(ValueA - ValueB) < float.Epsilon);
			return conditionOperator switch {
				ConditionOperator.Equal => isEqual,
				ConditionOperator.NotEqual => !isEqual,
				ConditionOperator.GreaterThan => ValueA > ValueB,
				ConditionOperator.LessThan => ValueA < ValueB,
				ConditionOperator.GreaterThanOrEqual => ValueA > ValueB || isEqual,
				ConditionOperator.LessThanOrEqual => ValueA < ValueB || isEqual,
				_ => true,
			} ? True : False;
		}
	}
	public class FormulaConditionAB : FormulaItem_Branch
	{
#if UNITY_EDITOR
		protected override string Type => "Condition";
		public override string ToString() => $"Condition[{A}, {B}, {comparisonOperator} => {True}, {False}]";
#endif
		[HorizontalGroup("Item"), VerticalGroup("Item/A"), LabelText("A"), LabelWidth(20), SerializeReference]
		public FormulaItem A;
		[HorizontalGroup("Item"), VerticalGroup("Item/A"), LabelText("B"), LabelWidth(20), SerializeReference]
		public FormulaItem B;
		[HorizontalGroup("Item"), VerticalGroup("Item/A"), HideLabel, SerializeField]
		private ComparisonOperator comparisonOperator;
		private enum ComparisonOperator
		{
			[InspectorName("A == B")] Equal,              // ==
			[InspectorName("A != B")] NotEqual,           // !=
			[InspectorName("A > B")] GreaterThan,        // >
			[InspectorName("A < B")] LessThan,           // <
			[InspectorName("A >= B")] GreaterThanOrEqual, // >=
			[InspectorName("A <= B")] LessThanOrEqual     // <=
		}
		[HorizontalGroup("Item"), VerticalGroup("Item/A"), LabelText("True"), LabelWidth(35),HideLabel, SerializeField, SerializeReference]
		private FormulaItem True;
		[HorizontalGroup("Item"), VerticalGroup("Item/A"), LabelText("False"), LabelWidth(35),HideLabel, SerializeField, SerializeReference]
		private FormulaItem False;

		public FormulaItem ComparisonResult(float ValueA, float ValueB)
		{
			bool isEqual = ValueA == ValueB || (Mathf.Abs(ValueA - ValueB) < float.Epsilon);
			return comparisonOperator switch {
				ComparisonOperator.Equal => isEqual,
				ComparisonOperator.NotEqual => !isEqual,
				ComparisonOperator.GreaterThan => ValueA > ValueB,
				ComparisonOperator.LessThan => ValueA < ValueB,
				ComparisonOperator.GreaterThanOrEqual => ValueA > ValueB || isEqual,
				ComparisonOperator.LessThanOrEqual => ValueA < ValueB || isEqual,
				_ => true,
			} ? True : False;
		}
	}
	#endregion
}
