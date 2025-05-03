using BC.Base;

using TFSystem.SaveLoad;

using Unity.Cinemachine;

using UnityEngine;

namespace TFContent
{
	public class RTSCameraController : CameraController, ISaveLoadTarget
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
		private CinemachineTargetGroup targetGroup;

		private RTSCameraInputData rtsCameraInput;

		private Vector3 rotationValue;
		private Vector3 smoothVelocity;
		private float cameraDistance;
		protected override void BaseAwake()
		{
			if(camera == null) camera = GetComponentInChildren<Camera>();
			if(cameraBrain == null) cameraBrain = GetComponentInChildren<CinemachineBrain>();
			if(cinemachine == null) cinemachine = GetComponentInChildren<CinemachineCamera>();
			if(positionComposer == null) positionComposer = GetComponentInChildren<CinemachinePositionComposer>();
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
				targetGroup != null;
		}

		protected override void BaseEnable()
		{
			if(!CheckReference()) return;

			RTSCameraInputData.CameraOption cameraOption = rtsCameraInput.cameraOption;
			positionComposer.Damping = cameraOption.trackingDamping;

			cameraBrain.IgnoreTimeScale = cameraOption.usingUnscaledDeltaTime;

			rotationValue = (positionComposer.FollowTarget ?? cinemachine.transform).rotation.eulerAngles;
			smoothVelocity = Vector3.zero;
			cameraDistance = positionComposer.CameraDistance;
		}

		public override void CameraMainUpdate()
		{
			if(!CheckReference()) return;

			RTSCameraInputData.CameraOption cameraOption = rtsCameraInput.cameraOption;
			RTSCameraInputData.CameraInputValue inputValue = rtsCameraInput.inputValue;
			Transform posTarget = targetGroup.transform;
			Transform rotTarget = cinemachine.transform;

			float deltaTime = DeltaTime();
			float distance = cameraDistance;
			bool isCloseZoom = cameraOption.IsCloseZoom(distance, out float nearToFar);

			Vector3 position = posTarget.position;
			position -= GetMoveKeyDelta(in inputValue, in cameraOption, rotTarget, in distance) * deltaTime;
			position += GetMoveDeltaScreenToWorld(in inputValue, posTarget, isCloseZoom);
			posTarget.position = position;

			Vector3 deltaAngle = inputValue.RotateEuler * deltaTime;
			Transform followTransform = positionComposer.FollowTarget;
			Vector3 rotationTarget = followTransform.rotation.eulerAngles;
			rotationTarget += deltaAngle;
			rotationTarget.x = cameraOption.ClampRotationX(rotationTarget.x, nearToFar);
			rotationValue = Vector3.SmoothDamp(rotationValue, rotationTarget, ref smoothVelocity, 0.2f);
			rotTarget.eulerAngles = rotationValue;
			followTransform.rotation = Quaternion.Euler(rotationTarget);

			distance -= inputValue.zoom * deltaTime;
			distance = cameraOption.ClampZoom(distance);
			positionComposer.CameraDistance = cameraDistance = distance;

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

		Vector3 GetMoveDeltaScreenToWorld(in RTSCameraInputData.CameraInputValue cameraInput, Transform posTarget, bool isCloseZoom)
		{
			if(cameraInput.mouseMove.magnitude < float.Epsilon) return Vector3.zero;

			/// keyMoveSpeed
			Vector2 currScreenPos = cameraInput.mousePos;
			Vector2 prevScreenPos = cameraInput.mousePos - cameraInput.mouseMove;

			Ray currRay = camera.ScreenPointToRay(currScreenPos);
			Ray prevRay = camera.ScreenPointToRay(prevScreenPos);
			Plane plane = new Plane(isCloseZoom ? -camera.transform.forward: Vector3.up, posTarget.position);

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

			Vector3 worldDir = currWorldPos - prevWorldPos;
			if(isCloseZoom)
			{
				Vector3 right = camera.transform.right;
				//Vector3 forward = Vector3.Cross(plane.normal, right).normalized;
				Vector3 worldForward = Vector3.Cross(Vector3.up, right).normalized;
				right = Vector3.Project(worldDir, right);
				worldForward = Vector3.Project(worldDir, worldForward);

				return right + worldForward;
			}
			else
			{
				return worldDir;
			}
		}

		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
			data.SaveValue(nameof(rotationValue), rotationValue);
			data.SaveValue(nameof(smoothVelocity), smoothVelocity);
			data.SaveValue(nameof(cameraDistance), cameraDistance);

			data.SaveValue(nameof(camera), camera.transform.GetWorldPose());
			data.SaveValue(nameof(targetGroup), targetGroup.transform.GetWorldPose());
			data.SaveValue(nameof(cinemachine), cinemachine.transform.GetWorldPose());
		}

		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			if(data.LoadValue(nameof(camera), out Pose cameraPose))
			{
				camera.transform.SetWorldPose(cameraPose);
			}
			if(data.LoadValue(nameof(targetGroup), out Pose targetGroupPOse))
			{
				targetGroup.transform.SetWorldPose(targetGroupPOse);
			}
			if(data.LoadValue(nameof(cinemachine), out Pose cinemachinePose))
			{
				cinemachine.transform.SetWorldPose(cinemachinePose);
			}

			data.LoadValue(nameof(rotationValue), out rotationValue, cinemachine.transform.eulerAngles);
			Transform followTransform = positionComposer.FollowTarget;
			followTransform.rotation = Quaternion.Euler(rotationValue);
			data.LoadValue(nameof(smoothVelocity), out smoothVelocity, Vector3.zero);
			data.LoadValue(nameof(cameraDistance), out cameraDistance, positionComposer.CameraDistance);
		}
	}
}
