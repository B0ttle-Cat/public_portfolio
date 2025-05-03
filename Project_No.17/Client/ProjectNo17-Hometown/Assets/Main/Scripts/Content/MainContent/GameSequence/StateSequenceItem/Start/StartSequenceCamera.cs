using UnityEngine;
namespace TFContent
{
	public class StartSequenceCamera : GameSequenceItem
	{
		private CameraMainUpdate cameraMainUpdate;

		protected override bool OnActionValid()
		{
			return ThisContainer.TryGetComponent<CameraMainUpdate>(out cameraMainUpdate);
		}

		protected override async Awaitable OnActionUpdate()
		{
			cameraMainUpdate.enabled = true;
			cameraMainUpdate.MainUpdate();
		}

		protected override void OnActionEnd()
		{
			cameraMainUpdate = null;
			DestroyThis();
		}
	}
}