using BC.ODCC;
using BC.OdccBase;

using UnityEngine;
namespace BC.Projectile
{
	public class ProjectileMoveTrackingData : ProjectileMoveData
	{
		public ProjectileMoveTrackingData() : base()
		{

		}
		public Vector3 GetTrackingPosition()
		{
			return reactionTargetValue.Position;
		}
		public Vector3 GetTrackingPosition(ObjectBehaviour trackingObject)
		{
			if(trackingObject == null) return reactionTargetValue.Position;
			if(trackingObject.ThisContainer.TryGetComponent<IReactionTarget>(out var hitTarget))
			{
				return hitTarget.Position;
			}
			else
			{
				return hitTarget.ThisTransform.position;
			}
		}
	}
}