using BC.OdccBase;

using UnityEngine;
namespace TFContent
{
	public class InitBehaviorCamera : OdccBehaviorTarget
	{
		public CameraManualUpdate manualUpdate;

		protected override bool OnActionValid()
		{
			return manualUpdate != null;
		}

		protected override async Awaitable OnActionUpdate()
		{
			manualUpdate.ManualUpdate();
		}

		protected override void OnActionEnd()
		{
			DestroyThis();
			manualUpdate = null;
		}
	}
}