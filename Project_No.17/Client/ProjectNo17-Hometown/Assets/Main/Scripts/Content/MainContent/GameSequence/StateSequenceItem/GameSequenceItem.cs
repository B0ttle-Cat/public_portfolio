using BC.ODCC;

using UnityEngine;

namespace TFContent
{
	public abstract class GameSequenceItem : ComponentBehaviour
	{
		public async Awaitable<bool> OnPlay()
		{
			try
			{
				if(OnActionValid())
				{
					await OnActionUpdate();
					return true;
				}
				else
				{
					return false;
				}
			}
			catch(System.Exception ex)
			{
				Debug.LogException(ex);
				return false;
			}
		}
		public void OnEnd()
		{
			try
			{
				OnActionEnd();
			}
			catch(System.Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		protected abstract bool OnActionValid();
		protected abstract Awaitable OnActionUpdate();
		protected abstract void OnActionEnd();

#if UNITY_EDITOR
		public override void DestroyThis(bool removeThisGameObject = false)
		{
			if(!UnityEditor.EditorApplication.isPlaying) return;
			base.DestroyThis(removeThisGameObject);
		}
#endif
	}
}
