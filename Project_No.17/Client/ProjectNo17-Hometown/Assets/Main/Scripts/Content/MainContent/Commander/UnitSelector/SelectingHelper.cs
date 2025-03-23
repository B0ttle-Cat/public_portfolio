using BC.Base;
using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class SelectingHelper : ComponentBehaviour//, IOdccUpdate
	{

		[SerializeField]
		private bool ignoreSelectPoint = false;
		[SerializeField]
		private bool ignoreSelectRect = false;

		private CapsuleCollider selectCollider;

		protected override void BaseEnable()
		{
			selectCollider = GetComponentInChildren<CapsuleCollider>(true);
			if(selectCollider != null)
			{
				selectCollider.gameObject.layer = LayerMask.NameToLayer("SelectingTarget");
				selectCollider.isTrigger = true;
			}
		}
		protected override void BaseDisable()
		{
			selectCollider = null;
		}
		protected override void BaseDestroy()
		{
			selectCollider = null;
		}

		public bool OnCheckPointSelecting(in Vector2 screenPoint)
		{
			if(ignoreSelectPoint || selectCollider == null) return false;

			var mainCamera = Camera.main;

			var bounds = selectCollider.bounds;
			var radius = bounds.extents.x;
			var center = bounds.center;
			var centerTop = center;
			var centerBot = center;
			var centerRight = center;
			centerTop.y = bounds.max.y - radius;
			centerBot.y = bounds.min.y + radius;
			centerRight += (mainCamera.transform.right * radius);

			center = mainCamera.WorldToScreenPoint(center);
			if(center.z < mainCamera.nearClipPlane) return false;
			centerTop = mainCamera.WorldToScreenPoint(centerTop);
			centerBot = mainCamera.WorldToScreenPoint(centerBot);
			centerRight = mainCamera.WorldToScreenPoint(centerRight);
			radius = Mathf.Abs(center.x - centerRight.x);

			Vector2 centerTopXY = centerTop.XY();
			Vector2 centerBotXY = centerBot.XY();

			float distance = Vector2.Distance(centerTopXY, screenPoint);
			if(distance <= radius) return true;
			distance = Vector2.Distance(centerBotXY, screenPoint);
			if(distance <= radius) return true;

			Vector2 dirBT = centerTopXY - centerBotXY;
			Vector2 dirBS = screenPoint - centerBotXY;
			Vector2 dirTB = -dirBT;
			Vector2 dirTS = screenPoint - centerTopXY;
			float dotBTBS = Vector2.Dot(dirBT, dirBS);
			if(dotBTBS < 0) return false;

			float dotTBTS = Vector2.Dot(dirTB, dirTS);
			if(dotTBTS < 0) return false;

			Vector2 projectionPoint = centerBotXY + dirBT.normalized * dotBTBS;
			distance = Vector2.Distance(projectionPoint, screenPoint);
			if(dotTBTS < 0) return false;

			return false;
		}
		public bool OnCheckRectSelecting(in Rect screenRect)
		{
			if(ignoreSelectPoint || selectCollider == null) return false;

			var mainCamera = Camera.main;

			var bounds = selectCollider.bounds;
			var center = bounds.center;
			var centerTop = center;
			var centerBot = center;
			centerTop.y = bounds.max.y;
			centerBot.y = bounds.min.y;

			center = mainCamera.WorldToScreenPoint(center);
			if(center.z < mainCamera.nearClipPlane) return false;
			centerTop = mainCamera.WorldToScreenPoint(centerTop);
			centerBot = mainCamera.WorldToScreenPoint(centerBot);

			Vector2 centerTopXY = centerTop.XY();
			Vector2 centerBotXY = centerBot.XY();
			if(screenRect.Contains(centerTopXY)) return true;
			if(screenRect.Contains(centerBotXY)) return true;

			Vector2 LB = screenRect.min;
			Vector2 LT = new Vector2(screenRect.xMin,screenRect.yMax);
			Vector2 RT = screenRect.max;
			Vector2 RB = new Vector2(screenRect.xMax,screenRect.yMin);

			if(CrossLine_AB_CD(centerTopXY, centerBotXY, LB, LT)) return true;
			if(CrossLine_AB_CD(centerTopXY, centerBotXY, LT, RT)) return true;
			if(CrossLine_AB_CD(centerTopXY, centerBotXY, RT, RB)) return true;
			if(CrossLine_AB_CD(centerTopXY, centerBotXY, RB, LB)) return true;
			bool CrossLine_AB_CD(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
			{
				int ab_c = CCW(A, B, C);
				int ab_d = CCW(A, B, D);
				int cd_a = CCW(C, D, A);
				int cd_b = CCW(C, D, B);

				// 선분이 서로 다른 방향으로 CCW 값을 가질 때 교차함
				if(ab_c * ab_d < 0 && cd_a * cd_b < 0)
					return true;

				// **특수한 경우 (선분이 일직선상에 존재할 때)**
				if(ab_c == 0 && IsPointOnSegment(A, B, C)) return true;
				if(ab_d == 0 && IsPointOnSegment(A, B, D)) return true;
				if(cd_a == 0 && IsPointOnSegment(C, D, A)) return true;
				if(cd_b == 0 && IsPointOnSegment(C, D, B)) return true;

				return false;

				int CCW(Vector2 p1, Vector2 p2, Vector2 p3)
				{
					float crossProduct = (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);

					if(crossProduct > 0) return 1;  // 반시계 방향
					if(crossProduct < 0) return -1; // 시계 방향
					return 0; // 일직선
				}

				bool IsPointOnSegment(Vector2 A, Vector2 B, Vector2 P)
				{
					return (P.x >= Mathf.Min(A.x, B.x) && P.x <= Mathf.Max(A.x, B.x) &&
							P.y >= Mathf.Min(A.y, B.y) && P.y <= Mathf.Max(A.y, B.y));
				}
			}

			return false;
		}

		//public bool OnCheckPointSelecting(Vector2 screenPoint, float minDragDelta, out SelectingTarget selectTarget)
		//{
		//	selectTarget = this.selectTarget;

		//	if(ignoreSelectPoint || selectTarget == null || !selectTarget.isActiveAndEnabled) return false;
		//	if(selectCollider == null) return false;

		//	float halfMinDragDelta = minDragDelta * 0.5f;
		//	Ray screenPointRay = Camera.main.ScreenPointToRay(screenPoint);

		//	Vector3 colliderCenter = selectCollider.bounds.center;
		//	Vector3 worldPosition = colliderCenter;

		//	Plane plane = new Plane(-screenPointRay.direction, colliderCenter);
		//	if(!plane.Raycast(screenPointRay, out float distance))
		//	{
		//		return false;
		//	}
		//	worldPosition = screenPointRay.origin + screenPointRay.direction * distance;

		//	Vector3 closestPoint = plane.ClosestPointOnPlane(selectCollider.ClosestPoint(worldPosition));

		//	if(Vector3.Distance(closestPoint, worldPosition) < float.Epsilon)
		//	{
		//		return true;
		//	}
		//	else
		//	{
		//		Vector2 colliderScreenPoint = Camera.main.WorldToScreenPoint(closestPoint);
		//		if(Vector2.Distance(colliderScreenPoint, screenPoint) < halfMinDragDelta)
		//		{
		//			return true;
		//		}
		//	}
		//	return false;
		//}

		//public bool OnCheckRectSelecting(Rect screenRect, out SelectingTarget selectTarget)
		//{
		//	selectTarget = this.selectTarget;

		//	if(ignoreSelectRect || selectTarget == null || !selectTarget.isActiveAndEnabled) return false;
		//	if(selectCollider == null) return false;

		//	Vector2 screenPoint = screenRect.center;
		//	Ray screenPointRay = Camera.main.ScreenPointToRay(screenPoint);

		//	Vector3 colliderCenter = selectCollider.bounds.center;
		//	Vector3 worldPosition = colliderCenter;

		//	Plane plane = new Plane(-screenPointRay.direction, colliderCenter);
		//	if(!plane.Raycast(screenPointRay, out float distance))
		//	{
		//		return false;
		//	}
		//	worldPosition = screenPointRay.origin + screenPointRay.direction * distance;

		//	Vector3 closestPoint = selectCollider.ClosestPoint(worldPosition);
		//	Vector2 colliderScreenPoint = Camera.main.WorldToScreenPoint(closestPoint);
		//	if(screenRect.Contains(colliderScreenPoint))
		//	{
		//		return true;
		//	}
		//	return false;
		//}
	}
}