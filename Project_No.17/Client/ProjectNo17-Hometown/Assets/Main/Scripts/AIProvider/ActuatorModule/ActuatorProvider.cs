using System;
using System.Collections.Generic;
using System.Linq;

using BC.ODCC;
using BC.OdccBase;
using BC.Shape;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using UnityEngine;

namespace BC.Actuator
{
	public partial class ActuatorProvider : ComponentBehaviour, IOdccUpdate, IShapeRenderer
	{
		[Serializable]
		public class BuffDebuff
		{
			public ActuatorModule lifeActuatorProvider;
			public AbilityStruct ability;

			public bool IsPlaying()
			{
				return lifeActuatorProvider != null;
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

		[ShowInInspector]
		public Dictionary<ActuatorModule, float> CooldownList = new Dictionary<ActuatorModule, float>();
		protected override void BaseAwake()
		{
			int length = ActiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule original = ActiveActuatorList[i];
				if(original == null)
				{
					ActiveActuatorList.RemoveAt(i);
					i--;
					length--;
					continue;
				}
				ActuatorModule copyModule = Instantiate<ActuatorModule>(original);
				copyModule.name = original.name;
				ActiveActuatorList[i] = copyModule;
			}
			length = PassiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule original = PassiveActuatorList[i];
				if(original == null)
				{
					PassiveActuatorList.RemoveAt(i);
					i--;
					length--;
					continue;
				}
				ActuatorModule copyModule = Instantiate<ActuatorModule>(original);
				copyModule.name = original.name;
				PassiveActuatorList[i] = copyModule;
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
				CooldownUpdate(actuator, in deltaTime);
				OnActiveActuatorUpdate(actuator, in deltaTime);
			}

			length = PassiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule actuator = PassiveActuatorList[i];
				if(actuator == null) continue;
				CooldownUpdate(actuator, in deltaTime);
				OnPassiveActuatorUpdate(actuator, in deltaTime);
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

		void IShapeRenderer.OnDrawShapes(Camera cam)
		{
		}

	}
#if UNITY_EDITOR
	public partial class ActuatorProvider : IShapeRenderer_EditorScriptableObject
	{
		void IShapeRenderer_EditorScriptableObject.Editor_OnDrawShapes()
		{
			if(!UnityEditor.EditorApplication.isPlaying) return;
			if(ThisContainer.TryGetComponent<IReactionTarget>(out var reactionTarget))
			{
				ActiveActuatorList.ForEach(activeActuator => {
					activeActuator.Editor_ShowPlayModeRangePreview(reactionTarget.Position, reactionTarget.Radius);
				});
			}
		}
	}
#endif
	public partial class ActuatorProvider // static
	{
		public static bool TryActiveActuator(ObjectBehaviour Order, ActuatorModule Actuator, ObjectBehaviour Target)
		{
			if(Actuator == null || Target == null) return false;
			if(Actuator.Target.isNonTarget)
			{
				if(Target.ThisContainer.TryGetComponent<IReactionTarget>(out var target))
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
			if(!Order.ThisContainer.TryGetComponent<IAbilityProvider>(out var ability)) return false;

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
	}

	public partial class ActuatorProvider // Active/Passive And BuffDebuff
	{
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

		private void CooldownUpdate(ActuatorModule actuator, in float deltaTime)
		{
			if(CooldownList != null && CooldownList.TryGetValue(actuator, out float cooldown))
			{
				cooldown -= deltaTime;
				if(cooldown < 0f)
				{
					cooldown = 0f;
					CooldownList.Remove(actuator);
				}
				else
				{
					CooldownList[actuator] = cooldown;
				}
			}
		}
		private void OnActiveActuatorUpdate(ActuatorModule actuator, in float deltaTime)
		{

		}
		private void OnPassiveActuatorUpdate(ActuatorModule actuator, in float deltaTime)
		{
			if(CooldownList.TryGetValue(actuator, out float cooldown))
			{
				cooldown -= deltaTime;
			}
		}
	}
	public partial class ActuatorProvider // BuffDebuff
	{
		private bool OnBuffDebuffListUpdate(BuffDebuff buffer, in float deltaTime)
		{
			return buffer.IsPlaying();
		}
	}
	public partial class ActuatorProvider // Compute Combat
	{
		public void OnActuatorPlay(ActuatorModule actuator)
		{
			if(actuator == null) return;
			float cooldown = actuator.Basic?.cooldown ?? 0f;
			if(cooldown > 0f)
			{
				if(CooldownList == null) CooldownList = new Dictionary<ActuatorModule, float>();
				CooldownList.Add(actuator, actuator.Basic.cooldown);
			}
		}
		private bool IsReadyActuator(ActuatorModule actuator)
		{
			if(actuator == null) return false;
			if(!actuator.IsReadyActuator(ThisObject)) return false;
			if(CooldownList != null && CooldownList.ContainsKey(actuator)) return false;
			return true;
		}

		public bool TryGetCanAnyActuatorTarget(IAbilityProvider orderAbility, IAbilityProvider targetAbility, UnitRelationType unitRelationType, int targetLayer, out List<ActuatorModule> actuatorProviderModules)
		{
			actuatorProviderModules = new List<ActuatorModule>();
			int length = ActiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule activeActuator = ActiveActuatorList[i];
				if(IsReadyActuator(activeActuator) && activeActuator.IsCanActuatorTarget(orderAbility, targetAbility, unitRelationType, targetLayer))
				{
					actuatorProviderModules.Add(activeActuator);
				}
			}
			return actuatorProviderModules.Count > 0;
		}
	}
	public partial class ActuatorProvider : ISaveLoadTarget
	{
		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
			var actuatorList = ActiveActuatorList.Where(x => x != null);
			data.SaveValue(nameof(ActiveActuatorList), string.Join(SaveLoadKey.RecordSeparator, actuatorList.Select(i => $"ActuatorModule/{i.name}")));
			actuatorList = PassiveActuatorList.Where(x => x != null);
			data.SaveValue(nameof(PassiveActuatorList), string.Join(SaveLoadKey.RecordSeparator, actuatorList.Select(i => $"ActuatorModule/{i.name}")));

