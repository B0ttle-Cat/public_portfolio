using System;

using BC.AIProvider;
using BC.ODCC;
using BC.OdccBase;

using TFSystem.SaveLoad;
namespace TFContent
{
	public partial class CombatEffectUpdater : CommandUpdater, IOdccUpdate
	{
		private UnitCommandComponent UnitCommand { get; set; }
		private UnitCommandData CommandData => UnitCommand.CommandData;
		private AbilityProvider UnitAbility => UnitCommand.UnitAbility;
		private IAbilityProvider EditAbility => UnitAbility;

		private Action effectBuffer;
		protected override void BaseAwake()
		{
			UnitCommand = ThisContainer.GetComponent<UnitCommandComponent>();
		}
		void IOdccUpdate.BaseUpdate() => OnEffectBufferUpdater();
		private void OnEffectBufferUpdater()
		{
			if(EditAbility == null)
			{
				return;
			}
			if(CommandData.IsDead)
			{
				effectBuffer = null;
				return;
			}
			if(effectBuffer != null)
			{
				effectBuffer.Invoke();
				effectBuffer = null;
			}
		}
		public override void OnCommandUpdater()
		{
			CommandData.IsAlive = !EditAbility.TryGetAbilityValue(AbilityType.ShieldPoint, out float healthPoint) || healthPoint > 0;
		}
	}
	public partial class CombatEffectUpdater : IActuatorEffectListener
	{
		public void OnEffectValue(IProjectileObject projectileObject, ActuatorEffectType actuatorEffectType, float value)
		{
			Action action = actuatorEffectType switch {
				ActuatorEffectType.Damage_Shield_Attack => () => OnDamage(value, false),
				ActuatorEffectType.Damage_Shield_Critical => () => OnDamage(value, true),
				ActuatorEffectType.Recovery_Shield => () => OnRecovery(value),
				_ => null,
			};
			if(action == null) return;
			effectBuffer += action;
		}
		void IActuatorEffectListener.OnEffectValue(ActuatorEffectType actuatorEffectType, float value)
		{
			Action action = actuatorEffectType switch {
				ActuatorEffectType.Damage_Shield_Attack => () => OnDamage(value, false),
				ActuatorEffectType.Damage_Shield_Critical => () => OnDamage(value, true),
				ActuatorEffectType.Recovery_Shield => () => OnRecovery(value),
				_ => null,
			};
			if(action == null) return;
			effectBuffer += action;
		}
		protected virtual void OnDamage(float value, bool isCritical)
		{
			if(EditAbility.TryGetAbilityValue(AbilityType.ShieldPoint, out float shieldPoint))
			{
				EditAbility.TrySetAbilityValue(AbilityType.ShieldPoint, shieldPoint - value);
			}
		}
		protected virtual void OnRecovery(float value)
		{
			if(EditAbility.TryGetAbilityValue(AbilityType.ShieldPoint, out float shieldPoint))
			{
				EditAbility.TrySetAbilityValue(AbilityType.ShieldPoint, shieldPoint + value);
			}
		}


	}
	public partial class CombatEffectUpdater : ISaveLoadTarget
	{
		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
		}

		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
		}
	}
}