using UnityEngine;

namespace TFContent
{
	public class ProjectileMoveTowardsData : ProjectileMoveData
	{
		public ProjectileMoveTowardsData() : base()
		{

		}

		public override Vector3 Forward => (targetPosition - startPosition).normalized;

		protected override void Disposing()
		{
			//	Vector3.MoveTowards
		}

	}
}