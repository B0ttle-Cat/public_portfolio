using BC.ODCC;

using TFSystem.SaveLoad;

using UnityEngine;

namespace TFContent
{
	public class CameraSystem : ObjectBehaviour, ISaveLoadTarget
	{
		private static Camera mainCamera;
		public static Camera MainCamera => mainCamera != null ? mainCamera : Camera.main;
		protected override void BaseAwake()
		{
			InitMainCamera();
		}
		private void InitMainCamera()
		{
			mainCamera = GetComponentInChildren<Camera>(true);
			if(mainCamera.tag != "MainCamera")
			{
				Camera.main.tag = "Untagged";
			}
			mainCamera.tag = "MainCamera";
		}

		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
			ISaveLoadTarget.SaveThisDataObject(ThisContainer, ref data);
		}

		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			ISaveLoadTarget.LoadThisDataObject(ThisContainer, in data);
		}
	}
}
