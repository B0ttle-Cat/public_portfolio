using System;

using BC.Base;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFSystem.UI
{
	public interface UIViewItem
	{
		[DisplayAsString]
		public string ViewItemName { get; set; }
		public void Setup() { SetupView(); SetupValue(); }
		// View가 초기화 또는 업데이트 되야 할 경우
		public void SetupView();
		// 값이 초기화 또는 업데이트 되야 할 경우
		public void SetupValue();
	}
	public interface UIBinding<T>
	{
		public T GetValue();
		public void SetValue(T setValue, bool? _interaction = null);
	}
	public interface UIShowHide
	{
		public IUIShowAndHide IShowAndHide { get; set; }
		public void InitShow() => IShowAndHide.InitShow();
		public void InitHide() => IShowAndHide.InitHide();
		public Awaitable OnShow() => IShowAndHide.OnShow();
		public Awaitable OnHide() => IShowAndHide.OnHide();
	}
	public interface UIEventHandle
	{
		public bool interaction { get; set; }
	}
	public interface UIEvent_OnClick : UIEventHandle
	{
		public Action onClick { get; set; }
	}
	public interface UIEvent_OnSelect : UIEventHandle
	{
		public Action onSelect { get; set; }
	}
	public interface UIEvent_OnSubmit : UIEventHandle
	{
		public Action onSubmit { get; set; }
	}
	public interface UIEvent_OnClick<T> : UIEventHandle
	{
		public Action<T> onClick { get; set; }
	}
	public interface UIEvent_OnSelect<T> : UIEventHandle
	{
		public Action<T> onSelect { get; set; }
	}
	public interface UIEvent_OnSubmit<T> : UIEventHandle
	{
		public Action<T> onSubmit { get; set; }
	}
	public interface UIEvent_OnChangeValue<T> : UIEventHandle
	{
		public ObserverValue<T> onValueChanged { get; set; }
	}


}
