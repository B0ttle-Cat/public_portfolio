using BC.OdccBase;

using UnityEngine;

namespace TFContent
{
	public class StartBehaviorWorld : OdccBehaviorTarget
	{

		protected override bool OnActionValid()
		{
			return true;
		}

		protected override async Awaitable OnActionUpdate()
		{

			await Awaitable.NextFrameAsync();

		}

		protected override void OnActionEnd()
		{
		}
	}
}