			int length = ActiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule saveActuator = ActiveActuatorList[i];
				if(saveActuator == null) continue;
			}
			length = PassiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule saveActuator = PassiveActuatorList[i];
				if(saveActuator == null) continue;
			}


			data.SaveValue(nameof(CooldownList), string.Join(SaveLoadKey.RecordSeparator, CooldownList.Select(i => $"{i.Key.name}{SaveLoadKey.UnitSeparator}{i.Value}")));
		}

		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			if(data.LoadValue(nameof(ActiveActuatorList), out string actuatorList))
			{
				SetActuatorList(ref ActiveActuatorList, actuatorList);
			}
			if(data.LoadValue(nameof(PassiveActuatorList), out actuatorList))
			{
				SetActuatorList(ref PassiveActuatorList, actuatorList);
			}

			int length = ActiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule saveActuator = ActiveActuatorList[i];
				if(saveActuator == null) continue;
			}
			length = PassiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule saveActuator = PassiveActuatorList[i];
				if(saveActuator == null) continue;
			}

			if(data.LoadValue(nameof(CooldownList), out string cooldownList))
			{
				cooldownList.Split(SaveLoadKey.RecordSeparator).ToList().ForEach(i => {
					string[] split = i.Split(SaveLoadKey.UnitSeparator);
					if(split.Length != 2) return;
					string name = split[0];
					float value = float.Parse(split[1]);
					ActuatorModule actuator = ActiveActuatorList.FirstOrDefault(x => x.name == name);
					if(actuator == null)
					{
						actuator = PassiveActuatorList.FirstOrDefault(x => x.name == name);
					}
					if(actuator != null)
					{
						CooldownList.Add(actuator, value);
					}
				});
			}

			void SetActuatorList(ref List<ActuatorModule> targetActuatorList, string activeActuatorList)
			{
				int length = targetActuatorList.Count;
				for(int i = 0 ; i < length ; i++)
				{
					try
					{
						if(targetActuatorList[i] == null) continue;
						DestroyImmediate(targetActuatorList[i]);
					}
					catch(Exception e)
					{
						Debug.LogError($"Failed to destroy actuator: {e}");
					}
				}
				targetActuatorList.Clear();

				string[] actuatorNames = activeActuatorList.Split(SaveLoadKey.RecordSeparator);
				length = actuatorNames == null ? 0 : actuatorNames.Length;
				for(int i = 0 ; i<actuatorNames.Length ; i++)
				{
					string resourcesPath = actuatorNames[i];
					ActuatorModule asset = Resources.Load<ActuatorModule>(resourcesPath);
					if(asset == null) continue;
					ActuatorModule actuator = Instantiate<ActuatorModule>(asset);
					actuator.name = asset.name;
					targetActuatorList.Add(actuator);
				}
			}
		}

		void ISaveLoadData.OnLoadComplete()
		{
			int length = ActiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule saveActuator = ActiveActuatorList[i];
				if(saveActuator == null) continue;
			}
			length = PassiveActuatorList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				ActuatorModule saveActuator = PassiveActuatorList[i];
				if(saveActuator == null) continue;
			}
		}
	}
}