using System;
using System.Threading;

using Sirenix.OdinInspector;

using UnityEngine;

using Object = UnityEngine.Object;

namespace TFContent
{
	[Serializable]
	public abstract class GameStateSequence
	{
		[ShowInInspector, PropertyOrder(-99)]
		public bool IsPlay { get; private set; } = false;
		public void OnStart()
		{
			if(!IsPlay)
			{
				IsPlay = true;
				Start();
			}
		}
		public async Awaitable OnUpdate(CancellationToken token)
		{
			if(IsPlay)
			{
				await Update(token);
			}
		}
		public void OnEnd()
		{
			if(IsPlay)
			{
				IsPlay = false;
				End();
			}
		}

		protected abstract void Start();
		protected abstract Awaitable Update(CancellationToken token);
		protected abstract void End();
		protected bool FindGameSequenceItem<T>(out T t) where T : GameSequenceItem
		{
			t = Object.FindAnyObjectByType<T>();
			return t != null;
		}
		protected async Awaitable<bool> NextWork(GameSequenceItem item)
		{
			bool isWorkBreak = true;
			try
			{
				if(item != null)
				{
					isWorkBreak = !(await item.OnPlay());
					item.OnEnd();
				}
			}
			catch(System.Exception ex)
			{
				Debug.LogException(ex);
				isWorkBreak = true;
			}
			if(isWorkBreak)
			{
				if(item != null)
				{
					Debug.LogError($"{item.GetType().Name} Work Is Fail");
				}
				else
				{
					Debug.LogError($"GameSequenceItem Is Null");
				}
			}
			return isWorkBreak;
		}
	}
}
