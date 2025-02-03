using Pathfinding;

using UnityEngine;
using UnityEngine.InputSystem;
namespace TFContent
{
	public class MovePositionCommandController : CommandController<MovePositionCommandListener, (NNInfo nnINfo, Vector3 look)>
	{
		public LayerMask moveLayer;
		public NNConstraint constraint;

		UserInputSystem.CommandActions Command;
		public override void OnPerInputEnable(UserInputSystem inputActions)
		{
			Command = inputActions.Command;
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
	}
}