using BC.ODCC;

using UnityEngine;

namespace TFContent
{
	public class CameraSystem : ObjectBehaviour
	{
		public static Camera MainCamera;
		protected override void BaseAwake()
		{
			Camera.main.tag = "Untagged";
			MainCamera = GetComponentInChildren<Camera>(true);
			MainCamera.tag = "MainCamera";
		}
	}
}
