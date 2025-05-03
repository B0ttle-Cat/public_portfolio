using System.Collections.Generic;
using System.Linq;

using Shapes;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.Shape
{
	public class RectangleShape : BCShape
	{
		[Header("Pivot"), SerializeField]
		private AnchorType anchorType = AnchorType.PivotPos;

		public Vector3 Center = Vector3.zero;
		public Vector2 Size = new Vector2(3,5);
		public float Rotate = 0f;

		public float CornerRadius = 0f;

		[Header("DrawOption")]
		[SerializeField]
		private ShapeDraw drawStyle = ShapeDraw.Area;
		[SerializeField]
		private Color color = Color.red;
		[SerializeField, ShowIf("ShowAreaAndLine")]
		private Color subColor = Color.red;
		[ShowIf("ShowLine"), SerializeField]
		private float thickness = 0.1f;
		[ShowIf("ShowDashLine"), SerializeField]
		private DashStyle dashStyle = DashStyle.defaultDashStyle;
		private bool ShowLine => drawStyle != ShapeDraw.Area;
		private bool ShowDashLine => drawStyle == ShapeDraw.DashLine || drawStyle == ShapeDraw.AreaAndDashLine;
		private bool ShowAreaAndLine => drawStyle == ShapeDraw.AreaAndLine || drawStyle == ShapeDraw.AreaAndDashLine;
		public override void DrawShape(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate)
		{
			if(drawStyle == ShapeDraw.Hide) return;

			Draw.ResetStyle();

			Vector3 pos = AnchorPos(anchorType, pivot, target, minMaxRange) + Quaternion.Euler(0f,yAxisRotate , 0f) * Center;
			Quaternion rot = Quaternion.Euler(-90f,-90f + yAxisRotate + Rotate, 0f);
			Vector3 halfSize = Size * 0.5f;

			float cornerRadius = Mathf.Min(CornerRadius, halfSize.x, halfSize.y);
			switch(drawStyle)
			{
				case ShapeDraw.Area: DrawArea(color); break;
				case ShapeDraw.Line: DrawLine(color); break;
				case ShapeDraw.DashLine: DrawDashLine(color); break;
				case ShapeDraw.AreaAndLine: DrawArea(color); DrawLine(subColor); break;
				case ShapeDraw.AreaAndDashLine: DrawArea(color); DrawDashLine(subColor); break;
				default: break;
			}
			void DrawArea(Color color)
			{
				Draw.Rectangle(pos, rot, Size, cornerRadius, color);
			}
			void DrawLine(Color color)
			{
				Draw.RectangleBorder(pos, rot, Size, thickness, cornerRadius, color);
			}
			void DrawDashLine(Color color)
			{
				Draw.UseDashes = true;
				Draw.DashStyle = dashStyle;
				Draw.RectangleBorder(pos, rot, Size, thickness, cornerRadius, color);
			}
		}
		public override void OverlapCharacter(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate, Collider[] colliders, float yHeight = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
		{
			Vector3 pos = AnchorPos(anchorType, pivot, target, minMaxRange) + Quaternion.Euler(0,yAxisRotate, 0f) * Center;
			Vector3 halfSizeXZ = new Vector3(Size.x, 0f,Size.y) * 0.5f;
			Vector3 halfExtents = halfSizeXZ + Vector3.up * yHeight;
			Quaternion rot = Quaternion.Euler(0,yAxisRotate + Rotate, 0f);
			float cornerRadius = Mathf.Min(CornerRadius, halfExtents.x, halfExtents.z);

			if(cornerRadius <= float.Epsilon)
			{
				Physics.OverlapBoxNonAlloc(pos, halfExtents, colliders, rot, layerMask, trigger);
			}
			else
			{
				int limitCount = colliders.Length;
				HashSet<Collider> hashColliders = new HashSet<Collider>(limitCount);
				Vector3 halfExtents1 = halfExtents - Vector3.right * cornerRadius;
				Vector3 halfExtents2 = halfExtents - Vector3.forward * cornerRadius;
				if(cornerRadius < halfExtents.x && cornerRadius < halfExtents.z)
				{
					var colliders1 = Physics.OverlapBox(pos, halfExtents1, rot, layerMask, trigger);
					var colliders2 = Physics.OverlapBox(pos, halfExtents2, rot, layerMask, trigger);
					Vector3 posDir = halfSizeXZ;
					Vector3 pos1 = (pos + rot * posDir) + Vector3.up *   (yHeight - cornerRadius);
					Vector3 pos2 = (pos + rot * posDir) + Vector3.down * (yHeight + cornerRadius);
					var colliders3 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
					posDir.x = -halfSizeXZ.x;
					pos1 = (pos + rot * halfSizeXZ) + Vector3.up *   (yHeight - cornerRadius);
					pos2 = (pos + rot * halfSizeXZ) + Vector3.down * (yHeight + cornerRadius);
					var colliders4 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
					posDir.y = -halfSizeXZ.y;
					pos1 = (pos + rot * halfSizeXZ) + Vector3.up *   (yHeight - cornerRadius);
					pos2 = (pos + rot * halfSizeXZ) + Vector3.down * (yHeight + cornerRadius);
					var colliders5 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
					posDir.x = halfSizeXZ.x;
					pos1 = (pos + rot * halfSizeXZ) + Vector3.up *   (yHeight - cornerRadius);
					pos2 = (pos + rot * halfSizeXZ) + Vector3.down * (yHeight + cornerRadius);
					var colliders6 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);


					if(AddHashSet(hashColliders, colliders1, limitCount)) { }
					else if(AddHashSet(hashColliders, colliders2, limitCount)) { }
					else if(AddHashSet(hashColliders, colliders3, limitCount)) { }
					else if(AddHashSet(hashColliders, colliders4, limitCount)) { }
					else if(AddHashSet(hashColliders, colliders5, limitCount)) { }
					else if(AddHashSet(hashColliders, colliders6, limitCount)) { }
					HashToResultArray(hashColliders, colliders);
				}
				else if(cornerRadius < halfExtents.x)
				{
					var colliders1 = Physics.OverlapBox(pos, halfExtents1, rot, layerMask, trigger);
					Vector3 pos1 = (pos + rot * Vector3.right * halfExtents1.x) + Vector3.up *   (yHeight - cornerRadius);
					Vector3 pos2 = (pos + rot * Vector3.right * halfExtents1.x) + Vector3.down * (yHeight + cornerRadius);
					var colliders2 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
					pos1 = (pos + rot * Vector3.left * halfExtents1.x) + Vector3.up *   (yHeight - cornerRadius);
					pos2 = (pos + rot * Vector3.left * halfExtents1.x) + Vector3.down * (yHeight + cornerRadius);
					var colliders3 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);

					if(AddHashSet(hashColliders, colliders1, limitCount)) { }
					else if(AddHashSet(hashColliders, colliders2, limitCount)) { }
					else if(AddHashSet(hashColliders, colliders3, limitCount)) { }
					HashToResultArray(hashColliders, colliders);
				}
				else if(cornerRadius < halfExtents.z)
				{
					var colliders1 = Physics.OverlapBox(pos, halfExtents2, rot, layerMask, trigger);
					Vector3 pos1 = (pos + rot * Vector3.forward * halfExtents2.z) + Vector3.up *   (yHeight - cornerRadius);
					Vector3 pos2 = (pos + rot * Vector3.forward * halfExtents2.z) + Vector3.down * (yHeight + cornerRadius);
					var colliders2 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
					pos1 = (pos + rot * Vector3.back * halfExtents2.x) + Vector3.up *   (yHeight - cornerRadius);
					pos2 = (pos + rot * Vector3.back * halfExtents2.x) + Vector3.down * (yHeight + cornerRadius);
					var colliders3 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);

					if(AddHashSet(hashColliders, colliders1, limitCount)) { }
					else if(AddHashSet(hashColliders, colliders2, limitCount)) { }
					else if(AddHashSet(hashColliders, colliders3, limitCount)) { }
					HashToResultArray(hashColliders, colliders);
				}
				else
				{
					Vector3 pos1 = pos + Vector3.up *   (yHeight - cornerRadius);
					Vector3 pos2 = pos + Vector3.down * (yHeight + cornerRadius);
					Physics.OverlapCapsuleNonAlloc(pos1, pos2, cornerRadius, colliders, layerMask, trigger);
				}
			}
			bool AddHashSet(HashSet<Collider> hashColliders, Collider[] colliders, int limitCount)
			{
				int length = colliders.Length;
				for(int i = 0 ; i < length ; i++)
				{
					if(colliders[i] != null && hashColliders.Add(colliders[i]))
					{
						if(hashColliders.Count == limitCount)
						{
							return true;
						}
					}
				}
				return false;
			}
			void HashToResultArray(HashSet<Collider> hashColliders, Collider[] colliders)
			{
				int index = 0;
				foreach(var item in hashColliders)
				{
					colliders[index++] = item;
				}
			}
		}
		public override Collider[] OverlapCharacter(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate, float yHeight = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
		{
			Vector3 pos = AnchorPos(anchorType, pivot, target, minMaxRange) + Quaternion.Euler(0,yAxisRotate, 0f) * Center;
			Vector3 halfSizeXZ = new Vector3(Size.x, 0f,Size.y) * 0.5f;
			Vector3 halfExtents = halfSizeXZ + Vector3.up * yHeight;
			Quaternion rot = Quaternion.Euler(0,yAxisRotate + Rotate, 0f);
			float cornerRadius = Mathf.Min(CornerRadius, halfExtents.x, halfExtents.z);

			if(cornerRadius <= float.Epsilon)
			{
				return Physics.OverlapBox(pos, halfExtents, rot, layerMask, trigger);
			}
			else
			{
				HashSet<Collider> hashColliders = new HashSet<Collider>(10);
				Vector3 halfExtents1 = halfExtents - Vector3.right * cornerRadius;
				Vector3 halfExtents2 = halfExtents - Vector3.forward * cornerRadius;
				if(cornerRadius < halfExtents.x && cornerRadius < halfExtents.z)
				{
					var colliders1 = Physics.OverlapBox(pos, halfExtents1, rot, layerMask, trigger);
					var colliders2 = Physics.OverlapBox(pos, halfExtents2, rot, layerMask, trigger);
					Vector3 posDir = halfSizeXZ;
					Vector3 pos1 = (pos + rot * posDir) + Vector3.up *   (yHeight - cornerRadius);
					Vector3 pos2 = (pos + rot * posDir) + Vector3.down * (yHeight + cornerRadius);
					var colliders3 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
					posDir.x = -halfSizeXZ.x;
					pos1 = (pos + rot * halfSizeXZ) + Vector3.up *   (yHeight - cornerRadius);
					pos2 = (pos + rot * halfSizeXZ) + Vector3.down * (yHeight + cornerRadius);
					var colliders4 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
					posDir.y = -halfSizeXZ.y;
					pos1 = (pos + rot * halfSizeXZ) + Vector3.up *   (yHeight - cornerRadius);
					pos2 = (pos + rot * halfSizeXZ) + Vector3.down * (yHeight + cornerRadius);
					var colliders5 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
					posDir.x = halfSizeXZ.x;
					pos1 = (pos + rot * halfSizeXZ) + Vector3.up *   (yHeight - cornerRadius);
					pos2 = (pos + rot * halfSizeXZ) + Vector3.down * (yHeight + cornerRadius);
					var colliders6 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);


					AddHashSet(hashColliders, colliders1);
					AddHashSet(hashColliders, colliders2);
					AddHashSet(hashColliders, colliders3);
					AddHashSet(hashColliders, colliders4);
					AddHashSet(hashColliders, colliders5);
					AddHashSet(hashColliders, colliders6);
					return HashToResultArray(hashColliders);
				}
				else if(cornerRadius < halfExtents.x)
				{
					var colliders1 = Physics.OverlapBox(pos, halfExtents1, rot, layerMask, trigger);
					Vector3 pos1 = (pos + rot * Vector3.right * halfExtents1.x) + Vector3.up *   (yHeight - cornerRadius);
					Vector3 pos2 = (pos + rot * Vector3.right * halfExtents1.x) + Vector3.down * (yHeight + cornerRadius);
					var colliders2 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
					pos1 = (pos + rot * Vector3.left * halfExtents1.x) + Vector3.up *   (yHeight - cornerRadius);
					pos2 = (pos + rot * Vector3.left * halfExtents1.x) + Vector3.down * (yHeight + cornerRadius);
					var colliders3 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);

					AddHashSet(hashColliders, colliders1);
					AddHashSet(hashColliders, colliders2);
					AddHashSet(hashColliders, colliders3);
					return HashToResultArray(hashColliders);
				}
				else if(cornerRadius < halfExtents.z)
				{
					var colliders1 = Physics.OverlapBox(pos, halfExtents2, rot, layerMask, trigger);
					Vector3 pos1 = (pos + rot * Vector3.forward * halfExtents2.z) + Vector3.up *   (yHeight - cornerRadius);
					Vector3 pos2 = (pos + rot * Vector3.forward * halfExtents2.z) + Vector3.down * (yHeight + cornerRadius);
					var colliders2 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
					pos1 = (pos + rot * Vector3.back * halfExtents2.x) + Vector3.up *   (yHeight - cornerRadius);
					pos2 = (pos + rot * Vector3.back * halfExtents2.x) + Vector3.down * (yHeight + cornerRadius);
					var colliders3 = Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);

					AddHashSet(hashColliders, colliders1);
					AddHashSet(hashColliders, colliders2);
					AddHashSet(hashColliders, colliders3);
					return HashToResultArray(hashColliders);
				}
				else
				{
					Vector3 pos1 = pos + Vector3.up *   (yHeight - cornerRadius);
					Vector3 pos2 = pos + Vector3.down * (yHeight + cornerRadius);
					return Physics.OverlapCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
				}
			}
			void AddHashSet(HashSet<Collider> hashColliders, Collider[] colliders)
			{
				int length = colliders.Length;
				for(int i = 0 ; i < length ; i++)
				{
					if(colliders[i] != null)
					{
						hashColliders.Add(colliders[i]);
					}
				}
			}
			Collider[] HashToResultArray(HashSet<Collider> hashColliders)
			{
				return hashColliders.ToArray();
			}
		}
		public override bool CheckCharacter(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate, float yHeight = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
		{
			Vector3 pos = AnchorPos(anchorType, pivot, target, minMaxRange) + Quaternion.Euler(0,yAxisRotate, 0f) * Center;
			Vector3 halfSizeXZ = new Vector3(Size.x, 0f,Size.y) * 0.5f;
			Vector3 halfExtents = halfSizeXZ + Vector3.up * yHeight;
			Quaternion rot = Quaternion.Euler(0,yAxisRotate + Rotate, 0f);
			float cornerRadius = Mathf.Min(CornerRadius, halfExtents.x, halfExtents.z);

			if(cornerRadius <= float.Epsilon)
			{
				return Physics.CheckBox(pos, halfExtents, rot, layerMask, trigger);
			}

			HashSet<Collider> hashColliders = new HashSet<Collider>(10);
			Vector3 halfExtents1 = halfExtents - Vector3.right * cornerRadius;
			Vector3 halfExtents2 = halfExtents - Vector3.forward * cornerRadius;

			if(cornerRadius < halfExtents.x && cornerRadius < halfExtents.z)
			{
				if(Physics.CheckBox(pos, halfExtents1, rot, layerMask, trigger))
				{
					return true;
				}
				else if(Physics.CheckBox(pos, halfExtents2, rot, layerMask, trigger))
				{
					return true;
				}
				Vector3 posDir = halfSizeXZ;
				Vector3 pos1 = (pos + rot * posDir) + Vector3.up *   (yHeight - cornerRadius);
				Vector3 pos2 = (pos + rot * posDir) + Vector3.down * (yHeight + cornerRadius);
				if(Physics.CheckCapsule(pos1, pos2, cornerRadius, layerMask, trigger))
				{
					return true;
				}
				posDir.x = -halfSizeXZ.x;
				pos1 = (pos + rot * halfSizeXZ) + Vector3.up *   (yHeight - cornerRadius);
				pos2 = (pos + rot * halfSizeXZ) + Vector3.down * (yHeight + cornerRadius);
				if(Physics.CheckCapsule(pos1, pos2, cornerRadius, layerMask, trigger))
				{
					return true;
				}
				posDir.y = -halfSizeXZ.y;
				pos1 = (pos + rot * halfSizeXZ) + Vector3.up *   (yHeight - cornerRadius);
				pos2 = (pos + rot * halfSizeXZ) + Vector3.down * (yHeight + cornerRadius);
				if(Physics.CheckCapsule(pos1, pos2, cornerRadius, layerMask, trigger))
				{
					return true;
				}
				posDir.x = halfSizeXZ.x;
				pos1 = (pos + rot * halfSizeXZ) + Vector3.up *   (yHeight - cornerRadius);
				pos2 = (pos + rot * halfSizeXZ) + Vector3.down * (yHeight + cornerRadius);
				if(Physics.CheckCapsule(pos1, pos2, cornerRadius, layerMask, trigger))
				{
					return true;
				}
			}
			else if(cornerRadius < halfExtents.x)
			{
				if(Physics.CheckBox(pos, halfExtents1, rot, layerMask, trigger))
				{
					return true;
				}
				Vector3 pos1 = (pos + rot * Vector3.right * halfExtents1.x) + Vector3.up *   (yHeight - cornerRadius);
				Vector3 pos2 = (pos + rot * Vector3.right * halfExtents1.x) + Vector3.down * (yHeight + cornerRadius);
				if(Physics.CheckCapsule(pos1, pos2, cornerRadius, layerMask, trigger))
				{
					return true;
				}
				pos1 = (pos + rot * Vector3.left * halfExtents1.x) + Vector3.up *   (yHeight - cornerRadius);
				pos2 = (pos + rot * Vector3.left * halfExtents1.x) + Vector3.down * (yHeight + cornerRadius);
				if(Physics.CheckCapsule(pos1, pos2, cornerRadius, layerMask, trigger))
				{
					return true;
				}
			}
			else if(cornerRadius < halfExtents.z)
			{
				if(Physics.CheckBox(pos, halfExtents2, rot, layerMask, trigger))
				{
					return true;
				}
				Vector3 pos1 = (pos + rot * Vector3.forward * halfExtents2.z) + Vector3.up *   (yHeight - cornerRadius);
				Vector3 pos2 = (pos + rot * Vector3.forward * halfExtents2.z) + Vector3.down * (yHeight + cornerRadius);
				if(Physics.CheckCapsule(pos1, pos2, cornerRadius, layerMask, trigger))
				{
					return true;
				}
				pos1 = (pos + rot * Vector3.back * halfExtents2.x) + Vector3.up *   (yHeight - cornerRadius);
				pos2 = (pos + rot * Vector3.back * halfExtents2.x) + Vector3.down * (yHeight + cornerRadius);
				if(Physics.CheckCapsule(pos1, pos2, cornerRadius, layerMask, trigger))
				{
					return true;
				}
			}
			else
			{
				Vector3 pos1 = pos + Vector3.up *   (yHeight - cornerRadius);
				Vector3 pos2 = pos + Vector3.down * (yHeight + cornerRadius);
				return Physics.CheckCapsule(pos1, pos2, cornerRadius, layerMask, trigger);
			}
			return false;
		}
	}
}
