using System;

using BC.AIProvider;

using UnityEngine;

namespace TFContent
{
	[Serializable]
	public struct CombatCommandData
	{
		public int selectCombatIndex;
		public bool IsPreview;
		public bool IsSelect { get => !IsPreview; set => IsPreview = !value; }
		public Vector2 MousePos;
		public void Clear()
		{
			selectCombatIndex = -1;
			IsPreview = false;
			MousePos = Vector2.zero;
		}
	}

	public class CombatCommandListener : CommandListener<CombatCommandData>//, IOdccUpdate
	{
		private UnitCommandComponent unitCommand;
		private ActuatorProvider actuatorProvider;
		//private CombatComputeData combatData;

		private int lastSelectCombatIndex;

		protected override async void BaseStart()
		{
			actuatorProvider = await ThisContainer.AwaitGetComponent<ActuatorProvider>();
			//	combatData = await ThisContainer.AwaitGetData<CombatComputeData>();
		}
		public override void CommandListenerUpdate<TCommand>(TCommand order, in CombatCommandData commandValue)
		{
			if(order == null) return;
			if(unitCommand == null && !ThisContainer.TryGetComponent<UnitCommandComponent>(out unitCommand)) return;
			if(actuatorProvider == null|| unitCommand == null) return;// || combatData ==null) return;

			ActuatorModule actuator = actuatorProvider.GetActiveActuator(commandValue.selectCombatIndex);
			if(actuator == null) return;
		}
	}
}