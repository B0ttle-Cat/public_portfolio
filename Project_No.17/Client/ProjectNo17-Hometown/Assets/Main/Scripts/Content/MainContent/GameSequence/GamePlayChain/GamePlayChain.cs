using System;

namespace TFContent
{
	public abstract class GamePlayChain : IDisposable
	{
		public Func<bool> enterCondition;

		public void Dispose()
		{
			if(callDisposable) return;
			callDisposable = true;

			OnDispose();
		}
		private bool callDisposable { get; set; }
		protected virtual void OnDispose()
		{

			enterCondition = null;
		}
	}
}
