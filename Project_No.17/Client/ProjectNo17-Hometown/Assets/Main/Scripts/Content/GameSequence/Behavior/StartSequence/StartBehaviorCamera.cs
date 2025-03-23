using BC.OdccBase;

using UnityEngine;
namespace TFContent
{
	public class StartBehaviorCamera : OdccBehaviorTarget
	{
		public CameraManualUpdate manualUpdate;

		protected override bool OnActionValid()
		{
			return manualUpdate != null;
		}

		protected override async Awaitable OnActionUpdate()
		{

			manualUpdate.enabled = true;
			manualUpdate.ManualUpdate();
		}

		protected override void OnActionEnd()
		{
			manualUpdate = null;
		}
	}
}