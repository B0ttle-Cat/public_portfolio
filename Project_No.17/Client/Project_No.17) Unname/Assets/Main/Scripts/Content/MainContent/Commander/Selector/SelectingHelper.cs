using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class SelectingHelper : ComponentBehaviour//, IOdccUpdate
	{
		[SerializeField]
		private SelectingTarget selectTarget;
		public SelectingTarget SelectTarget { get; private set; }

		public enum HelperRange
		{
			ManualSelect = 0,
			OnlyThisCollider,
			IncludeChildCollider,
		}
		[SerializeField]
		private HelperRange helperRange = HelperRange.ManualSelect;

		[SerializeField]
		private bool ignoreSelectPoint = false;
		[SerializeField]
		private bool ignoreSelectRect = false;

		[SerializeField]
		private Collider[] selectCollider;

		protected override void BaseEnable()
		{
			SelectTarget = selectTarget == null ? ThisContainer.GetComponent<SelectingTarget>() : selectTarget;

			if(helperRange != HelperRange.ManualSelect)
			{
				selectCollider = helperRange switch {
					HelperRange.OnlyThisCollider => selectCollider = gameObject.GetComponents<Collider>(),
					HelperRange.IncludeChildCollider => selectCollider = gameObject.GetComponentsInChildren<Collider>(true),
					_ => null
				};
			}

			int length = selectCollider.Length;
			for(int i = 0 ; i < length ; i++)
			{
				selectCollider[i].gameObject.layer = LayerMask.NameToLayer("SelectingHelper");
				selectCollider[i].isTrigger = true;
			}
		}
		protected override void BaseDisable()
		{
			if(helperRange != HelperRange.ManualSelect)
			{
				selectCollider = null;
			}
		}
		protected override void BaseDestroy()
		{
			selectTarget = null;
			selectCollider = null;
		}


		public bool OnCheckPointSelecting(Vector2 screenPoint, float minDragDelta, out SelectingTarget selectTarget)
		{
			selectTarget = SelectTarget;

			if(ignoreSelectPoint || selectTarget == null || !selectTarget.isActiveAndEnabled) return false;
			float halfMinDragDelta = minDragDelta * 0.5f;
			Ray screenPointRay = Camera.main.ScreenPointToRay(screenPoint);

			if(selectCollider == null || selectCollider.Length == 0) return false;
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
			selectTarget = SelectTarget;

			if(ignoreSelectRect || selectTarget == null || !selectTarget.isActiveAndEnabled) return false;

			Vector2 screenPoint = screenRect.center;
			Ray screenPointRay = Camera.main.ScreenPointToRay(screenPoint);

			if(selectCollider == null || selectCollider.Length == 0) return false;
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