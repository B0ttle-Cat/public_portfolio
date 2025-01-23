using System.Collections.Generic;
using System.Linq;

using BC.Base;
using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class SelectorController : ComponentBehaviour, ISelectorController
	{
		QuerySystem querySelectingHelper;
		OdccQueryCollector collectorSelectingHelper;
		OdccQueryLooper onCheckPointSelectingHelper;
		OdccQueryLooper onCheckRectSelectingHelper;

		private float minDragDelta;
		private Vector3? checkPoint;
		private Rect? checkRect;

		private List<SelectingTarget> selectingTargets;
		private List<SelectingTarget> previewSelectingTargets;

		private SelectingTarget checkSelectingTarget;
		private HashSet<SelectingTarget> checkSelectingTargetList;

		protected override void BaseAwake()
		{
			selectingTargets = new List<SelectingTarget>();
			previewSelectingTargets = new List<SelectingTarget>();
			checkSelectingTarget = null;
			checkSelectingTargetList = new HashSet<SelectingTarget>();

			querySelectingHelper = QuerySystemBuilder.CreateQuery()
				.WithAll<SelectingHelper>().Build(ThisScene);
			collectorSelectingHelper = OdccQueryCollector.CreateQueryCollector(querySelectingHelper, ThisScene)
				.CreateActionEvent(nameof(onCheckPointSelectingHelper), out onCheckPointSelectingHelper)
					.CallForeach<SelectingHelper>(OnCheckPointSelectingHelper).GetCollector()
				.CreateActionEvent(nameof(onCheckRectSelectingHelper), out onCheckRectSelectingHelper)
					.CallForeach<SelectingHelper>(OnCheckRectSelectingHelper).GetCollector();
		}
		protected override void BaseDestroy()
		{
			if(querySelectingHelper != null)
			{
				collectorSelectingHelper.DeleteActionEvent(nameof(onCheckPointSelectingHelper));
				collectorSelectingHelper.DeleteActionEvent(nameof(onCheckRectSelectingHelper));
				collectorSelectingHelper = null;
				onCheckRectSelectingHelper = null;
				onCheckPointSelectingHelper = null;
				querySelectingHelper = null;
			}

			checkPoint = null;
			checkRect = null;

			checkSelectingTarget = null;
			checkSelectingTargetList = null;

			selectingTargets = null;
			previewSelectingTargets = null;
		}

		private void OnCheckPointSelectingHelper(OdccQueryLooper.LoopInfo loopInfo, SelectingHelper helper)
		{
			if(helper.OnCheckPointSelecting(checkPoint.Value, minDragDelta, out SelectingTarget selectingTarget))
			{
				loopInfo.isLooperBreak = () => true;
				checkSelectingTarget = selectingTarget;
			}
		}
		private void OnCheckRectSelectingHelper(OdccQueryLooper.LoopInfo loopInfo, SelectingHelper helper)
		{
			if(helper.OnCheckRectSelecting(checkRect.Value, out SelectingTarget selectingTarget))
			{
				checkSelectingTargetList.Add(selectingTarget);
			}
		}

		internal void SelectPoint(Vector2 startPoint, float minDragDelta, bool includeFlag, bool excludeFlag)
		{
			GetSelectingHelperPoint(startPoint, minDragDelta);
			selectingTargets = EventManager.Call<ISelectorCommandListener, List<SelectingTarget>>(call => call.GetSelectList<SelectingTarget>());
			SelectPoint(checkSelectingTarget, includeFlag, excludeFlag);
			checkSelectingTarget = null;
			EventManager.Call<ISelectorCommandListener>(call => call.GetSelectListUpdate(selectingTargets));
		}
		public void SelectPoint(SelectingTarget target, bool includeFlag, bool excludeFlag)
		{
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
		internal void SelectRect(Vector2 startPoint, Vector2 endPoint, float minDragDelta, bool includeFlag, bool excludeFlag)
		{
			GetSelectingHelperRect(startPoint, endPoint, minDragDelta);
			List<SelectingTarget> selectingTargets = EventManager.Call<ISelectorCommandListener, List<SelectingTarget>>(call => call.GetSelectList<SelectingTarget>());
			SelectRect(checkSelectingTargetList, includeFlag, excludeFlag);
			EventManager.Call<ISelectorCommandListener, List<SelectingTarget>>(call => call.GetSelectList<SelectingTarget>());
		}
		public void SelectRect(HashSet<SelectingTarget> targets, bool includeFlag, bool excludeFlag)
		{
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
		internal void PreviewSelectPoint(Vector2 startPoint, float minDragDelta)
		{
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
		internal void PreviewSelectRect(Vector2 startPoint, Vector2 endPoint, float minDragDelta)
		{
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
			onCheckPointSelectingHelper.RunAction();

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
			onCheckRectSelectingHelper.RunAction();

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
			EventManager.Call<ISelectorCommandListener>(call => {
				OnClearAllTarget();

				call.GetSelectTargetAndState<SelectingTarget>(number, out var target, out var changeToSelect);
				if(target != null)
				{
					selectingTargets = call.GetSelectList<SelectingTarget>();
					SelectPoint(target, !changeToSelect, changeToSelect);
				}

				call.GetSelectListUpdate(selectingTargets);
				OnShowAllTarget();
			});
		}
		internal void OnInvertSelect()
		{
			EventManager.Call<ISelectorCommandListener>(call => {
				OnClearAllTarget();

				selectingTargets = call.GetUnselectList<SelectingTarget>();
				call.GetSelectListUpdate(selectingTargets);

				OnShowAllTarget();
			});
		}
	}
}