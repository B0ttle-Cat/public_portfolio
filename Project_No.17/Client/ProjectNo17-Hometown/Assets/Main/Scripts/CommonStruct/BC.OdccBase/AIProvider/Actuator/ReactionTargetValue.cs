using System;

using BC.ODCC;

using UnityEngine;
namespace BC.OdccBase
{
	public struct ReactionTargetInfo : IDisposable
	{
		private ObjectBehaviour TargetObject;
		private IReactionTarget TargetObjectReaction;
		private Vector3 TargetPosition;

		public ReactionTargetInfo(ObjectBehaviour targetObject)
		{
			TargetObject=targetObject;
			TargetObjectReaction = targetObject.ThisContainer.GetComponent<IReactionTarget>();
			TargetPosition = default;
			TargetPosition = Position;
		}
		public ReactionTargetInfo(Vector3 targetPosition)
		{
			TargetObject=null;
			TargetObjectReaction = null;
			TargetPosition=targetPosition;
		}
		public Vector3 Position {
			get {
				if(IsTracking)
				{
					if(TargetObjectReaction != null)
					{
						TargetPosition = TargetObjectReaction.Position;
					}
					else
					{
						TargetPosition = TargetObject.ThisTransform.position;
					}
				}
				return TargetPosition;
			}
		}
		public bool IsTracking => TargetObject != null;
		public void Dispose()
		{
			TargetObject = null;
			TargetObjectReaction = null;
		}
		public override string ToString()
		{
			if(TargetObject == null) return TargetPosition.ToString();
			else return TargetObject.GameObject.name;
		}
	}
}
