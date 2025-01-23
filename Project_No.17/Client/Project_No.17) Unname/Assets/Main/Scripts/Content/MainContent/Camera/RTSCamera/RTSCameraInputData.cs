using System;

using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;
namespace TFContent
{
	public class RTSCameraInputData : DataObject
	{

		[Serializable]
		public struct CameraOption
		{
			public bool usingUnscaledDeltaTime;
			[Range(0.1f,1f)]
			public float adjustSlowMoveByZoom;

			[Header("Damping")]
			public Vector3 trackingDamping;
			public float rotateDamping;

			[Header("Limit")]
			public Vector2 rotateXMinMax;
			public Vector2 zoomMinMax;


			[Button("Reset")]
			public void Reset()
			{
				usingUnscaledDeltaTime = true;
				adjustSlowMoveByZoom = .2f;

				trackingDamping = Vector3.one;
				rotateDamping = 1;

				rotateXMinMax = new Vector2(30, 90);
				zoomMinMax = new Vector2(5, 20);
			}
			public float ClampRotationX(float rotationX)
			{
				return Mathf.Clamp(rotationX, rotateXMinMax.x, rotateXMinMax.y);
			}
			public float ClampZoom(float cameraDistance)
			{
				return Mathf.Clamp(cameraDistance, zoomMinMax.x, zoomMinMax.y);
			}

			internal float LerpZoomValue(float distance)
			{
				return (distance-zoomMinMax.x)/(zoomMinMax.y - zoomMinMax.x);
			}
		}
		public CameraOption cameraOption;


		[Serializable]
		public struct InputOption
		{
			[Header("Sensitivity")]
			public float moveSensitivity;
			public Vector2 rotateSensitivity;
			public float zoomSensitivity;

			[Header("Invert")]
			public bool moveInvert;
			public bool rotateInvertX;
			public bool rotateInvertY;
			public bool zoomInvert;

			Vector2 MoveInvert => new(moveInvert ? -1 : 1, moveInvert ? -1 : 1);
			Vector2 RotateInvert => new(rotateInvertX ? -1 : 1, rotateInvertY ? -1 : 1);
			float ZoomInvert => zoomInvert ? -1 : 1;

			public Vector2 ApplyMove(Vector2 input) => input * moveSensitivity * MoveInvert;
			public Vector2 ApplyRotate(Vector2 input) => input * rotateSensitivity * RotateInvert;
			public float ApplyZoom(float input) => input * zoomSensitivity * ZoomInvert;

			[Button("Reset")]
			public void Reset()
			{
				moveSensitivity = 1;
				rotateSensitivity = Vector2.one;
				zoomSensitivity = 1;
				moveInvert = false;
				rotateInvertX = false;
				rotateInvertY = false;
				zoomInvert = false;
			}
		}
		public InputOption keyboardOption;
		public InputOption mouseOption;

		[Serializable]
		public struct CameraInputValue
		{
			public Vector2 move;
			public Vector2 mousePos;
			public Vector2 mouseMove;
			public Vector2 rotate;
			public float zoom;

			public Vector3 MoveXZ => new Vector3(move.x, 0, move.y);
			public Vector3 RotateEuler => new Vector3(rotate.y, rotate.x, 0);
		}
		[Space]
		public CameraInputValue inputValue;

		public RTSCameraInputData() : base()
		{

		}

		protected override void Disposing()
		{

		}
	}
}