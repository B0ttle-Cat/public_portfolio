using BC.AIProvider;
using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;




#if UNITY_EDITOR
#endif

using UnityEngine;

namespace BC.Actuator
{
	[CreateAssetMenu(fileName = "ActuatorModule", menuName = "BC/ActuatorModule/ActuatorModule")]
	public partial class ActuatorModule : ScriptableObject, IActuatorModule
	{
		public new string name {
			get {
				if(HasBasic && !string.IsNullOrWhiteSpace(Basic.ActuatorName))
				{
					return Basic.ActuatorName;
				}
				return base.name;
			}
			set {
				if(HasBasic)
				{
					Basic.ActuatorName = value;
				}
				base.name = value;
			}
		}

		[TabGroup("ActuatorModule","ProvideModule", order: 10)]
		[TitleGroup("ActuatorModule/ProvideModule/Basic"), SerializeField, HideLabel, InlineButton("Paste"), InlineButton("Copy"), InlineButton("Remove"), InlineButton("AddBasic", "Add")]
		private ActuatorBasic basic;
		[TitleGroup("ActuatorModule/ProvideModule/HitRange&Shape"), SerializeField,HideLabel,  InlineButton("Paste"), InlineButton("Copy"), InlineButton("Remove"), InlineButton("AddHitShape", "Add")]
		private ActuatorHitShape hitShape;
		[TitleGroup("ActuatorModule/ProvideModule/Priority"), SerializeField, HideLabel, InlineButton("Paste"), InlineButton("Copy"), InlineButton("Remove"), InlineButton("AddPriority", "Add")]
		private ActuatorPriorityOrder priority;
		[TitleGroup("ActuatorModule/ProvideModule/Target"), SerializeField,HideLabel,  InlineButton("Paste"), InlineButton("Copy"), InlineButton("Remove"), InlineButton("AddTarget", "Add")]
		private ActuatorTarget target;
		[TitleGroup("ActuatorModule/ProvideModule/Basic")]
		[ShowInInspector, InlineEditor, ShowIf("HasBasic")]
		public ActuatorBasic Basic { get => basic; private set => basic = value; }
		public bool HasBasic => Basic != null;
		[TitleGroup("ActuatorModule/ProvideModule/HitRange&Shape")]
		[ShowInInspector, InlineEditor, ShowIf("HasHitShape")]
		public ActuatorHitShape HitShape { get => hitShape; private set => hitShape = value; }
		public bool HasHitShape => HitShape != null;
		[TitleGroup("ActuatorModule/ProvideModule/Priority")]
		[ShowInInspector, InlineEditor, ShowIf("HasPriority")]
		public ActuatorPriorityOrder Priority { get => priority; private set => priority = value; }
		public bool HasPriority => Priority != null;
		[TitleGroup("ActuatorModule/ProvideModule/Target")]
		[ShowInInspector, InlineEditor, ShowIf("HasTarget")]
		public ActuatorTarget Target { get => target; private set => target = value; }
		public bool HasTarget => Target != null;
	}
	public partial class ActuatorModule // ProvideModule
	{
		public bool IsReadyActuator(ObjectBehaviour Order)
		{
			return HasBasic && Basic.isActive && IsCostValidation(Order);
		}
		public bool IsCostValidation(ObjectBehaviour Order)
		{
			if(!Order.ThisContainer.TryGetComponent<IAbilityProvider>(out var ability)) return true;

			int shieldCost = Basic.shieldCost;
			int materialCost = Basic.materialCost;
			int batteryCost = Basic.batteryCost;
			int ammoCost = Basic.ammoCost;

			int shieldPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.ShieldPoint));
			int materialPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.MaterialPoint));
			int batteryPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.BatteryPoint));
			int ammoCount = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.AmmoCount));

			if(shieldPoint - shieldCost < 1) return false;
			if(materialPoint - materialCost < 0) return false;
			if(batteryPoint - batteryCost < 0) return false;
			if(ammoCount - ammoCost < 0) return false;

			return true;
		}

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
		public bool IsCanActuatorTarget(IAbilityProvider orderAbility, IAbilityProvider targetAbility, UnitRelationType unitRelationType, int targetLayer)
		{
			return HasTarget && Target.IsCanActuatorTarget(orderAbility, targetAbility, unitRelationType, targetLayer);
		}
		#endregion
	}
	public partial class ActuatorModule // PlayerModule
	{
		[TabGroup("ActuatorModule","PlayerModule", order: 20)]
		[TitleGroup("ActuatorModule/PlayerModule/Animator"), HideLabel, SerializeField, InlineButton("Paste"), InlineButton("Copy"), InlineButton("Remove"), InlineButton("AddAnimator", "Add")]
		private ActuatorPlayAnimator animator;
		[TitleGroup("ActuatorModule/PlayerModule/Animator"), ShowInInspector, InlineEditor, ShowIf("HasAnimator")]
		public ActuatorPlayAnimator Animator { get => animator; private set => animator = value; }
		public bool HasAnimator => Animator != null;
	}
	public partial class ActuatorModule // EffectorModule
	{
		[TabGroup("ActuatorModule","EffectorModule", order: 30)]
		[TitleGroup("ActuatorModule/EffectorModule/Formula"), HideLabel, SerializeField, InlineButton("Paste"), InlineButton("Copy"), InlineButton("Remove"), InlineButton("AddFormula", "Add")]
		private ActuatorFormulaItem formula;
#if UNITY_EDITOR
		[TitleGroup("ActuatorModule/EffectorModule/Formula"), ShowInInspector, LabelText("MainFormulaList"), ShowIf("HasFormula")]
		private ActuatorFormulaSystem[] PreviewFormulaItemList => HasFormula ? formula.GetAllFormulaItemArray() : null;
#endif
		public ActuatorFormulaItem Formula { get => formula; private set => formula = value; }
		public bool HasFormula => formula != null;

		[TabGroup("ActuatorModule","EffectorModule", order: 30)]
		[TitleGroup("ActuatorModule/EffectorModule/Instance"), HideLabel, SerializeField, InlineButton("Paste"), InlineButton("Copy"), InlineButton("Remove"), InlineButton("AddInstance", "Add")]
		private ActuatorInstance instance;
		[TitleGroup("ActuatorModule/EffectorModule/Instance"), ShowInInspector, InlineEditor, ShowIf("HasInstance")]
		public ActuatorInstance Instance { get => instance; private set => instance = value; }
		public bool HasInstance => instance != null;

		bool IActuatorModule.CalculateBooleanEffect(IAbilityProvider order, IAbilityProvider target, ActuatorEffectType actuatorEffectType, bool defaultValue)
		{
			return CalculateBooleanValue(order as AbilityProvider, target as AbilityProvider, actuatorEffectType, defaultValue);
		}
		float IActuatorModule.CalculateFloatEffect(IAbilityProvider order, IAbilityProvider target, ActuatorEffectType actuatorEffectType, float defaultValue)
		{
			return CalculateFloatValue(order as AbilityProvider, target as AbilityProvider, actuatorEffectType, defaultValue);
		}
		bool IActuatorModule.TryCalculateFloatEffect(IAbilityProvider order, IAbilityProvider target, ActuatorEffectType actuatorEffectType, out float result)
		{
			return TryCalculateFloatValue(order as AbilityProvider, target as AbilityProvider, actuatorEffectType, out result);
		}
		public bool HasCalculateEffectType(ActuatorEffectType actuatorEffectType)
		{
			return HasFormula && Formula.HasFormulaItem(actuatorEffectType);
		}
		public bool CalculateBooleanValue(AbilityProvider order, AbilityProvider target, ActuatorEffectType actuatorEffectType, bool defaultValue = false)
		{
			if(HasFormula && Formula.TryCalculate(order, target, actuatorEffectType, out bool result))
			{
				return result;
			}
			return defaultValue;
		}
		public float CalculateFloatValue(AbilityProvider order, AbilityProvider target, ActuatorEffectType actuatorEffectType, float defaultValue = 0f)
		{
			if(HasFormula && Formula.TryCalculate(order, target, actuatorEffectType, out float result))
			{
				return result;
			}
			return defaultValue;
		}
		public bool TryCalculateFloatValue(AbilityProvider order, AbilityProvider target, ActuatorEffectType actuatorEffectType, out float result)
		{
			if(HasFormula && Formula.TryCalculate(order, target, actuatorEffectType, out result))
			{
				return true;
			}
			result = 0f;
			return false;
		}
	}
}