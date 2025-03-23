using BC.OdccBase;

using Sirenix.OdinInspector;



#if UNITY_EDITOR
#endif

using UnityEngine;

namespace BC.AIProvider
{
	[CreateAssetMenu(fileName = "ActuatorModule", menuName = "BC/ActuatorModule/MainModule")]
	public partial class ActuatorModule : ScriptableObject
	{
		[HideLabel, TitleGroup("Basic"), SerializeField, InlineEditor]
		private ActuatorBasic basic;
		[HideLabel, TitleGroup("Effect"), SerializeField, InlineEditor]
		private ActuatorEffect effect;
		[HideLabel, TitleGroup("HitRange&Shape"), SerializeField, InlineEditor]
		private ActuatorHitShape hitShape;
		[HideLabel, TitleGroup("Priority"), SerializeField, InlineEditor]
		private ActuatorPriorityOrder priority;
		[HideLabel, TitleGroup("Target"), SerializeField, InlineEditor]
		private ActuatorTarget target;

		public ActuatorBasic Basic { get => basic; private set => basic = value; }
		public bool HasBasic => Basic != null;
		public ActuatorEffect Effect { get => effect; private set => effect = value; }
		public bool HasEffect => Effect != null;
		public ActuatorHitShape HitShape { get => hitShape; private set => hitShape = value; }
		public bool HasHitShape => HitShape != null;
		public ActuatorPriorityOrder Priority { get => priority; private set => priority = value; }
		public bool HasPriority => Priority != null;
		public ActuatorTarget Target { get => target; private set => target = value; }
		public bool HasTarget => Target != null;

		#region Effect
		public void ComputeActuatorEffect(AbilityProvider order, AbilityProvider target, ActuatorEffectType actuatorEffectType, out float effectValue)
		{
			effectValue = 0;
			if(Effect != null)
			{
				Effect.OnActuatorEffect(order, target, actuatorEffectType, out effectValue);
			}
		}
		public bool ComputeActuatorEffect(AbilityProvider order, AbilityProvider target, ActuatorEffectType actuatorEffectType)
		{
			return Effect != null && Effect.OnActuatorEffect(order, target, actuatorEffectType, out float effectValue) && Mathf.Abs(effectValue) >= 0.00001f;
		}
		#endregion
		#region HitShape
		public void OnDrawHitShape(Vector3 orderPos, Vector3 targetPos, float pivotRadius)
		{
			if(!HasHitShape) return;
			HitShape.DrawShape(orderPos, targetPos, pivotRadius);
		}
		public void OnOverlapCharacter(Vector3 orderPos, Vector3 targetPos, float pivotRadius, Collider[] colliders, int layerMask = -2)
		{
			if(!HasHitShape) return;
			if(layerMask == -2) layerMask = Target.hitTargetLayerMask.value;
			HitShape.OverlapCharacter(orderPos, targetPos, pivotRadius, colliders, 50f, layerMask, QueryTriggerInteraction.Ignore);
		}
		public Collider[] OnOverlapCharacter(Vector3 orderPos, Vector3 targetPos, float pivotRadius, int layerMask = -2)
		{
			if(!HasHitShape) return new Collider[0];
			if(layerMask == -2) layerMask = Target.hitTargetLayerMask.value;
			return HitShape.OverlapCharacter(orderPos, targetPos, pivotRadius, 50f, layerMask, QueryTriggerInteraction.Ignore);
		}
		public bool OnCheckCharacter(Vector3 orderPos, Vector3 targetPos, float pivotRadius, int layerMask = -2)
		{
			if(!HasHitShape) return false;
			if(layerMask == -2) layerMask = Target.hitTargetLayerMask.value;
			return HitShape.CheckCharacter(orderPos, targetPos, pivotRadius, 50f, layerMask, QueryTriggerInteraction.Ignore);
		}
		public void GetShapeRange(out float nearRange, out float farRange, out float minRange, out float maxRange)
		{
			if(!HasHitShape)
			{
				nearRange = minRange = 0f;
				farRange = maxRange = float.PositiveInfinity;
				return;
			}
			nearRange = HitShape.Range.x;
			farRange = HitShape.Range.y;
			minRange = HitShape.MinRange;
			maxRange = HitShape.MaxRange;
		}
		public bool IsHitUsingShape()
		{
			return HasHitShape && HitShape.HasShape;
		}
		#endregion
		#region Priority
		public void OnPriorityInit()
		{
			if(!HasPriority) return;
			Priority.PriorityInit();
		}
		/*
		public void PriorityUpdate(ObjectBehaviour self, RelationType targetType, in AbilityStruct abilityStruct, in SensorProvider.SensoringData colliderData)
		{
			if(!HasPriority) return;
			Priority.PriorityUpdate(self, targetType, in abilityStruct, in colliderData);
		}
		internal float GetPriorityValue(ObjectBehaviour entity)
		{
			if(!HasPriority) return 0f;
			return Priority.GetPriorityValue(entity);
		}
		*/
		public void ComputePriorityPoint(ICombatCompute combatCompute, in UnitPairsTarget[] unitPairsTarget, out float[] points, out int highestIndex)
		{
			if(!HasPriority || unitPairsTarget == null)
			{
				points = null;
				highestIndex = -1;
				return;
			}
			int length = unitPairsTarget.Length;
			if(length == 0)
			{
				points = new float[0];
				highestIndex = -1;
				return;
			}

			Priority.ComputePriorityPoint(combatCompute, in unitPairsTarget, out points, out highestIndex);
		}

		#endregion
		#region Target
		public bool IsCanActuatorTarget(AbilityProvider orderAbility, AbilityProvider targetAbility, UnitRelationType unitRelationType, int targetLayer)
		{
			return HasTarget && Target.IsCanActuatorTarget(orderAbility, targetAbility, unitRelationType, targetLayer);
		}
		#endregion
	}
}
