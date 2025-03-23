using BC.ODCC;

using UnityEngine;
using UnityEngine.InputSystem;

using static UserInputSystem;
namespace TFContent
{
	public class SelectorInput : ComponentBehaviour, IPerInputUpdate
	{
		private SelectorController selectorController;

		public Vector2 pointDownValue;
		public Vector2 pointUpValue;
		public Vector2 pointStayValue;

		private SelectionActions selection;

		//public Action<Vector2> onPointDown;
		//public Action<Vector2> onPointUp;
		//public Action<Vector2> onPointStay;
		//public Action onCancelSelect;
		//public Action onCancelAllSelect;
		//public Action<int> onNumKeys;

		private float minDragDelta = 50f;

		private bool selectFlag;
		private bool includeFlag;
		private bool excludeFlag;

		private bool focusAndPauseFlag;
		private void OnApplicationPause(bool pause)
		{
			selectFlag = false;
			focusAndPauseFlag = true;
			//OnCancelAllSelect();
		}
		private void OnApplicationFocus(bool focus)
		{
			selectFlag = false;
			focusAndPauseFlag = true;
			//OnCancelAllSelect();
		}
		protected override void BaseAwake()
		{
			//Debug.Log("BaseAwake");
			ThisContainer.TryGetComponent<SelectorController>(out selectorController);
		}

		protected override void BaseEnable()
		{
			//onPointDown = OnPointDown;
			//onPointUp = OnPointUp;
			//onPointStay = OnPointStay;
			//onCancelSelect = OnCancelSelect;
			//onCancelAllSelect = OnCancelAllSelect;
			//onNumKeys = OnNumKeys;
		}

		private void OnPointDown(Vector2 point)
		{
			if(!selection.enabled) return;
			if(focusAndPauseFlag) return;
			pointDownValue = point;

			Vector2 startPoint = pointDownValue;

			selectorController.DrawPoint(startPoint, minDragDelta, includeFlag, excludeFlag);
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
				selectorController.SelectPoint(startPoint, minDragDelta, includeFlag, excludeFlag);
			}
			else
			{
				selectorController.SelectRect(startPoint, endPoint, size, minDragDelta, width, height, includeFlag, excludeFlag);
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
				selectorController.PreviewSelectPoint(startPoint, minDragDelta, includeFlag, excludeFlag);
			}
			else
			{
				selectorController.PreviewSelectRect(startPoint, endPoint, size, minDragDelta, width, height, includeFlag, excludeFlag);
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

		protected override void BaseDestroy()
		{
			//onPointDown = null;
			//onPointUp = null;
			//onPointStay = null;
			//onCancelSelect = null;
			//onCancelAllSelect = null;
			//onNumKeys = null;
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
		}
		void Select_performed(InputAction.CallbackContext context)
		{
			selectFlag = true;
			OnPointDown(selection.MousePos.ReadValue<Vector2>());
		}
		void Select_canceled(InputAction.CallbackContext context)
		{
			if(!selectFlag) return;
			selectFlag = false;

			OnPointUp(selection.MousePos.ReadValue<Vector2>());
		}
		void Cancel_performed(InputAction.CallbackContext context)
		{
			if(selectFlag)
			{
				selectFlag = false;
				OnCancelSelect();
			}
			else
			{
				OnCancelAllSelect();
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
		void NumKeys_performed(int num)
		{
			OnNumKeys(num);
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
					OnPointDown(mousePos);
				}
				OnPointStay(mousePos);
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