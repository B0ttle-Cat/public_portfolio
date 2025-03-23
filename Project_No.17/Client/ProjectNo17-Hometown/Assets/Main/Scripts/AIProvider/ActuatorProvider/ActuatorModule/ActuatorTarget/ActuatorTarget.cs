using System;

using BC.Base;
using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

using static BC.AIProvider.ActuatorEffect;

namespace BC.AIProvider
{
	public partial class ActuatorTarget : ScriptableObject
	{
		[TitleGroup("Target",subtitle:"위치 기반 타겟(Non-Target) 여부 / 자동 지정 가능한 대상 및 레이어", alignment: TitleAlignments.Split)]
		[LabelText("Is Non Target")]
		public bool isNonTarget;
		public UnitRelationType targetType;
		public LayerMask hitTargetLayerMask;

		[InlineProperty, HideLabel,TitleGroup("Is Can ActuatorTarget Check",subtitle:"결과가 0(false)이면 False / 1(true)이면 True", alignment: TitleAlignments.Split)]
		public FormulaEvaluator.FormulaGroup FormulaSystem;

		internal bool IsCanActuatorTarget(AbilityProvider orderAbility, AbilityProvider targetAbility, UnitRelationType unitRelationType, int targetLayer)
		{
			if(unitRelationType == UnitRelationType.None || orderAbility == null || targetAbility == null) return false;
			if(!targetType.HasFlag(unitRelationType)) return false;
			if(targetLayer != hitTargetLayerMask.ToLayer()) return false;

			Func<GetterType, AbilityType, float> _GetAbility = GetAbility;
			if(FormulaEvaluator.TryBoolEvaluate(FormulaSystem, (value) => value > 0, out var isCanReaction, _GetAbility))
			{
				return isCanReaction;
			}
			return false;

			float GetAbility(GetterType getter, AbilityType ability) => getter switch {
				GetterType.Order => orderAbility.GetAbilityValue(ability),
				GetterType.Target => targetAbility.GetAbilityValue(ability),
				_ => 0f,
			};
		}

#if UNITY_EDITOR
		[TitleGroup("Is Can ActuatorTarget Check")]
		[Button("Test Can ActuatorTarget (Walk Only Play Mode)")]
		private bool TestCanActuatorTarget(ObjectBehaviour order, ObjectBehaviour target, UnitRelationType unitRelationType)
		{

			if(order == null || target == null) return false;

			var orderIFF = order.GetComponentInChildren<IFFProvider>(true);
			if(orderIFF == null) return false;
			var orderAbility = order.GetComponentInChildren<AbilityProvider>(true);
			if(orderAbility == null) return false;

			var targetIFF = target.GetComponentInChildren<IFFProvider>(true);
			if(targetIFF == null) return false;
			var targetAbility = target.GetComponentInChildren<AbilityProvider>(true);
			if(targetAbility == null) return false;

			var reactionTarget = target.GetComponentInChildren<ReactionTarget>(true);
			if(reactionTarget != null && !hitTargetLayerMask.HasLayer(reactionTarget.Layer)) return false;

			if(UnityEditor.EditorApplication.isPlaying)
			{
				unitRelationType = IFFProvider.IFFMatching.GetUnitRelation(orderIFF, targetIFF);
			}
			return IsCanActuatorTarget(orderAbility, targetAbility, unitRelationType, reactionTarget.Layer);
		}
#endif
	}
}
