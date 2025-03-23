using UnityEngine;

namespace TFContent
{
	public abstract class ProjectileMoveData : ProjectileData
	{
		public Vector3 startPosition;
		public float moveSpeed = 30f;

		public Vector3 targetPosition;
		public virtual Vector3 Forward => (targetPosition - currentPosition).normalized;
		public Vector3 currentPosition {
			get {
				if(ThisContainer == null || ThisContainer.ThisObject == null || ThisContainer.ThisObject.ThisTransform == null) return startPosition;
				return ThisContainer.ThisObject.ThisTransform.position;
			}
		}
	}
}
