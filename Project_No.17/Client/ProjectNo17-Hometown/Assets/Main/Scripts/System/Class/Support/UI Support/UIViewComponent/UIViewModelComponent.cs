using System;
using System.Collections.Generic;
using System.Linq;

using BC.Base;

using Sirenix.OdinInspector;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace TFSystem.UI
{
	public abstract class UIViewModelComponent : UIViewComponent, IUIViewModel
	{
#if UNITY_EDITOR
		[ShowInInspector, ValueDropdown("GetType_UIShowAndHide"), OnValueChanged("ChangeType_UIShowAndHide"), HideInPlayMode]
		[ShowIf("@thisUIShowAndHide==null")]
		private Type typeThisUIShowAndHide;
#endif
		[SerializeField, ReadOnly, PropertySpace(0,10)]
		private UIShowAndHide thisUIShowAndHide;

		[SerializeField, ReadOnly]
		private bool onAction = false;
		public IUIShowAndHide ThisUIShowAndHide {
			get {
				if(thisUIShowAndHide == null)
				{
					thisUIShowAndHide = gameObject.GetComponentInChildren<UIShowAndHide>();
				}
				return thisUIShowAndHide;
			}
		}

		private ViewItemSetter viewItemSetter;


		protected struct ViewItemSetter : IDisposable
		{
			private List<UIViewItem> uiViewItemList;
			internal static ViewItemSetter New() => new ViewItemSetter() {
				uiViewItemList = new List<UIViewItem>()
			};

			public bool HsaItem => uiViewItemList != null && uiViewItemList.Count > 0;
			/// <summary>
			/// 1. 초기화 할 UIViewItem 넣기.<br></br>
			/// 2. nameof(uiViewItem) 를 사용하기.
			/// </summary>
			/// <returns></returns>
			public ViewItemSetter Add(UIViewItem uiViewItem, string nameOfViewItem)
			{
				if(uiViewItem == null || nameOfViewItem.IsNullOrWhiteSpace())
				{
					UnityEngine.Debug.LogError($"UIViewItem({uiViewItem}) 또는 ViewItemName({nameOfViewItem}) 은 null 또는 빈 값을 허용하지 않습니다.");
					return this;
				}
				if(uiViewItemList.Contains(uiViewItem)) return this;
				if(uiViewItemList.FindIndex(i => i.ViewItemName.Equals(nameOfViewItem)) >= 0) return this;

				uiViewItemList.Add(uiViewItem);
				uiViewItem.ViewItemName = nameOfViewItem;

				uiViewItem.Setup();
				return this;
			}
			public ViewItemSetter Add<TEnum>(ToggleGroupView<TEnum> uiViewItem, string nameOfViewItem, Action<TEnum> onChangeValue)
			{
				if(uiViewItem == null || nameOfViewItem.IsNullOrWhiteSpace())
				{
					UnityEngine.Debug.LogError($"UIViewItem({uiViewItem}) 또는 ViewItemName({nameOfViewItem}) 은 null 또는 빈 값을 허용하지 않습니다.");
					return this;
				}
				if(uiViewItemList.Contains(uiViewItem)) return this;
				if(uiViewItemList.FindIndex(i => i.ViewItemName.Equals(nameOfViewItem)) >= 0) return this;

				uiViewItemList.Add(uiViewItem);
				uiViewItem.ViewItemName = nameOfViewItem;
				uiViewItem.onValueChanged = new ObserverValue<TEnum>(onChangeValue);
				uiViewItem.Setup();
				return this;
			}
			public ViewItemSetter Add<TEnum>(DropdownView<TEnum> uiViewItem, string nameOfViewItem, Action<TEnum> onChangeValue)
			{
				if(uiViewItem == null || nameOfViewItem.IsNullOrWhiteSpace())
				{
					UnityEngine.Debug.LogError($"UIViewItem({uiViewItem}) 또는 ViewItemName({nameOfViewItem}) 은 null 또는 빈 값을 허용하지 않습니다.");
					return this;
				}
				if(uiViewItemList.Contains(uiViewItem)) return this;
				if(uiViewItemList.FindIndex(i => i.ViewItemName.Equals(nameOfViewItem)) >= 0) return this;

				uiViewItemList.Add(uiViewItem);
				uiViewItem.ViewItemName = nameOfViewItem;
				uiViewItem.onValueChanged = new ObserverValue<TEnum>(onChangeValue);
				uiViewItem.Setup();
				return this;
			}
			public ViewItemSetter Add(TextInputField uiViewItem, string nameOfViewItem, Action<string> onChangeValue)
			{
				if(uiViewItem == null || nameOfViewItem.IsNullOrWhiteSpace())
				{
					UnityEngine.Debug.LogError($"UIViewItem({uiViewItem}) 또는 ViewItemName({nameOfViewItem}) 은 null 또는 빈 값을 허용하지 않습니다.");
					return this;
				}
				if(uiViewItemList.Contains(uiViewItem)) return this;
				if(uiViewItemList.FindIndex(i => i.ViewItemName.Equals(nameOfViewItem)) >= 0) return this;

				uiViewItemList.Add(uiViewItem);
				uiViewItem.ViewItemName = nameOfViewItem;
				uiViewItem.onValueChanged = new ObserverValue<string>(onChangeValue);
				uiViewItem.Setup();
				return this;
			}
			public bool TryGetBinding<T>(string nameOfViewItem, out UIBinding<T> viewItem)
			{
				viewItem = null;
				if(!HsaItem) return false;

				if(nameOfViewItem.IsNotNullOrWhiteSpace())
				{
					int length = uiViewItemList.Count;
					for(int i = 0 ; i < length ; i++)
					{
						UIViewItem item = uiViewItemList[i];
						if(item.ViewItemName.Equals(nameOfViewItem) && item is UIBinding<T> tItem)
						{
							viewItem = tItem;
							break;
						}
					}
				}
				return viewItem != null;
			}
			public bool TryGetShowHide(string nameOfViewItem, out UIShowHide viewItem)
			{
				viewItem = null;
				if(!HsaItem) return false;

				if(nameOfViewItem.IsNotNullOrWhiteSpace())
				{
					int length = uiViewItemList.Count;
					for(int i = 0 ; i < length ; i++)
					{
						UIViewItem item = uiViewItemList[i];
						if(item.ViewItemName.Equals(nameOfViewItem) && item is UIShowHide tItem)
						{
							viewItem = tItem;
							break;
						}
					}
				}
				return viewItem != null;
			}
			public bool TryGetEventHandle<THandle>(string nameOfViewItem, out THandle eventHandle) where THandle : class, UIEventHandle
			{
				eventHandle = null;
				if(!HsaItem) return false;

				if(nameOfViewItem.IsNotNullOrWhiteSpace())
				{
					int length = uiViewItemList.Count;
					for(int i = 0 ; i < length ; i++)
					{
						UIViewItem item = uiViewItemList[i];
						if(item.ViewItemName.Equals(nameOfViewItem) && item is THandle tItem)
						{
							eventHandle = tItem;
							break;
						}
					}
				}
				return eventHandle != null;
			}

			public void Dispose()
			{
				if(uiViewItemList != null)
				{
					uiViewItemList.Clear();
					uiViewItemList = null;
				}
			}

		}
#if UNITY_EDITOR
		private ValueDropdownList<Type> GetType_UIShowAndHide()
		{
			return GetType_List(typeof(UIShowAndHide));
		}
		private void ChangeType_UIShowAndHide()
		{
			if(typeThisUIShowAndHide != null)
			{
				thisUIShowAndHide = ChangeType_AddComp(typeThisUIShowAndHide) as UIShowAndHide;
			}
		}
		private ValueDropdownList<Type> GetType_List(Type baseType)
		{
			ValueDropdownList<Type> list = new ValueDropdownList<Type>();
			var typeList = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t));
			foreach(var item in typeList)
			{
				list.Add(item.Name, item);
			}
			return list;
		}
		private Component ChangeType_AddComp(Type type)
		{
			if(type == null) return null;

			if(typeof(MonoBehaviour).IsAssignableFrom(type))
			{
				if(!gameObject.TryGetComponent(type, out var comp))
				{
					comp = gameObject.AddComponent(type);
					Debug.Log($"[{type.Name}] 컴포넌트가 추가되었습니다.");
				}
				return comp;
			}
			return null;
		}
