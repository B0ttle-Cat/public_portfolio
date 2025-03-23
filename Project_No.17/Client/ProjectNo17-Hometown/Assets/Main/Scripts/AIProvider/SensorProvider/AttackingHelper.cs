//using BC.ODCC;

//using UnityEngine;
//namespace BC.AIProvider
//{
//	public class AttackingHelper : ComponentBehaviour//, IOdccUpdate
//	{
//		private Collider selectCollider;

//		public Collider SelectCollider {
//			get {
//				if(selectCollider == null) UpdateCollider();
//				return selectCollider;
//			}
//		}

//		protected override void BaseEnable()
//		{
//			UpdateCollider();
//		}
//		public void UpdateCollider()
//		{
//			selectCollider = GetComponent<Collider>();
//			if(selectCollider != null)
//			{
//				selectCollider.gameObject.layer = LayerMask.NameToLayer("AttackingTarget");
//				selectCollider.isTrigger = false;
//			}
//		}
//		protected override void BaseDisable()
//		{
//			selectCollider = null;
//		}

//		internal Vector3 ClosestPoint(Vector3 checkPoint)
//		{
//			var collider = SelectCollider;
//			if(collider  == null) return ThisTransform.position;
//			return collider.ClosestPoint(checkPoint);
//		}
//	}
//}