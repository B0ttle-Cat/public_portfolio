using UnityEngine;
namespace TFContent
{
	public class InitSequenceCamera : GameSequenceItem
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