#endif

		protected override void BaseValidate(in bool isPrefab = false)
		{
			if(isPrefab) return;
			if(ThisUIShowAndHide == null)
			{
				Debug.LogException(new Exception("IUIShowAndHide 컴포넌트가 없습니다. UIViewComponent 에는 최소한 1개의 UIShowAndHide 가 있어야 합니다."));
			}
		}
		sealed protected override async void BaseAwake()
		{
			viewItemSetter = ViewItemSetter.New();
			onAction = false;
#if UNITY_EDITOR
			while(ApplicationController.AppController == null)
			{
				await Awaitable.NextFrameAsync();
			}
#endif
			AwakeUIView(ref viewItemSetter);
		}
		sealed protected override async void BaseEnable()
		{
			await OnShowUIView();
		}
		sealed protected override async void BaseDisable()
		{
			await OnHideUIView();
		}
		sealed protected override void BaseDestroy()
		{
			DestroyUIView();
			viewItemSetter.Dispose();
		}

		protected abstract void AwakeUIView(ref ViewItemSetter viewItemSetter);
		protected virtual void DestroyUIView() { }
		protected virtual async Awaitable OnShowUIView() { await ThisUIShowAndHide.OnShow(); }
		protected virtual async Awaitable OnHideUIView() { await ThisUIShowAndHide.OnShow(); }

		public bool TryGetBinding<T>(string nameOfViewItem, out UIBinding<T> viewItem)
		{
			return viewItemSetter.TryGetBinding(nameOfViewItem, out viewItem);
		}
		public bool TryGetShowHide<T>(string nameOfViewItem, out UIShowHide viewItem)
		{
			return viewItemSetter.TryGetShowHide(nameOfViewItem, out viewItem);
		}
		public bool TryGetEventHandle<THandle>(string nameOfViewItem, out THandle eventHandle) where THandle : class, UIEventHandle
		{
			return viewItemSetter.TryGetEventHandle(nameOfViewItem, out eventHandle);
		}

		protected async Awaitable WaitOnAction(Awaitable awaitable)
		{
			if(onAction) return;
			onAction = true;
			await awaitable;
			onAction = false;
		}
		protected async Awaitable WaitOnAction(Func<Awaitable> awaitable)
		{
			if(awaitable == null) return;
			if(onAction) return;
			onAction = true;
			var _await = awaitable();
			if(_await != null)
			{
				await _await;
			}
			onAction = false;
		}
	}
}
