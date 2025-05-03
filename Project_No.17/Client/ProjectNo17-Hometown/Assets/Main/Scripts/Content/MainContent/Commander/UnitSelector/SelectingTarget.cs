using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class SelectingTarget : ComponentBehaviour, IOdccUpdate.Late
	{
		private SelectingHandle selectingHandle;

		public enum SelectState
		{
			UnSelect = 0,
			IsProvisional,
			IsSelect,
		}
		private SelectState selectState;
		private SelectState lastSelectState;

		protected override void BaseValidate(in bool isPrefab = false)
		{

		}
		protected override void BaseAwake()
		{
			selectState = SelectState.UnSelect;
			lastSelectState = SelectState.UnSelect;
			selectingHandle = null;
			OnHideSelecting();
		}
		protected override void BaseStart()
		{
			OnHideSelecting();
		}
		public void ShowProvisionalSelecting()
		{
			OnShowSelecting(true);
		}
		public void ShowSelecting()
		{
			OnShowSelecting(false);
		}
		public void HideSelecting()
		{
			OnHideSelecting();
		}
		void OnShowSelecting(bool isProvisional)
		{
			selectState = isProvisional ? SelectState.IsProvisional : SelectState.IsSelect;
		}
		void OnHideSelecting()
		{
			selectState = SelectState.UnSelect;
		}
		void IOdccUpdate.Late.BaseLateUpdate()
		{
			if(selectState == lastSelectState) return;

			lastSelectState = selectState;

			switch(selectState)
			{
				case SelectState.UnSelect:
					if(selectingHandle != null)
					{
						Destroy(selectingHandle.gameObject);
					}
					selectingHandle = null;
					break;
				case SelectState.IsProvisional:
					if(selectingHandle == null)
					{
						var handleObj = new GameObject("SelectingHandle");
						handleObj.transform.parent = ThisTransform;
						selectingHandle = handleObj.AddComponent<SelectingHandle>();
					}
					selectingHandle.IsProvisional = true;
					break;
				case SelectState.IsSelect:
					if(selectingHandle == null)
					{
						var handleObj = new GameObject("SelectingHandle");
						handleObj.transform.parent = ThisTransform;
						selectingHandle = handleObj.AddComponent<SelectingHandle>();
					}
					selectingHandle.IsProvisional = false;
					break;
				default:
					break;
			}
		}


		public bool OnCheckPointSelecting(Vector2 screenPoint)
		{
			var helpers = ThisContainer.GetAllComponent<SelectingHelper>();
			int length = helpers.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var helper = helpers[i];
				if(helper == null || !helper.isActiveAndEnabled) continue;

				if(helper.OnCheckPointSelecting(in screenPoint))
				{
					return true;
				}
			}
			return false;
		}
		public bool OnCheckRectSelecting(Rect screenRect)
		{
			var helpers = ThisContainer.GetAllComponent<SelectingHelper>();
			int length = helpers.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var helper = helpers[i];
				if(helper == null || !helper.isActiveAndEnabled) continue;

				if(helper.OnCheckRectSelecting(in screenRect))
				{
					return true;
				}
			}
			return false;
		}
	}
}