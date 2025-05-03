using System;

using BC.ODCC;

using UnityEngine;

namespace TFSystem
{
	public interface IUIViewController<TViewState> : IOdccObject where TViewState : Enum
	{
		public TViewState CurrentViewState { get; }
		public TViewState LastViewStateStack { get; }
		public bool HasPrevThisStateInStack(TViewState viewState, out TViewState prevState);
		/// <summary>
		/// 지정된 뷰로 강제로 변경함. 거의 모든 경우에서 OnChangeViewState 를 사용할것.
		/// </summary>
		public Awaitable OnInitViewState(TViewState viewState);
		/// <summary>
		/// 지정된 뷰로 변경함.
		/// </summary>
		/// <param name="viewState"></param>
		/// <returns></returns>
		public Awaitable OnChangeViewState(TViewState viewState, bool updateLastViewStack = true);
		public async Awaitable OnCloseViewState(TViewState viewState)
		{
			if(HasPrevThisStateInStack(viewState, out var prevState))
			{
				await OnChangeViewState(prevState, true);
			}
			else if(viewState.Equals(CurrentViewState))
			{
				await OnChangeViewState(LastViewStateStack, true);
			}
		}
		public void OnChangeViewState(TViewState viewState, Action<TViewState> callback);

	}
}
