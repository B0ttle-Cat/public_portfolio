using BC.OdccBase;

using UnityEngine;
namespace TFContent
{
	public class InitBehaviorCamera : OdccBehaviorTarget
	{
		protected override bool OnActionValid()
		{
			return true;
		}

		protected override async Awaitable OnActionUpdate()
		{
		}

		protected override void OnActionEnd()
		{
		}
	}
}