using System;
using System.Collections.Generic;

using BC.Base;
using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace TFSystem.UI
{
	public abstract class UIViewController<TViewState> : ObjectBehaviour, IUIViewController<TViewState> where TViewState : Enum
	{
		[Title("InitViewState")]
		[SerializeField, EnumToggleButtons, HideLabel]
		[OnValueChanged("Editor_ChangeViewState")]
		private TViewState initViewState;
#if UNITY_EDITOR
		private bool delayAwake { get; set; }
		private void Editor_ChangeViewState()
		{
			IUIViewController<TViewState> uiViewController = this;
			uiViewController.OnInitViewState(initViewState);
		}
#endif

		[ShowInInspector, ReadOnly]
		public TViewState CurrentViewState { get; private set; }
		public TViewState LastViewStateStack {
			get {
				if(stateChangeStack == null)
					stateChangeStack = new List<TViewState>();
				if(stateChangeStack.Count == 0) return initViewState;
				return stateChangeStack[^1];
			}
		}
		[ShowInInspector, ReadOnly]
		protected SystemState ThisSystemState { get; private set; }
		[Serializable]
		private struct ViewState
		{
			[HideLabel,Space]
			public TViewState state;

			[LabelText("Show ViewModel List")]
			[ValueDropdown("UIViewObjectList", IsUniqueList = true, ExcludeExistingValuesInList = true)]
			[ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
			[PropertySpace(0,10)]
			public List<UIViewModelComponent> viewComponent;
#if UNITY_EDITOR
			ValueDropdownList<UIViewModelComponent> UIViewObjectList()
			{
				var thisObject = UnityEditor.Selection.activeObject;
				if(thisObject == null) return new ValueDropdownList<UIViewModelComponent>();
				if(thisObject is not GameObject gameObject) return new ValueDropdownList<UIViewModelComponent>();
				var viewModels = UnityEngine.GameObject.FindObjectsByType<UIViewModelComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);

				ValueDropdownList<UIViewModelComponent> list = new ValueDropdownList<UIViewModelComponent>();
				int length = viewModels.Length;
				for(int i = 0 ; i < length ; i++)
				{
					if(gameObject.scene == viewModels[i].gameObject.scene)
						list.Add(viewModels[i].GetType().Name, viewModels[i]);
				}
				return list;
			}
#endif
		}
		[SerializeField]
		[ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
		private List<ViewState> viewStateList = new List<ViewState>();

		public List<TViewState> stateChangeStack;

		[ShowInInspector, ReadOnly]
		private bool isViewUpdate { get; set; }

		protected override async void BaseAwake()
		{
			isViewUpdate = false;
			if(ThisContainer.TryGetParentObject<SystemState>(out var systemObject))
			{
				ThisSystemState = systemObject;
			}

#if UNITY_EDITOR
			delayAwake = true;
			while(ApplicationController.AppController == null)
			{
				await Awaitable.NextFrameAsync();
			}
			delayAwake = false;
#endif
			AwakeInController();
		}
		protected abstract void AwakeInController();
		protected override void BaseDestroy()
		{
			DestroyInController();
			ThisSystemState = null;
		}
		protected abstract void DestroyInController();
		protected override async void BaseStart()
		{
#if UNITY_EDITOR
			while(delayAwake)
			{
				await Awaitable.NextFrameAsync();
			}
#endif
			StartInController();
		}
		protected abstract void StartInController();


		async Awaitable IUIViewController<TViewState>.OnInitViewState(TViewState viewState)
		{
			await InitViewState(viewState);
		}
		async Awaitable IUIViewController<TViewState>.OnChangeViewState(TViewState viewState, bool updateLastViewStack = true)
		{
			await ChangeViewState(viewState, updateLastViewStack);
		}
		async void IUIViewController<TViewState>.OnChangeViewState(TViewState viewState, Action<TViewState> callback)
		{
			IUIViewController<TViewState> uiViewController = this;
			await uiViewController.OnChangeViewState(viewState);
			callback?.Invoke(CurrentViewState);
		}

		protected async Awaitable InitViewState(TViewState viewState)
		{
			viewState = await CheckChangeState(viewState);

			if(CurrentViewState.Equals(viewState)) return;
			isViewUpdate = true;
			try
			{
				var prevIndex = viewStateList.FindIndex(i => i.state.Equals(CurrentViewState));
				var nextIndex = viewStateList.FindIndex(i => i.state.Equals(viewState));
				List<UIViewModelComponent> prevStateList = prevIndex < 0 ? new List<UIViewModelComponent>() : new List<UIViewModelComponent>(viewStateList[prevIndex].viewComponent);
				List<UIViewModelComponent> nextStateList = nextIndex < 0 ? new List<UIViewModelComponent>() : new List<UIViewModelComponent>(viewStateList[nextIndex].viewComponent);
				RemoveDuplicatesStatet(prevStateList, nextStateList);

				Action deactive = null;
				Action onactive = null;

				int prevCount = prevStateList.Count;
				for(int i = 0 ; i < prevCount ; i++)
				{
					UIViewModelComponent uiViewComponent = prevStateList[i];
					IUIViewModel uiViewModel = uiViewComponent;
					deactive += () => {
						if(uiViewComponent == null || uiViewComponent.GameObject == null) return;
						uiViewModel.InitHide();
						uiViewModel.GameObject.SetActive(false);
					};
				}
				int nextCount = nextStateList.Count;
				for(int i = 0 ; i < nextCount ; i++)
				{
					UIViewModelComponent uiViewComponent = nextStateList[i];
					IUIViewModel uiViewModel = uiViewComponent;
					onactive += () => {
						if(uiViewComponent == null || uiViewComponent.GameObject == null) return;
						uiViewComponent.GameObject.SetActive(true);
						uiViewModel.InitShow();
					};
				}

				CurrentViewState = viewState;
				stateChangeStack ??= new List<TViewState>();
				stateChangeStack.Clear();
				deactive?.Invoke();
				onactive?.Invoke();
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			finally
			{
				isViewUpdate = false;
			}
		}
		protected async Awaitable ChangeViewState(TViewState viewState, bool updateLastViewStack)
		{
			viewState = await CheckChangeState(viewState);

			if(CurrentViewState.Equals(viewState)) return;
			isViewUpdate = true;
			try
			{
				var prevIndex = viewStateList.FindIndex(i => i.state.Equals(CurrentViewState));
				var nextIndex = viewStateList.FindIndex(i => i.state.Equals(viewState));
				List<UIViewModelComponent> prevStateList = prevIndex < 0 ? new () : new List<UIViewModelComponent>(viewStateList[prevIndex].viewComponent);
				List<UIViewModelComponent> nextStateList = nextIndex < 0 ? new () : new List<UIViewModelComponent>(viewStateList[nextIndex].viewComponent);
				RemoveDuplicatesStatet(prevStateList, nextStateList);

				List<Awaitable> showHideAwait = new List<Awaitable>();
				Action deactive = null;
				Action onactive = null;
				int prevCount = prevStateList.Count;
				for(int i = 0 ; i < prevCount ; i++)
				{
					UIViewModelComponent uiViewComponent = prevStateList[i];
					IUIViewModel uiViewModel = uiViewComponent;
					deactive += () => {
						if(uiViewComponent == null || uiViewComponent.GameObject == null) return;
						uiViewComponent.GameObject.SetActive(false);
					};
					showHideAwait.Add(uiViewModel.OnHide());
				}
				int nextCount = nextStateList.Count;
				for(int i = 0 ; i < nextCount ; i++)
				{
					UIViewModelComponent uiViewComponent = nextStateList[i];
					IUIViewModel uiViewModel = uiViewComponent;
					onactive += () => {
						if(uiViewComponent == null || uiViewComponent.GameObject == null) return;
						uiViewComponent.GameObject.SetActive(true);
					};
					showHideAwait.Add(uiViewModel.OnShow());
				}

				if(updateLastViewStack)
				{
					stateChangeStack??= new List<TViewState>();
					PushViewState(stateChangeStack, CurrentViewState);
				}
				CurrentViewState = viewState;
				onactive?.Invoke();
				await AwaitableUtility.ParallelWaitAll(showHideAwait.ToArray());
				deactive?.Invoke();
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}
			finally
			{
				isViewUpdate = false;
			}
		}
		protected abstract Awaitable<TViewState> CheckChangeState(TViewState viewState);

		private void RemoveDuplicatesStatet(List<UIViewModelComponent> prevStateList, List<UIViewModelComponent> nextStateList)
		{
			HashSet<UIViewModelComponent> duplicates = new HashSet<UIViewModelComponent>(prevStateList);
			duplicates.IntersectWith(nextStateList);
			prevStateList.RemoveAll(item => duplicates.Contains(item));
			nextStateList.RemoveAll(item => duplicates.Contains(item));
		}

		public void PushViewState(List<TViewState> list, TViewState newValue)
		{
			int index = list.IndexOf(newValue);

			if(index >= 0)
			{
				// 기존 값 이후로 전부 제거
				list.RemoveRange(index + 1, list.Count - (index + 1));
				list.RemoveAt(index); // 기존 항목 제거 (중복 방지)
			}

			list.Add(newValue); // 마지막에 추가
		}
		public bool HasPrevThisStateInStack(TViewState viewState, out TViewState prevState)
		{
			prevState = LastViewStateStack;
			if(stateChangeStack == null) return false;
			int index = stateChangeStack.IndexOf(viewState);
			if(index < 0) return false;
			if(index == 0) return false;
			prevState = stateChangeStack[index - 1];
			return true;
		}
	}
}
