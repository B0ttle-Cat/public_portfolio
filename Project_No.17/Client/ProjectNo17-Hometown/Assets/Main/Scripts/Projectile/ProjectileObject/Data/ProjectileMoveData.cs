using BC.OdccBase;

using UnityEngine;

namespace BC.Projectile
{
	public abstract class ProjectileMoveData : ProjectileData
	{
		public Vector3 startPosition;
		public float moveSpeed = 30f;

		public ReactionTargetInfo reactionTargetValue;
		public virtual Vector3 Forward => (reactionTargetValue.Position - currentPosition).normalized;
		public Vector3 currentPosition {
			get {
				if(ThisContainer == null || ThisContainer.ThisObject == null || ThisContainer.ThisObject.ThisTransform == null) return startPosition;
				return ThisContainer.ThisObject.ThisTransform.position;
			}
		}

		protected override void Disposing()
		{
			reactionTargetValue.Dispose();
		}
	}
}
