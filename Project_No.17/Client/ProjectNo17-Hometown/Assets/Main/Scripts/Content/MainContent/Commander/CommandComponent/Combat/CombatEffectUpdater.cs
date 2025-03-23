using System;

using BC.AIProvider;
using BC.ODCC;
using BC.OdccBase;
namespace TFContent
{
	public class CombatEffectUpdater : ComponentBehaviour, IOdccUpdate, IActuatorEffectListener
	{
		private UnitCommandComponent UnitCommand { get; set; }
		private UnitCommandData CommandData => UnitCommand.CommandData;
		private AbilityProvider UnitAbility => UnitCommand.UnitAbility;
		private IAbilityStruct EditAbility => UnitAbility;

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
		public void OnCommandUpdater()
		{
			CommandData.IsAlive = !EditAbility.TryGetAbility(AbilityType.ShieldPoint, out float healthPoint) || healthPoint > 0;
		}

		void IActuatorEffectListener.OnEffectValue(ActuatorEffectType actuatorEffectType, float value)
		{
			Action action = actuatorEffectType switch {
				ActuatorEffectType.Damage_Shield => () => OnDamage(value, false),
				ActuatorEffectType.Damage_Shield_Critical => () => OnDamage(value, true),
				ActuatorEffectType.Recovery_Shield => () => OnRecovery(value),
				_ => null,
			};
			if(action == null) return;
			effectBuffer += action;
		}
		protected virtual void OnDamage(float value, bool isCritical)
		{
			if(EditAbility.TryGetAbility(AbilityType.ShieldPoint, out float shieldPoint))
			{
				EditAbility.TrySetAbility(AbilityType.ShieldPoint, shieldPoint - value);
			}
		}
		protected virtual void OnRecovery(float value)
		{
			if(EditAbility.TryGetAbility(AbilityType.ShieldPoint, out float shieldPoint))
			{
				EditAbility.TrySetAbility(AbilityType.ShieldPoint, shieldPoint + value);
			}
		}
	}
}