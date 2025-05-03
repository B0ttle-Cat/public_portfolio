using Pathfinding;

using TFSystem.SaveLoad;

using UnityEngine;
using UnityEngine.InputSystem;
namespace TFContent
{
	public class MovementCommandController : CommandController<MovementCommandListener, (NNInfo nnInfo, Vector3 look)>, ISaveLoadTarget
	{
		public LayerMask moveLayer;
		public NNConstraint constraint;

		UserInputSystem.MovementCommandActions Command;
		public override void OnPerInputEnable(UserInputSystem inputActions)
		{
			Command = inputActions.MovementCommand;
			Command.Enable();

			Command.Move.performed += Move_performed;
		}

		private void Move_performed(InputAction.CallbackContext context)
		{
			if(!enabled) return;

			var mousePos = Command.MousePos.ReadValue<Vector2>();
			var camera = CameraSystem.MainCamera;
			var mainPath = WorldSystem.MainPath;

			var camRay = camera.ScreenPointToRay(mousePos);
			if(!FindMoveTarget(out var nnInfo)) return;
			bool FindMoveTarget(out NNInfo nnInfo)
			{
				nnInfo = NNInfo.Empty;
				if(!Physics.Raycast(camRay, out RaycastHit hit, camera.farClipPlane, moveLayer)) return false;

				nnInfo =  mainPath.GetNearest(hit.point, constraint);
				return nnInfo.node != null;
			}

			OnMovePosition(nnInfo);
		}

		public override void OnPerInputUpdate(UserInputSystem inputActions)
		{
			if(!Command.enabled || !enabled) return;

		}
		public override void OnPerInputDisable(UserInputSystem inputActions)
		{
			Command.Disable();
		}

		public void OnMovePosition(NNInfo moveTarget)
		{
			Value = (moveTarget, default);
		}
		public void OnMovePosition(NNInfo moveTarget, Vector3 look)
		{
			Value = (moveTarget, look);
		}

		void ISaveLoadData.OnSave(ref SaveLoadData data) { }
		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			InitValue((default, Vector3.zero));
		}
	}
}