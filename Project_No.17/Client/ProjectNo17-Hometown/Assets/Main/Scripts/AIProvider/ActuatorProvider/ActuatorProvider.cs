using System;
using System.Collections.Generic;

using BC.ODCC;
using BC.OdccBase;
using BC.Shape;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.AIProvider
{
	public class ActuatorProvider : ComponentBehaviour, IOdccUpdate, IAbilityStruct, IShapeRenderer, IShapeRenderer_EditorScriptableObject
	{
		[Serializable]
		public class BuffDebuff
		{
			public ActuatorModule lifeActuator;
			public AbilityStruct ability;

			public bool IsPlaying()
			{
				return lifeActuator != null && lifeActuator.IsPlaying;
			}
			public bool TryGetAbility(AbilityType abilityType, out float value)
			{
				value = 0;
				return ability != null && ability.TryGetAbility(abilityType, out value);
			}
		}

		[TabGroup("Active Actuator")]
		[ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
		[InlineEditor]
		public List<ActuatorModule> ActiveActuatorList;
		[TabGroup(tab: "Passive Actuator")]
		[ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
		[InlineEditor]
		public List<ActuatorModule> PassiveActuatorList;
		[TabGroup("Active Buff & Debuff")]
		[ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
		public List<BuffDebuff> BuffDebuffList = new List<BuffDebuff>();
		protected override void BaseAwake()
		{
			int length = ActiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule original = ActiveActuatorList[i];
				if(original == null)
				{
					i--;
					continue;
				}
				ActuatorModule copyModule = Instantiate<ActuatorModule>(original);
				//copyModule.CopyNewSubModule();
				ActiveActuatorList[i] = copyModule;
			}
		}

		void IOdccUpdate.BaseUpdate()
		{
			float deltaTime = Time.deltaTime;

			int length = ActiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule actuator = ActiveActuatorList[i];
				if(actuator == null) continue;
				OnActuatorUpdate(actuator, in deltaTime);
			}

			length = PassiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule actuator = PassiveActuatorList[i];
				if(actuator == null) continue;
				OnActuatorUpdate(actuator, in deltaTime);
			}

			length = BuffDebuffList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				BuffDebuff buffer = BuffDebuffList[i];
				if(buffer == null)
				{
					BuffDebuffList.RemoveAt(i--);
					length--;
					continue;
				}
				if(OnBuffDebuffListUpdate(buffer, in deltaTime))
				{
					BuffDebuffList.RemoveAt(i--);
					length--;
				}
			}
		}
		private void OnActuatorUpdate(ActuatorModule actuator, in float deltaTime)
		{
			if(actuator.IsPlaying)
			{

			}
		}

		private bool OnBuffDebuffListUpdate(BuffDebuff buffer, in float deltaTime)
		{
			return buffer.IsPlaying();
		}

		public static bool TryActiveActuator(ObjectBehaviour Order, ActuatorModule Actuator, ObjectBehaviour Target)
		{
			if(Actuator == null || Target == null) return false;
			if(Actuator.Target.isNonTarget)
			{
				if(Target.ThisContainer.TryGetComponent<ReactionTarget>(out var target))
				{
					return TryActiveActuator(Order, Actuator, target.ThisTransform.position);
				}
				return false;
			}
			if(!IsCostValidation(Order, Actuator)) return false;


			return true;
		}
		public static bool TryActiveActuator(ObjectBehaviour Order, ActuatorModule Actuator, Vector3 Target)
		{
			if(Actuator == null) return false;
			if(!IsCostValidation(Order, Actuator)) return false;

			return true;
		}
		private static bool IsCostValidation(ObjectBehaviour Order, ActuatorModule Actuator)
		{
			if(!Order.ThisContainer.TryGetComponent<AbilityProvider>(out var ability)) return false;

			int shieldCost = Actuator.Basic.shieldCost;
			int materialCost = Actuator.Basic.materialCost;
			int batteryCost = Actuator.Basic.batteryCost;
			int ammoCost = Actuator.Basic.ammoCost;

			int shieldPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.ShieldPoint));
			int materialPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.MaterialPoint));
			int batteryPoint = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.BatteryPoint));
			int ammoCount = Mathf.FloorToInt(ability.GetAbilityValue(OdccBase.AbilityType.AmmoCount));

			if(shieldPoint - shieldCost < 1) return false;
			if(materialPoint - materialPoint < 0) return false;
			if(batteryPoint - batteryPoint < 0) return false;
			if(ammoCount - ammoCost < 0) return false;

			return true;
		}
		public ActuatorModule GetActiveActuator(int index)
		{
			if(ActiveActuatorList == null) return null;
			else if(index < 0 || index >= ActiveActuatorList.Count) return null;
			else return ActiveActuatorList[index];
		}
		public ActuatorModule GetPassiveActuator(int index)
		{
			if(ActiveActuatorList == null) return null;
			else if(index < 0 || index >= ActiveActuatorList.Count) return null;
			else return PassiveActuatorList[index];
		}

		public float FurthestReactionRange(bool usingMaxRange)
		{
			float furthestRange = 0f;
			int length = ActiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule actuator = ActiveActuatorList[i];
				if(actuator == null) continue;
				if(!actuator.HasHitShape) continue;
				actuator.GetShapeRange(out _, out float farRange, out _, out float maxRange);
				float range = usingMaxRange ? maxRange : farRange;
				if(furthestRange < range)
				{
					furthestRange = range;
				}
			}
			return furthestRange;
		}

		bool IAbilityStruct.HasAbility(AbilityType abilityType)
		{
			int length = BuffDebuffList ==null ? 0 : BuffDebuffList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var buffer = BuffDebuffList[i];
				if(buffer == null) continue;
				if(buffer.IsPlaying() && buffer.TryGetAbility(abilityType, out _))
				{
					return true;
				}
			}
			return false;
		}

		bool IAbilityStruct.TryGetAbility(AbilityType abilityType, out float value)
		{
			bool hasValue = false;
			value = 0;

			int length = BuffDebuffList == null ? 0 : BuffDebuffList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var buffer = BuffDebuffList[i];
				if(buffer == null) continue;
				if(buffer.IsPlaying() && buffer.TryGetAbility(abilityType, out var _value))
				{
					value += _value;
					hasValue = true;
				}
			}
			return hasValue;
		}


#if UNITY_EDITOR
		void IShapeRenderer_EditorScriptableObject.Editor_OnDrawShapes()
		{
			if(!UnityEditor.EditorApplication.isPlaying) return;
			if(ThisContainer.TryGetComponent<ReactionTarget>(out var reactionTarget))
			{
				ActiveActuatorList.ForEach(activeActuator => {
					activeActuator.Editor_ShowPlayModeRangePreview(reactionTarget.Position, reactionTarget.Radius);
				});
			}
		}
#endif
		void IShapeRenderer.OnDrawShapes(Camera cam)
		{
		}

		public bool TryGetCanAnyActuatorTarget(AbilityProvider orderAbility, AbilityProvider targetAbility, UnitRelationType unitRelationType, int targetLayer, out List<ActuatorModule> actuatorModules)
		{
			actuatorModules = new List<ActuatorModule>();
			int length = ActiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule activeActuator = ActiveActuatorList[i];
				if(activeActuator == null) continue;

				if(activeActuator.IsCanActuatorTarget(orderAbility, targetAbility, unitRelationType, targetLayer))
				{
					actuatorModules.Add(activeActuator);
				}
			}
			return actuatorModules.Count > 0;
		}
	}
}