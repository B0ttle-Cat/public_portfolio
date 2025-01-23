using BC.OdccBase;

using UnityEngine;
namespace TFContent
{
	public class StartBehaviorScenario : OdccBehaviorTarget
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
			DestroyThis();
		}
	}
}