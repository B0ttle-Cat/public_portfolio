using BC.AIProvider;
using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class ProjectileMoveTrackingData : ProjectileMoveData
	{
		public ProjectileMoveTrackingData() : base()
		{

		}
		public ObjectBehaviour trackingObject;
		public Vector3 GetTrackingPosition()
		{
			return GetTrackingPosition(trackingObject);
		}
		public Vector3 GetTrackingPosition(ObjectBehaviour trackingObject)
		{
			if(trackingObject == null) return targetPosition;
			if(trackingObject.ThisContainer.TryGetComponent<ReactionTarget>(out var reactionTarget))
			{
				return reactionTarget.Position;
			}
			else
			{
				return reactionTarget.ThisTransform.position;
			}
		}
		protected override void Disposing()
		{
			trackingObject = null;
		}

	}
}