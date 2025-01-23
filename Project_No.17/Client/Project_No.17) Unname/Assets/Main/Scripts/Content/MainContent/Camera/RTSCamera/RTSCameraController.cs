using Unity.Cinemachine;

using UnityEngine;

namespace TFContent
{
	public class RTSCameraController : CameraController
	{
		[SerializeField]
		private new Camera camera;
		[SerializeField]
		private CinemachineBrain cameraBrain;
		[SerializeField]
		private CinemachineCamera cinemachine;
		[SerializeField]
		private CinemachinePositionComposer positionComposer;
		[SerializeField]
		private CinemachineRotateWithFollowTarget rotateComposer;
		[SerializeField]
		private CinemachineTargetGroup targetGroup;

		private RTSCameraInputData rtsCameraInput;

		protected override void BaseAwake()
		{
			if(camera == null) camera = GetComponentInChildren<Camera>();
			if(cameraBrain == null) cameraBrain = GetComponentInChildren<CinemachineBrain>();
			if(cinemachine == null) cinemachine = GetComponentInChildren<CinemachineCamera>();
			if(positionComposer == null) positionComposer = GetComponentInChildren<CinemachinePositionComposer>();
			if(rotateComposer == null) rotateComposer = GetComponentInChildren<CinemachineRotateWithFollowTarget>();
			if(targetGroup == null) targetGroup = GetComponentInChildren<CinemachineTargetGroup>();

			if(!ThisContainer.TryGetData<RTSCameraInputData>(out rtsCameraInput))
			{
				rtsCameraInput = ThisContainer.AddData<RTSCameraInputData>();
			}
		}

		bool CheckReference()
		{
			return camera != null &&
				cameraBrain != null &&
				cinemachine != null &&
				positionComposer != null &&
				rotateComposer != null &&
				targetGroup != null;
		}

		protected override void BaseEnable()
		{
			if(!CheckReference()) return;

			RTSCameraInputData.CameraOption cameraOption = rtsCameraInput.cameraOption;
			positionComposer.Damping = cameraOption.trackingDamping;
			rotateComposer.Damping = cameraOption.rotateDamping;

			cameraBrain.IgnoreTimeScale = cameraOption.usingUnscaledDeltaTime;
		}

		public override void ManualUpdate()
		{
			if(!CheckReference()) return;

			RTSCameraInputData.CameraOption cameraOption = rtsCameraInput.cameraOption;
			RTSCameraInputData.CameraInputValue inputValue = rtsCameraInput.inputValue;
			Transform target = targetGroup.transform;

			Vector3 position = target.position;
			Vector3 eulerAngles = target.eulerAngles;
			float distance = positionComposer.CameraDistance;

			float deltaTime = DeltaTime();
			position += GetMoveKeyDelta(in inputValue, in cameraOption, target, in distance) * deltaTime;
			position += GetMoveDeltaScreenToWorld(in inputValue, target);
			eulerAngles += inputValue.RotateEuler * deltaTime;
			distance -= inputValue.zoom * deltaTime;

			eulerAngles.x = cameraOption.ClampRotationX(eulerAngles.x);
			distance = cameraOption.ClampZoom(distance);

			target.position = position;
			target.eulerAngles = eulerAngles;
			positionComposer.CameraDistance = distance;

			float DeltaTime()
			{
				return cameraOption.usingUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;
			}
		}

		Vector3 GetMoveKeyDelta(in RTSCameraInputData.CameraInputValue cameraInput, in RTSCameraInputData.CameraOption cameraOption, Transform target, in float distance)
		{
			if(cameraInput.move.magnitude < float.Epsilon) return Vector3.zero;

			/// keyMoveSpeed
			Vector3 keyMoveDelta = cameraInput.MoveXZ;
			Vector3 keyMoveSpeed = Vector3.zero;
			keyMoveSpeed = Vector3.Lerp(keyMoveDelta * cameraOption.adjustSlowMoveByZoom, keyMoveDelta, cameraOption.LerpZoomValue(distance));

			Vector3 targetRight = target.right;
			targetRight.y = 0f;
			targetRight.Normalize();
			Vector3 targetForward = Vector3.Cross(targetRight, Vector3.up);
			targetForward.Normalize();

			keyMoveSpeed = targetRight * keyMoveSpeed.x +  targetForward * keyMoveSpeed.z;
			return keyMoveSpeed;
		}

		Vector3 asasd = Vector3.zero;
		Vector3 GetMoveDeltaScreenToWorld(in RTSCameraInputData.CameraInputValue cameraInput, Transform target)
		{
			if(cameraInput.mouseMove.magnitude < float.Epsilon) return Vector3.zero;

			/// keyMoveSpeed
			Vector2 currScreenPos = cameraInput.mousePos;
			Vector2 prevScreenPos = cameraInput.mousePos - cameraInput.mouseMove;

			Plane plane = new Plane(Vector3.up, target.position);
			Ray currRay = camera.ScreenPointToRay(currScreenPos);
			Ray prevRay = camera.ScreenPointToRay(prevScreenPos);

			Vector3 currWorldPos = Vector3.zero;
			Vector3 prevWorldPos = Vector3.zero;

			if(plane.Raycast(currRay, out float currDistance))
			{
				currWorldPos = currRay.GetPoint(currDistance);
			}
			if(plane.Raycast(prevRay, out float prevDistance))
			{
				prevWorldPos = prevRay.GetPoint(prevDistance);
			}

			asasd += currWorldPos - prevWorldPos;
			return currWorldPos - prevWorldPos;
		}
	}
}
