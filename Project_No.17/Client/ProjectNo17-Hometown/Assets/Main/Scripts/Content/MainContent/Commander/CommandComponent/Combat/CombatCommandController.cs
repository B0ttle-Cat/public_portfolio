using TFSystem.SaveLoad;

using UnityEngine;
using UnityEngine.InputSystem;

namespace TFContent
{
	public class CombatCommandController : CommandController<CombatCommandListener, CombatCommandData>, ISaveLoadTarget
	{
		UserInputSystem.CombatCommandActions Command;
		[SerializeField]
		private CombatCommandData combatCommandData;

		public override void OnPerInputEnable(UserInputSystem inputActions)
		{
			if(CommandSystem == null) return;

			Command = inputActions.CombatCommand;
			Command.Enable();

			combatCommandData.Clear();
			Command.OnAction.performed += OnAction_performed;
			Command.Cancel.performed += Cancel_performed;
			Command.ActionKeyQ.started+=ActionKey_started;
			Command.ActionKeyW.started+=ActionKey_started;
			Command.ActionKeyE.started+=ActionKey_started;
			Command.ActionKeyR.started+=ActionKey_started;
			Command.ActionKeyT.started+=ActionKey_started;
			Command.ActionKeyY.started+=ActionKey_started;
		}

		private void OnAction_performed(InputAction.CallbackContext obj)
		{
			if(CommandSystem == null || CommandSystem.CommandType != CommandSystem.CommandControllerType.CombatCommand) return;
			CommandSystem.CommandType = CommandSystem.CommandControllerType.DefaultCommand;

			Command.OnAction.performed -= OnAction_performed;
			Command.Cancel.performed -= Cancel_performed;

			if(combatCommandData.selectCombatIndex >= 0)
			{
				Vector2 mousePos = Command.MousePos.ReadValue<Vector2>();
				combatCommandData.MousePos = mousePos;
				combatCommandData.IsSelect = true;
				Value = combatCommandData;
			}
			combatCommandData.Clear();
		}

		private void Cancel_performed(InputAction.CallbackContext obj)
		{
			if(CommandSystem == null || CommandSystem.CommandType != CommandSystem.CommandControllerType.CombatCommand) return;
			CommandSystem.CommandType = CommandSystem.CommandControllerType.DefaultCommand;

			Command.OnAction.performed -= OnAction_performed;
			Command.Cancel.performed -= Cancel_performed;

			combatCommandData.Clear();
		}

		private void ActionKey_started(InputAction.CallbackContext obj)
		{
			if(!selectedOnlyOne || CommandSystem == null) return;

			CommandSystem.CommandType = CommandSystem.CommandControllerType.CombatCommand;

			Command.OnAction.performed += OnAction_performed;
			Command.Cancel.performed += Cancel_performed;

			int selectCombatIndex = -1;
			if(Command.ActionKeyQ.id == obj.action.id) selectCombatIndex = 0;
			else if(Command.ActionKeyW.id == obj.action.id) selectCombatIndex = 1;
			else if(Command.ActionKeyE.id == obj.action.id) selectCombatIndex = 2;
			else if(Command.ActionKeyR.id == obj.action.id) selectCombatIndex = 3;
			else if(Command.ActionKeyT.id == obj.action.id) selectCombatIndex = 4;
			else if(Command.ActionKeyY.id == obj.action.id) selectCombatIndex = 5;
			combatCommandData.IsPreview = true;
			combatCommandData.selectCombatIndex = selectCombatIndex;
		}

		public override void OnPerInputDisable(UserInputSystem inputActions)
		{
			Command.Disable();
		}

		public override void OnPerInputUpdate(UserInputSystem inputActions)
		{
			if(CommandSystem == null || CommandSystem.CommandType != CommandSystem.CommandControllerType.CombatCommand) return;
			if(!Command.enabled || !enabled) return;

			Vector2 mousePos = Command.MousePos.ReadValue<Vector2>();
			ActionPreviewDraw(mousePos);
		}

		private void ActionPreviewDraw(Vector2 mousePos)
		{
			if(combatCommandData.selectCombatIndex >= 0 || !combatCommandData.IsPreview) return;
			combatCommandData.MousePos = mousePos;
			Value = combatCommandData;
		}


		void ISaveLoadData.OnSave(ref SaveLoadData data) { }
		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			combatCommandData.Clear();
			InitValue(combatCommandData);
		}
	}
}