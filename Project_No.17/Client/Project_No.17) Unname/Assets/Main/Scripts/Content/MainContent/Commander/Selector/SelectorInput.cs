using System;

using BC.ODCC;

using UnityEngine;
using UnityEngine.InputSystem;

using static UserInputSystem;
namespace TFContent
{
	public class SelectorInput : ComponentBehaviour, IPerInputUpdate
	{
		private SelectorController selectorController;
		private SelectorDraw selectorDraw;

		public Vector2 pointDownValue;
		public Vector2 pointUpValue;
		public Vector2 pointStayValue;

		private SelectionActions selection;

		public Action<Vector2> onPointDown;
		public Action<Vector2> onPointUp;
		public Action<Vector2> onPointStay;
		public Action onCancelSelect;
		public Action onCancelAllSelect;
		public Action<int> onNumKeys;
		public Action onInvertSelect;

		private float minDragDelta = 50f;

		private bool selectFlag;
		private bool includeFlag;
		private bool excludeFlag;

		private bool focusAndPauseFlag;
		private void OnApplicationPause(bool pause)
		{
			selectFlag = false;
			focusAndPauseFlag = true;
			onCancelAllSelect?.Invoke();
		}
		private void OnApplicationFocus(bool focus)
		{
			selectFlag = false;
			focusAndPauseFlag = true;
			onCancelAllSelect?.Invoke();
		}
		protected override void BaseAwake()
		{
			ThisContainer.TryGetComponent<SelectorDraw>(out selectorDraw);
			ThisContainer.TryGetComponent<SelectorController>(out selectorController);
		}

		protected override void BaseEnable()
		{
			onPointDown = OnPointDown;
			onPointUp = OnPointUp;
			onPointStay = OnPointStay;
			onCancelSelect = OnCancelSelect;
			onCancelAllSelect = OnCancelAllSelect;
			onNumKeys = OnNumKeys;
			onInvertSelect = OnInvertSelect;
		}

		private void OnPointDown(Vector2 point)
		{
			if(Input.GetKey(KeyCode.S))
			{
				Debug.Log(pointDownValue);
			}

			if(!selection.enabled) return;
			if(focusAndPauseFlag) return;
			pointDownValue = point;

			Vector2 startPoint = pointDownValue;

			if(selectorDraw != null) selectorDraw.DrawPoint(startPoint, minDragDelta, includeFlag, excludeFlag);
		}
		private void OnPointUp(Vector2 point)
		{
			if(!selection.enabled) return;
			pointUpValue = point;

			Vector2 startPoint = pointDownValue;
			Vector2 endPoint = pointUpValue;

			Vector2 size = endPoint - startPoint;
			float width = Mathf.Abs(size.x);
			float height = Mathf.Abs(size.y);
			if(width < minDragDelta*0.5f && height < minDragDelta*0.5f)
			{
				if(selectorDraw != null) selectorDraw.DrawPoint(startPoint, minDragDelta, includeFlag, excludeFlag);
				selectorController.SelectPoint(startPoint, minDragDelta, includeFlag, excludeFlag);
			}
			else
			{
				if(selectorDraw != null) selectorDraw.DrawRect(startPoint, endPoint, size, minDragDelta, width, height, includeFlag, excludeFlag);
				selectorController.SelectRect(startPoint, endPoint, minDragDelta, includeFlag, excludeFlag);
			}
		}
		private void OnPointStay(Vector2 point)
		{
			if(!selection.enabled) return;
			pointStayValue = point;

			Vector2 startPoint = pointDownValue;
			Vector2 endPoint = pointStayValue;

			Vector2 size = endPoint - startPoint;
			float width = Mathf.Abs(size.x);
			float height = Mathf.Abs(size.y);
			if(width < minDragDelta*0.5f && height < minDragDelta*0.5f)
			{
				if(selectorDraw != null) selectorDraw.DrawPoint(startPoint, minDragDelta, includeFlag, excludeFlag);
				selectorController.PreviewSelectPoint(startPoint, minDragDelta);
			}
			else
			{
				if(selectorDraw != null) selectorDraw.DrawRect(startPoint, endPoint, size, minDragDelta, width, height, includeFlag, excludeFlag);
				selectorController.PreviewSelectRect(startPoint, endPoint, minDragDelta);
			}
		}
		private void OnCancelSelect()
		{
			selectorController.OnClearPreviewTarget();
		}
		private void OnCancelAllSelect()
		{
			selectorController.OnClearAllTarget();
		}
		private void OnNumKeys(int number)
		{
			selectorController.OnNumKeys(number);
		}
		private void OnInvertSelect()
		{
			selectorController.OnInvertSelect();
		}

		protected override void BaseDestroy()
		{
			selectorDraw = null;

			onPointDown = null;
			onPointUp = null;
			onPointStay = null;
			onCancelSelect = null;
			onCancelAllSelect = null;
			onNumKeys = null;
			onInvertSelect = null;
		}

		void IPerInputUpdate.OnPerInputEnable(UserInputSystem inputActions)
		{
			selection = inputActions.Selection;
			selection.Enable();

			selection.Select.performed += Select_performed;
			selection.Select.canceled += Select_canceled;
			selection.Cancel.performed += Cancel_performed;

			selection.Include.performed += Include_performed;
			selection.Include.canceled += Include_canceled;
			selection.Exclude.performed += Exclude_performed;
			selection.Exclude.canceled += Exclude_canceled;

			UserInput.NumberInput.performed += NumKeys_performed;
			selection.InvertSelect.performed += InvertSelect_performed;

		}
		void Select_performed(InputAction.CallbackContext context)
		{
			selectFlag = true;
			onPointDown.Invoke(selection.MousePos.ReadValue<Vector2>());
		}
		void Select_canceled(InputAction.CallbackContext context)
		{
			if(!selectFlag) return;
			selectFlag = false;

			onPointUp.Invoke(selection.MousePos.ReadValue<Vector2>());
		}
		void Cancel_performed(InputAction.CallbackContext context)
		{
			if(selectFlag)
			{
				selectFlag = false;
				onCancelSelect.Invoke();
			}
			else
			{
				onCancelAllSelect.Invoke();
			}
		}
		void Exclude_canceled(InputAction.CallbackContext context)
		{
			excludeFlag = false;
		}
		void Exclude_performed(InputAction.CallbackContext context)
		{
			excludeFlag = true;
		}
		void Include_canceled(InputAction.CallbackContext context)
		{
			includeFlag = false;
		}
		void Include_performed(InputAction.CallbackContext context)
		{
			includeFlag = true;
		}

		void InvertSelect_performed(InputAction.CallbackContext context)
		{
			onInvertSelect();
		}
		void NumKeys_performed(int obj)
		{
		}

		void IPerInputUpdate.OnPerInputUpdate(UserInputSystem inputActions)
		{
			if(!selection.enabled) return;

			if(selectFlag)
			{
				Vector2 mousePos = selection.MousePos.ReadValue<Vector2>();
				if(focusAndPauseFlag)
				{
					focusAndPauseFlag = false;
					onPointDown.Invoke(mousePos);
				}
				onPointStay.Invoke(mousePos);
			}
		}
		void IPerInputUpdate.OnPerInputDisable(UserInputSystem inputActions)
		{
			if(!selection.enabled) return;
			selection.Disable();
			UserInput.NumberInput.performed -= NumKeys_performed;
		}
	}
}