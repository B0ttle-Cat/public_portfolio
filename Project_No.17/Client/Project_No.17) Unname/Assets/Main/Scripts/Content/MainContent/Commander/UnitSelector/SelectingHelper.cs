using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class SelectingHelper : ComponentBehaviour//, IOdccUpdate
	{
		private SelectingTarget selectTarget;

		[SerializeField]
		private bool ignoreSelectPoint = false;
		[SerializeField]
		private bool ignoreSelectRect = false;

		private Collider[] selectCollider;

		protected override void BaseAwake()
		{
			selectTarget = selectTarget == null ? ThisContainer.GetComponent<SelectingTarget>() : selectTarget;
		}

		protected override void BaseEnable()
		{
			selectCollider = GetComponentsInChildren<Collider>();
			int length = selectCollider.Length;
			for(int i = 0 ; i < length ; i++)
			{
				selectCollider[i].gameObject.layer = LayerMask.NameToLayer("SelectingTarget");
				selectCollider[i].isTrigger = true;
			}
		}
		protected override void BaseDisable()
		{
			selectCollider = null;
		}
		protected override void BaseDestroy()
		{
			selectTarget = null;
			selectCollider = null;
		}


		public bool OnCheckPointSelecting(Vector2 screenPoint, float minDragDelta, out SelectingTarget selectTarget)
		{
			selectTarget = this.selectTarget;

			if(ignoreSelectPoint || selectTarget == null || !selectTarget.isActiveAndEnabled) return false;
			if(selectCollider == null || selectCollider.Length == 0) return false;

			float halfMinDragDelta = minDragDelta * 0.5f;
			Ray screenPointRay = Camera.main.ScreenPointToRay(screenPoint);

			foreach(Collider collider in selectCollider)
			{
				if(collider == null) continue;
				Vector3 colliderCenter = collider.bounds.center;
				Vector3 worldPosition = colliderCenter;

				Plane plane = new Plane(-screenPointRay.direction, colliderCenter);
				if(!plane.Raycast(screenPointRay, out float distance))
				{
					continue;
				}
				worldPosition = screenPointRay.origin + screenPointRay.direction * distance;

				Vector3 closestPoint = plane.ClosestPointOnPlane(collider.ClosestPoint(worldPosition));

				if(Vector3.Distance(closestPoint, worldPosition) < float.Epsilon)
				{
					return true;
				}
				else
				{
					Vector2 colliderScreenPoint = Camera.main.WorldToScreenPoint(closestPoint);
					if(Vector2.Distance(colliderScreenPoint, screenPoint) < halfMinDragDelta)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool OnCheckRectSelecting(Rect screenRect, out SelectingTarget selectTarget)
		{
			selectTarget = this.selectTarget;

			if(ignoreSelectRect || selectTarget == null || !selectTarget.isActiveAndEnabled) return false;
			if(selectCollider == null || selectCollider.Length == 0) return false;

			Vector2 screenPoint = screenRect.center;
			Ray screenPointRay = Camera.main.ScreenPointToRay(screenPoint);

			foreach(Collider collider in selectCollider)
			{
				if(collider == null) continue;
				Vector3 colliderCenter = collider.bounds.center;
				Vector3 worldPosition = colliderCenter;

				Plane plane = new Plane(-screenPointRay.direction, colliderCenter);
				if(!plane.Raycast(screenPointRay, out float distance))
				{
					continue;
				}
				worldPosition = screenPointRay.origin + screenPointRay.direction * distance;

				Vector3 closestPoint = collider.ClosestPoint(worldPosition);
				Vector2 colliderScreenPoint = Camera.main.WorldToScreenPoint(closestPoint);
				if(screenRect.Contains(colliderScreenPoint))
				{
					return true;
				}
			}
			return false;
		}
	}
}