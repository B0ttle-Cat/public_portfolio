using BC.OdccBase;

using UnityEngine;
namespace TFContent
{
	public class InitBehaviorScenario : OdccBehaviorTarget
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
			DestroyThis();
		}
	}
}