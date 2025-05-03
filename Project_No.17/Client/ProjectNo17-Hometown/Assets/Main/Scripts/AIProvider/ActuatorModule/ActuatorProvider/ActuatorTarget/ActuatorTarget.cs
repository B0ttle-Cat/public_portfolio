using System;

using BC.Base;
using BC.FormulaItem;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.Actuator
{
	public partial class ActuatorTarget : ScriptableObject
	{
		[TitleGroup("Target",subtitle:"위치 기반 타겟(Non-Target) 여부 / 자동 지정 가능한 대상 및 레이어", alignment: TitleAlignments.Split)]
		[LabelText("Is Non Target")]
		public bool isNonTarget;
		public UnitRelationType targetType;
		public LayerMask hitTargetLayerMask;

		[InlineProperty, HideLabel,TitleGroup("Is Can ActuatorTarget Check",subtitle:"결과가 0(false)이면 False / 1(true)이면 True", alignment: TitleAlignments.Split)]
		public FormulaGroup FormulaSystem;

		internal bool IsCanActuatorTarget(IAbilityProvider orderAbility, IAbilityProvider targetAbility, UnitRelationType unitRelationType, int targetLayer)
		{
			if(unitRelationType == UnitRelationType.None || orderAbility == null || targetAbility == null) return false;
			if(!targetType.HasFlag(unitRelationType)) return false;
			if(targetLayer != hitTargetLayerMask.ToLayer()) return false;

			Func<ActuatorFormulaGetterType, AbilityType, float> _GetAbility = GetAbility;
			if(FormulaEvaluator.TryBoolEvaluate(FormulaSystem, (value) => value > 0, out var isCanReaction, _GetAbility))
			{
				return isCanReaction;
			}
			return false;

			float GetAbility(ActuatorFormulaGetterType getter, AbilityType ability) => getter switch {
				ActuatorFormulaGetterType.Order => orderAbility.GetAbilityValue(ability),
				ActuatorFormulaGetterType.Target => targetAbility.GetAbilityValue(ability),
				_ => 0f,
			};
		}
	}
}
