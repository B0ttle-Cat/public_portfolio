using System;
using System.Collections.Generic;

namespace TFContent
{
	public class GamePlayStepChain : GamePlayChain
	{
		public Func<bool> exitCondition;
		public List<GamePlayChain> nextPlayChain;
		protected override void OnDispose()
		{
			base.OnDispose();
			exitCondition = null;
			if(nextPlayChain == null)
			{
				foreach(var next in nextPlayChain)
				{
					next?.Dispose();
				}
				nextPlayChain.Clear();
				nextPlayChain = null;
			}
		}
	}
}
