using System.Collections.Generic;
using System.Linq;

using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class SelectorController : ComponentBehaviour, ISelectorController
	{
		ISelectorCommandListener selectorCommandListener;

		OdccQuerySystem querySelectingHelper;
		OdccQueryCollector collectorSelectingHelper;
		OdccQueryLooper onCheckPointSelecting;
		OdccQueryLooper onCheckRectSelecting;

		private bool IsCanMultiSelect => selectorCommandListener?.CanMultiSelect() ?? false;
		private float minDragDelta;
		private Vector3? checkPoint;
		private Rect? checkRect;

		private List<SelectingTarget> selectingTargets;
		private List<SelectingTarget> previewSelectingTargets;

		private SelectingTarget checkSelectingTarget;
		private HashSet<SelectingTarget> checkSelectingTargetList;

		private SelectorDraw selectorDraw;

		protected override void BaseAwake()
		{
			selectingTargets = new List<SelectingTarget>();
			previewSelectingTargets = new List<SelectingTarget>();
			checkSelectingTarget = null;
			checkSelectingTargetList = new HashSet<SelectingTarget>();

			ThisContainer.TryGetComponent<SelectorDraw>(out selectorDraw);
			ThisContainer.TryGetObject<ISelectorCommandListener>(out selectorCommandListener);

			querySelectingHelper = OdccQueryBuilder.CreateQuery()
				.WithAll<SelectingTarget>().Build(ThisScene);
			collectorSelectingHelper = OdccQueryCollector.CreateQueryCollector(querySelectingHelper, ThisScene)
				.CreateActionEvent(nameof(onCheckPointSelecting), out onCheckPointSelecting)
					.CallForeach<SelectingTarget>(OnCheckPointSelecting).GetCollector()
				.CreateActionEvent(nameof(onCheckRectSelecting), out onCheckRectSelecting)
					.CallForeach<SelectingTarget>(OnCheckRectSelecting).GetCollector();
		}
		protected override void BaseDestroy()
		{
			if(querySelectingHelper != null)
			{
				collectorSelectingHelper.DeleteActionEvent(nameof(onCheckPointSelecting));
				collectorSelectingHelper.DeleteActionEvent(nameof(onCheckRectSelecting));
				collectorSelectingHelper = null;
				onCheckRectSelecting = null;
				onCheckPointSelecting = null;
				querySelectingHelper = null;
			}

			checkPoint = null;
			checkRect = null;

			checkSelectingTarget = null;
			checkSelectingTargetList = null;

			selectingTargets = null;
			previewSelectingTargets = null;
		}

		private void OnCheckPointSelecting(OdccQueryLooper.LoopInfo loopInfo, SelectingTarget target)
		{
			if(target.OnCheckPointSelecting(checkPoint.Value))
			{
				loopInfo.isLooperBreak = () => true;
				checkSelectingTarget = target;
			}
		}
		private void OnCheckRectSelecting(OdccQueryLooper.LoopInfo loopInfo, SelectingTarget target)
		{
			if(target.OnCheckRectSelecting(checkRect.Value))
			{
				checkSelectingTargetList.Add(target);
			}
		}
		internal void DrawPoint(Vector2 startPoint, float minDragDelta, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			if(selectorDraw != null) selectorDraw.DrawPoint(startPoint, minDragDelta, includeFlag, excludeFlag);
		}
		internal void SelectPoint(Vector2 startPoint, float minDragDelta, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			if(selectorDraw != null) selectorDraw.DrawPoint(startPoint, minDragDelta, includeFlag, excludeFlag);

			GetSelectingHelperPoint(startPoint, minDragDelta);
			selectingTargets = selectorCommandListener.GetSelectList<SelectingTarget>();
			SelectPoint(checkSelectingTarget, includeFlag, excludeFlag);
			checkSelectingTarget = null;
		}
		public void SelectPoint(SelectingTarget target, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;

			OnHideAllTarget();
			if(target == null)
			{
				if(includeFlag == excludeFlag)
				{
					selectingTargets.Clear();
				}
			}
			else
			{
				if(includeFlag == excludeFlag)
				{
					selectingTargets.Clear();
					selectingTargets.Add(target);
				}
				else if(includeFlag)
				{
					selectingTargets.Add(target);
				}
				else if(excludeFlag)
				{
					selectingTargets.Remove(target);
				}
			}
			OnClearPreviewTarget();
			ShowSelectTarget();
		}
		internal void SelectRect(Vector2 startPoint, Vector2 endPoint, Vector2 size, float minDragDelta, float width, float height, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			if(!IsCanMultiSelect) return;
			if(selectorDraw != null) selectorDraw.DrawRect(startPoint, endPoint, size, minDragDelta, width, height, includeFlag, excludeFlag);

			GetSelectingHelperRect(startPoint, endPoint, minDragDelta);
			selectingTargets = selectorCommandListener.GetSelectList<SelectingTarget>();
			SelectRect(checkSelectingTargetList, includeFlag, excludeFlag);
		}
		public void SelectRect(HashSet<SelectingTarget> targets, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			if(!IsCanMultiSelect) return;

			OnHideAllTarget();
			if(targets == null || targets.Count == 0)
			{
				if(includeFlag == excludeFlag)
				{
					selectingTargets.Clear();
				}
			}
			else
			{
				if(includeFlag == excludeFlag)
				{
					selectingTargets.Clear();
					selectingTargets.AddRange(targets);
				}
				else if(includeFlag)
				{
					selectingTargets.AddRange(targets);
				}
				else if(excludeFlag)
				{
					int length = selectingTargets.Count;
					for(int i = 0 ; i < length ; i++)
					{
						if(targets.Contains(selectingTargets[i]))
						{
							selectingTargets.RemoveAt(i);
							i--;
							length--;
						}
					}
				}
			}
			OnClearPreviewTarget();
			ShowSelectTarget();
		}
		internal void PreviewSelectPoint(Vector2 startPoint, float minDragDelta, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			if(selectorDraw != null) selectorDraw.DrawPoint(startPoint, minDragDelta, includeFlag, excludeFlag);

			GetSelectingHelperPoint(startPoint, minDragDelta);
			var target = checkSelectingTarget;

			OnHideAllTarget();
			if(target == null)
			{
				previewSelectingTargets.Clear();
			}
			else
			{
				previewSelectingTargets.Clear();
				previewSelectingTargets.Add(target);
			}
			OnShowAllTarget();
		}
		internal void PreviewSelectRect(Vector2 startPoint, Vector2 endPoint, Vector2 size, float minDragDelta, float width, float height, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			if(!IsCanMultiSelect) return;
			if(selectorDraw != null) selectorDraw.DrawRect(startPoint, endPoint, size, minDragDelta, width, height, includeFlag, excludeFlag);


			GetSelectingHelperRect(startPoint, endPoint, minDragDelta);
			SelectingTarget[] targets = checkSelectingTargetList.ToArray();

			OnHideAllTarget();
			if(targets == null || targets.Length == 0)
			{
				previewSelectingTargets.Clear();
			}
			else
			{
				previewSelectingTargets.Clear();
				previewSelectingTargets.AddRange(targets);
			}
			OnShowAllTarget();
		}

		private void GetSelectingHelperPoint(Vector2 startPoint, float minDragDelta)
		{
			checkPoint = startPoint;
			checkSelectingTarget = null;
			checkSelectingTargetList.Clear();
			this.minDragDelta = minDragDelta;
			onCheckPointSelecting.RunAction();

			checkPoint = null;
			checkRect = null;
		}
		private void GetSelectingHelperRect(Vector2 startPoint, Vector2 endPoint, float minDragDelta)
		{
			Rect rect = new Rect(startPoint, Vector2.zero);
			rect.min = Vector2.Min(startPoint, endPoint);
			rect.max = Vector2.Max(startPoint, endPoint);

			checkRect = rect;
			checkSelectingTarget = null;
			checkSelectingTargetList.Clear();
			this.minDragDelta = minDragDelta;
			onCheckRectSelecting.RunAction();

			checkPoint = null;
			checkRect = null;
		}

		internal void OnShowAllTarget()
		{
			ShowSelectTarget();
			ShowPreviewTarget();
		}
		internal void ShowSelectTarget()
		{
			if(selectingTargets == null || selectingTargets.Count == 0) return;
			foreach(var item in selectingTargets)
			{
				item.ShowSelecting();
			}
		}
		internal void ShowPreviewTarget()
		{
			if(previewSelectingTargets == null || previewSelectingTargets.Count == 0) return;
			foreach(var item in previewSelectingTargets)
			{
				item.ShowPreviewSelecting();
			}
		}
		internal void OnHideAllTarget()
		{
			OnHideSelectingTarget();
			OnHidePreviewTarget();
		}
		internal void OnHideSelectingTarget()
		{
			if(selectingTargets == null || selectingTargets.Count == 0) return;
			foreach(var item in selectingTargets)
			{
				item.HideSelecting();
			}
		}
		internal void OnHidePreviewTarget()
		{
			if(previewSelectingTargets == null || previewSelectingTargets.Count == 0) return;
			foreach(var item in previewSelectingTargets)
			{
				item.HideSelecting();
			}
		}

		internal void OnClearAllTarget()
		{
			OnClearSelectingTarget();
			OnClearPreviewTarget();
		}
		internal void OnClearSelectingTarget()
		{
			OnHideSelectingTarget();
			selectingTargets.Clear();
		}
		internal void OnClearPreviewTarget()
		{
			OnHidePreviewTarget();
			previewSelectingTargets.Clear();
		}

		internal void OnNumKeys(int number)
		{
			// number = 키보드 입력값
			if(selectorCommandListener == null) return;

			if(number == 0) number = 10;
			OnClearAllTarget();

			selectorCommandListener.GetSelectTargetAndState<SelectingTarget>(number, out var target, out var changeToSelect);
			if(target != null)
			{
				selectingTargets = selectorCommandListener.GetSelectList<SelectingTarget>();
				SelectPoint(target, !changeToSelect, changeToSelect);
			}

			OnShowAllTarget();
		}
	}
}