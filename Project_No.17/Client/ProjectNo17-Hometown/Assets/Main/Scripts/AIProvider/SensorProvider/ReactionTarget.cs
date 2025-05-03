using BC.Base;
using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.AIProvider
{
	public class ReactionTarget : ComponentBehaviour, IReactionTarget //, IOdccUpdate
	{
		private CapsuleCollider selectCollider;
		[SerializeField]
		private LayerMask thisLayer;

		public CapsuleCollider SelectCollider {
			get {
				if(selectCollider == null) UpdateCollider();
				return selectCollider;
			}
		}
		[ShowInInspector, ReadOnly]
		public Vector3 Position => ReactionPosition();
		[ShowInInspector, ReadOnly]
		public Quaternion Rotation => ReactionRotation();
		[ShowInInspector, ReadOnly]
		public float Radius => ReactionRadius();
		public int Layer => thisLayer.ToLayer();

		protected override void BaseEnable()
		{
			UpdateCollider();
		}
		public void UpdateCollider()
		{
			selectCollider = GetComponent<CapsuleCollider>();
			if(selectCollider != null)
			{
				selectCollider.gameObject.layer = thisLayer.ToLayer();
				selectCollider.isTrigger = false;
			}
			else
			{
				gameObject.layer = thisLayer.ToLayer();
			}
		}

		public Vector3 ReactionPosition()
		{
			return SelectCollider == null ? ThisTransform.position : SelectCollider.bounds.center;
		}
		public Quaternion ReactionRotation()
		{
			return SelectCollider == null ? ThisTransform.rotation : SelectCollider.transform.rotation;
		}
		public float ReactionRadius()
		{
			return SelectCollider == null ? 0f : selectCollider.radius;
		}
		public float ReactionHeight(bool includeRadius = true)
		{
			return SelectCollider == null ? 0f :
				includeRadius ? selectCollider.height : Mathf.Max(0f, selectCollider.height - ReactionRadius() * 2f);
		}
		public Vector3 ClosestPoint(Vector3 checkPoint)
		{
			if(SelectCollider == null) return ThisTransform.position;
			else return SelectCollider.ClosestPoint(checkPoint);
		}

		public Unity.Physics.PhysicsCollider GetPhysicsCollider()
		{
			Unity.Physics.PhysicsCollider physicsCollider = new Unity.Physics.PhysicsCollider {
				Value = Unity.Physics.CapsuleCollider.Create(
					GetCapsuleGeometry(),
					GetCollisionFilter(),
					GetCollisionMaterial())
			};
			return physicsCollider;
		}

		public Unity.Physics.CapsuleGeometry GetCapsuleGeometry()
		{
			Vector3 colliderDirection = SelectCollider.direction switch {
				0=> Vector3.right,
				1=> Vector3.up,
				2=> Vector3.forward,
				_=> Vector3.up
			};
			Vector3 center = SelectCollider.center;
			colliderDirection *= ReactionHeight(false) * 0.5f;
			Unity.Physics.CapsuleGeometry capsuleGeometry = new Unity.Physics.CapsuleGeometry() {
				Radius = Radius,
				Vertex0 = center + colliderDirection,
				Vertex1 = center - colliderDirection,
			};
			return capsuleGeometry;
		}
		public Unity.Physics.CollisionFilter GetCollisionFilter()
		{
			Unity.Physics.CollisionFilter collisionFilter = Unity.Physics.CollisionFilter.Default;

			collisionFilter.BelongsTo = (uint)thisLayer.value;
			collisionFilter.CollidesWith = 0;
			return collisionFilter;
		}

		public Unity.Physics.Material GetCollisionMaterial()
		{
			Unity.Physics.Material collisionMaterial = Unity.Physics.Material.Default;
			collisionMaterial.CollisionResponse = Unity.Physics.CollisionResponsePolicy.Collide;
			return collisionMaterial;
		}
	}
}