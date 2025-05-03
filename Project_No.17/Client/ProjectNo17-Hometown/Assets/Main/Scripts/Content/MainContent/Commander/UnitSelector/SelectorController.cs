using System.Collections.Generic;
using System.Linq;

using BC.ODCC;

using TFSystem.SaveLoad;

using UnityEngine;
namespace TFContent
{
	public partial class SelectorController : ComponentBehaviour
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
		private List<SelectingTarget> provisionalSelectingTargets;

		private SelectingTarget checkSingleSelectingTarget;
		private HashSet<SelectingTarget> checkMultiSelectingTarget;

		private SelectorDraw selectorDraw;

		protected override void BaseAwake()
		{
			selectingTargets = new List<SelectingTarget>();
			provisionalSelectingTargets = new List<SelectingTarget>();
			checkSingleSelectingTarget = null;
			checkMultiSelectingTarget = new HashSet<SelectingTarget>();

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

			checkSingleSelectingTarget = null;
			checkMultiSelectingTarget = null;

			selectingTargets = null;
			provisionalSelectingTargets = null;
		}
		private void OnCheckPointSelecting(OdccQueryLooper.LoopInfo loopInfo, SelectingTarget target)
		{
			if(target.OnCheckPointSelecting(checkPoint.Value))
			{
				loopInfo.isLooperBreak = () => true;
				checkSingleSelectingTarget = target;
			}
		}
		private void OnCheckRectSelecting(OdccQueryLooper.LoopInfo loopInfo, SelectingTarget target)
		{
			if(target.OnCheckRectSelecting(checkRect.Value))
			{
				checkMultiSelectingTarget.Add(target);
			}
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
	public partial class SelectorController// Show/Hide SelectTarget
	{
		internal void OnShowAllTarget()
		{
			ShowSelectTarget();
			ShowProvisionalTarget();
		}
		internal void ShowSelectTarget()
		{
			if(selectingTargets == null || selectingTargets.Count == 0) return;
			foreach(var item in selectingTargets)
			{
				item.ShowSelecting();
			}
		}
		internal void ShowProvisionalTarget()
		{
			if(provisionalSelectingTargets == null || provisionalSelectingTargets.Count == 0) return;
			foreach(SelectingTarget item in provisionalSelectingTargets)
			{
				item.ShowProvisionalSelecting();
			}
		}
		internal void OnHideAllTarget()
		{
			OnHideSelectingTarget();
			OnHideProvisionalTarget();
		}
		internal void OnHideSelectingTarget()
		{
			if(selectingTargets == null || selectingTargets.Count == 0) return;
			foreach(var item in selectingTargets)
			{
				item.HideSelecting();
			}
		}
		internal void OnHideProvisionalTarget()
		{
			if(provisionalSelectingTargets == null || provisionalSelectingTargets.Count == 0) return;
			foreach(var item in provisionalSelectingTargets)
			{
				item.HideSelecting();
			}
		}
		internal void OnClearAllTarget()
		{
			OnClearSelectingTarget();
			OnClearProvisionalTarget();
		}
		internal void OnClearSelectingTarget()
		{
			OnHideSelectingTarget();
			selectingTargets.Clear();
		}
		internal void OnClearProvisionalTarget()
		{
			OnHideProvisionalTarget();
			provisionalSelectingTargets.Clear();
		}
	}
	public partial class SelectorController : ISelectorController
	{
		internal void DrawPoint(Vector2 startPoint, float minDragDelta, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			if(selectorDraw != null) selectorDraw.DrawPoint(startPoint, minDragDelta, includeFlag, excludeFlag);
		}
		internal void DrawRect(Vector2 startPoint, Vector2 endPoint, Vector2 size, float minDragDelta, float width, float height, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			if(selectorDraw != null) selectorDraw.DrawRect(startPoint, endPoint, size, minDragDelta, width, height, includeFlag, excludeFlag);
		}

		internal void SelectPoint(Vector2 startPoint, float minDragDelta, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			DrawPoint(startPoint, minDragDelta, includeFlag, excludeFlag);

			GetSelectingHelperPoint(startPoint, minDragDelta);
			selectingTargets = selectorCommandListener.GetSelectList<SelectingTarget>();
			SelectPoint(checkSingleSelectingTarget, includeFlag, excludeFlag);
			checkSingleSelectingTarget = null;
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
			OnClearProvisionalTarget();
			ShowSelectTarget();
		}
		internal void SelectRect(Vector2 startPoint, Vector2 endPoint, Vector2 size, float minDragDelta, float width, float height, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			if(!IsCanMultiSelect) return;
			DrawRect(startPoint, endPoint, size, minDragDelta, width, height, includeFlag, excludeFlag);

			GetSelectingHelperRect(startPoint, endPoint, minDragDelta);
			selectingTargets = selectorCommandListener.GetSelectList<SelectingTarget>();
			SelectRect(checkMultiSelectingTarget, includeFlag, excludeFlag);
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
			OnClearProvisionalTarget();
			ShowSelectTarget();
		}
		internal void ProvisionalSelectPoint(Vector2 startPoint, float minDragDelta, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			DrawPoint(startPoint, minDragDelta, includeFlag, excludeFlag);

			GetSelectingHelperPoint(startPoint, minDragDelta);
			var target = checkSingleSelectingTarget;

			OnHideAllTarget();
			if(target == null)
			{
				provisionalSelectingTargets.Clear();
			}
			else
			{
				provisionalSelectingTargets.Clear();
				provisionalSelectingTargets.Add(target);
			}
			OnShowAllTarget();
		}
		internal void ProvisionalSelectRect(Vector2 startPoint, Vector2 endPoint, Vector2 size, float minDragDelta, float width, float height, bool includeFlag, bool excludeFlag)
		{
			if(selectorCommandListener == null) return;
			if(!IsCanMultiSelect) return;
			DrawRect(startPoint, endPoint, size, minDragDelta, width, height, includeFlag, excludeFlag);


			GetSelectingHelperRect(startPoint, endPoint, minDragDelta);
			SelectingTarget[] targets = checkMultiSelectingTarget.ToArray();

			OnHideAllTarget();
			if(targets == null || targets.Length == 0)
			{
				provisionalSelectingTargets.Clear();
			}
			else
			{
				provisionalSelectingTargets.Clear();
				provisionalSelectingTargets.AddRange(targets);
			}
			OnShowAllTarget();
		}

		private void GetSelectingHelperPoint(Vector2 startPoint, float minDragDelta)
		{
			checkPoint = startPoint;
			checkSingleSelectingTarget = null;
			checkMultiSelectingTarget.Clear();
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
			checkSingleSelectingTarget = null;
			checkMultiSelectingTarget.Clear();
			this.minDragDelta = minDragDelta;
			onCheckRectSelecting.RunAction();

			checkPoint = null;
			checkRect = null;
		}
	}
	public partial class SelectorController : ISaveLoadTarget
	{
		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
		}

		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			OnClearAllTarget();
			if(selectingTargets != null) selectingTargets.Clear();
			if(provisionalSelectingTargets != null) provisionalSelectingTargets.Clear();
			checkSingleSelectingTarget = null;
			if(checkMultiSelectingTarget != null) checkMultiSelectingTarget.Clear();
			if(selectingTargets == null) selectingTargets.Clear();
		}
	}
}