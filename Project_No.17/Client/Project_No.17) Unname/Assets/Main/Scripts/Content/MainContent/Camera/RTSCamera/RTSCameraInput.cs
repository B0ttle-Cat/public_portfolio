using BC.ODCC;

using UnityEngine;

namespace TFContent
{
	public class RTSCameraInput : ComponentBehaviour, IPerInputUpdate
	{
		[SerializeField]
		private new Camera camera;
		[SerializeField]
		private Transform followTarget;

		private RTSCameraInputData inputData;

		bool mouseMoveFlag;
		Vector2 prev_mouseMove;

		protected override void BaseEnable()
		{
			if(!ThisContainer.TryGetData<RTSCameraInputData>(out inputData))
			{
				inputData = ThisContainer.AddData<RTSCameraInputData>();
			}
		}
		protected override void BaseDestroy()
		{
			inputData = null;
		}
		UserInputSystem _inputActions;
		void IPerInputUpdate.OnPerInputEnable(UserInputSystem inputActions)
		{
			_inputActions = inputActions;
			var rtsCamera = inputActions.RTSCamera;
			rtsCamera.Enable();

			mouseMoveFlag = false;
		}

		void IPerInputUpdate.OnPerInputUpdate(UserInputSystem inputActions)
		{
			var rtsCamera = inputActions.RTSCamera;
			if(!rtsCamera.enabled) return;
			var keyboardOption = inputData.keyboardOption;
			var mouseOption = inputData.mouseOption;

			Vector2 rotateInputValue = Vector2.zero;
			float zoomInputValue = 0;

			bool isPressedRotate = rtsCamera.Rotate.IsPressed();

			Vector2 keyMove = rtsCamera.Move.ReadValue<Vector2>();
			Vector2 mouseMove = rtsCamera.MouseMove.ReadValue<Vector2>();
			bool isPressedMouseMove = rtsCamera.MouseMove.IsPressed();
			Vector2 mouseMove_Delta = Vector2.zero;
			mouseMove_Delta = isPressedMouseMove&&mouseMoveFlag ? (mouseMove - prev_mouseMove) : Vector2.zero;
			prev_mouseMove = mouseMove;
			mouseMoveFlag = isPressedMouseMove;

			if(isPressedRotate)
			{
				rotateInputValue += keyboardOption.ApplyRotate(keyMove);
				rotateInputValue += mouseOption.ApplyRotate(mouseMove_Delta);
				zoomInputValue += keyboardOption.ApplyZoom(rtsCamera.Zoom.ReadValue<float>());
			}
			else
			{
				keyMove = keyboardOption.ApplyMove(keyMove);
				mouseMove_Delta = mouseOption.ApplyMove(mouseMove_Delta);
				rotateInputValue = keyboardOption.ApplyRotate(Vector2.up * rtsCamera.Zoom.ReadValue<float>());
			}
			zoomInputValue += mouseOption.ApplyZoom(rtsCamera.MouseZoom.ReadValue<float>());

			var inputValue = inputData.inputValue;
			inputValue.move = isPressedRotate ? Vector2.zero : keyMove;
			inputValue.mouseMove = isPressedRotate ? Vector2.zero : mouseMove_Delta;
			inputValue.mousePos = isPressedRotate ? Vector2.zero : mouseMove;
			inputValue.rotate = isPressedRotate ? rotateInputValue : Vector2.zero;
			inputValue.zoom = zoomInputValue;
			inputData.inputValue = inputValue;
		}

		void IPerInputUpdate.OnPerInputDisable(UserInputSystem inputActions)
		{
			var rtsCamera = inputActions.RTSCamera;

			rtsCamera.Disable();
		}
	}
}
