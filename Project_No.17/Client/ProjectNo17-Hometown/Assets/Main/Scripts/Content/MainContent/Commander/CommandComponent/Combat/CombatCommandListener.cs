using System;

using BC.Actuator;

using UnityEngine;

namespace TFContent
{
	[Serializable]
	public struct CombatCommandData
	{
		public int selectCombatIndex;
		// 실제 명령을 선택한게 아니라 미리보기 상태인가?
		public bool IsPreview;
		public bool IsSelect { get => selectCombatIndex>=0 && !IsPreview; set => IsPreview = !value; }
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

		protected override async void BaseStart()
		{
			actuatorProvider = await ThisContainer.AwaitGetComponent<ActuatorProvider>();
		}
		public override void CommandListenerUpdate<TCommand>(TCommand order, in CombatCommandData commandValue)
		{
			if(order == null) return;
			if(unitCommand == null && !ThisContainer.TryGetComponent<UnitCommandComponent>(out unitCommand)) return;
			if(actuatorProvider == null|| unitCommand == null) return;

			ActuatorModule actuator = actuatorProvider.GetActiveActuator(commandValue.selectCombatIndex);
			if(actuator == null) return;
		}
	}
}