using Shapes;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.Shape
{
	public class CircleShape : BCShape
	{
		[Header("Pivot"), SerializeField]
		private AnchorType anchorType = AnchorType.PivotPos;
		private enum RangeType
		{
			CenterBase, MinMaxRange,
		}
		[SerializeField]
		private RangeType rangeType = RangeType.CenterBase;
		[Header("Shape")]
		public Vector3 Center = Vector3.zero;
		public Vector2 Radius = new Vector2(0f,5f);
		public float startAngle = 0f;
		public float endedAngle = 360f;

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
			Quaternion rot = Quaternion.Euler(0, yAxisRotate, 0f);
			Vector3 pos = AnchorPos(anchorType, pivot, target, minMaxRange) + rot * Center;
			rot *= Quaternion.Euler(-90f, -90f, 0f);

			GetRangeType(rangeType, in Radius, in minMaxRange, out float minRadius, out float maxRadius);

			float angleRadStart = startAngle * Mathf.Deg2Rad;
			float angleRadEnd = endedAngle * Mathf.Deg2Rad;

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
				float thickness = maxRadius - minRadius;
				float radius = minRadius + thickness * 0.5f;
				Draw.Arc(pos, rot, radius, thickness, angleRadStart, angleRadEnd, color);
			}
			void DrawLine(Color color)
			{
				if(minRadius >= float.Epsilon)
				{
					Draw.Arc(pos, rot, minRadius, thickness, angleRadStart, angleRadEnd, color);
				}
				Draw.Arc(pos, rot, maxRadius, thickness, angleRadStart, angleRadEnd, color);
				bool drawSideLine = Mathf.Abs(startAngle - endedAngle) < 360f;
				if(drawSideLine)
				{
					Vector3 startAngleDir = Quaternion.Euler(0f,yAxisRotate + startAngle,0f) * Vector3.forward;
					Vector3 endedAngleDir = Quaternion.Euler(0f,yAxisRotate + endedAngle,0f) * Vector3.forward;

					Draw.Line(pos + startAngleDir * minRadius, pos + startAngleDir * maxRadius, thickness, color);
					Draw.Line(pos + endedAngleDir * minRadius, pos + endedAngleDir * maxRadius, thickness, color);
				}
			}
			void DrawDashLine(Color color)
			{
				Draw.UseDashes = true;
				Draw.DashStyle = dashStyle;
				if(minRadius >= float.Epsilon)
				{
					Draw.Arc(pos, rot, minRadius, thickness, angleRadStart, angleRadEnd, color);
				}
				Draw.Arc(pos, rot, maxRadius, thickness, angleRadStart, angleRadEnd, color);
				bool drawSideLine = Mathf.Abs(startAngle - endedAngle) < 360f;
				if(drawSideLine)
				{
					Vector3 startAngleDir = Quaternion.Euler(0f,yAxisRotate + startAngle,0f) * Vector3.forward;
					Vector3 endedAngleDir = Quaternion.Euler(0f,yAxisRotate + endedAngle,0f) * Vector3.forward;

					Draw.Line(pos + startAngleDir * minRadius, pos + startAngleDir * maxRadius, thickness, color);
					Draw.Line(pos + endedAngleDir * minRadius, pos + endedAngleDir * maxRadius, thickness, color);
				}
			}
		}
		public override void OverlapCharacter(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate, Collider[] colliders, float yHeight = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
		{
			Quaternion rot = Quaternion.Euler(0, yAxisRotate, 0f);
			Vector3 pos = AnchorPos(anchorType, pivot, target, minMaxRange) + rot * Center;

			GetRangeType(rangeType, in Radius, in minMaxRange, out float minRadius, out float maxRadius);

			float minAngle = yAxisRotate + startAngle;
			float maxAngle = yAxisRotate + endedAngle;
			Vector3 minAngleDir = Quaternion.Euler(0f,minAngle,0f) * Vector3.forward;
			Vector3 maxAngleDir = Quaternion.Euler(0f,maxAngle,0f) * Vector3.forward;


			Vector3 point1 = pos + Vector3.down * yHeight;
			Vector3 point2 = pos + Vector3.up * yHeight;

			Physics.OverlapCapsuleNonAlloc(point1, point2, maxRadius, colliders, layerMask, trigger);
			int length = colliders.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var collider = colliders[i];
				if(collider == null) continue;
				if(collider is SphereCollider sphere)
				{
					Vector3 colliderCentner = collider.bounds.center;
					float colliderRadius = collider.bounds.extents.x;
					Vector3 direction = colliderCentner - pos;
					float magnitude = direction.magnitude;
					if(magnitude + colliderRadius < minRadius)
					{
						colliders[i] = null;
						continue;
					}
				}
				else if(collider is CapsuleCollider capsule && capsule.direction == 1)
				{
					Vector3 colliderCentner = collider.bounds.center;
					float colliderRadius = collider.bounds.extents.x;
					Vector3 direction = colliderCentner - pos;
					float magnitude = direction.magnitude;
					if(magnitude + colliderRadius < minRadius)
					{
						colliders[i] = null;
						continue;
					}
				}
				else
				{
					colliders[i] = null;
				}
			}
			for(int i = 0 ; i < length ; i++)
			{
				var collider = colliders[i];
				if(collider == null) continue;
				if((collider is SphereCollider sphere) || (collider is CapsuleCollider capsule && capsule.direction == 1))
				{
					Vector3 colliderCentner = collider.bounds.center;
					float colliderRadius = collider.bounds.extents.x;
					Vector3 direction = colliderCentner - pos;
					float magnitude = direction.magnitude;
					if(magnitude < float.Epsilon)
					{
						continue;
					}
					float colliderAngle = Vector3.Angle(Vector3.forward, direction.normalized);
					colliderAngle = colliderAngle >= 0 ? colliderAngle : 360f + colliderAngle;
					if(IsAngleBetween(colliderAngle, minAngle, maxAngle))
					{
						continue;
					}

					float minDot = Vector3.Dot(minAngleDir, direction);
					if(minDot>0)
					{
						Vector3 linePos = minAngleDir * minDot;
						if(Vector3.Distance(linePos, colliderCentner) <= colliderRadius)
						{
							continue;
						}
					}
					float maxDot = Vector3.Dot(maxAngleDir, direction);
					if(maxDot>0)
					{
						Vector3 linePos = maxAngleDir * minDot;
						if(Vector3.Distance(linePos, colliderCentner) <= colliderRadius)
						{
							continue;
						}
					}

					colliders[i] = null;
				}
				else
				{
					colliders[i] = null;
				}
			}
		}
		public override Collider[] OverlapCharacter(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate, float yHeight = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
		{
			Quaternion rot = Quaternion.Euler(0, yAxisRotate, 0f);
			Vector3 pos = AnchorPos(anchorType, pivot, target, minMaxRange) + rot * Center;

			GetRangeType(rangeType, in Radius, in minMaxRange, out float minRadius, out float maxRadius);

			float minAngle = yAxisRotate + startAngle;
			float maxAngle = yAxisRotate + endedAngle;
			Vector3 minAngleDir = Quaternion.Euler(0f,minAngle,0f) * Vector3.forward;
			Vector3 maxAngleDir = Quaternion.Euler(0f,maxAngle,0f) * Vector3.forward;


			Vector3 point1 = pos + Vector3.down * yHeight;
			Vector3 point2 = pos + Vector3.up * yHeight;

			Collider[] colliders = Physics.OverlapCapsule(point1, point2, maxRadius, layerMask, trigger);
			int length = colliders.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var collider = colliders[i];
				if(collider == null) continue;
				if(collider is SphereCollider sphere)
				{
					Vector3 colliderCentner = collider.bounds.center;
					float colliderRadius = collider.bounds.extents.x;
					Vector3 direction = colliderCentner - pos;
					float magnitude = direction.magnitude;
					if(magnitude + colliderRadius < minRadius)
					{
						colliders[i] = null;
						continue;
					}
				}
				else if(collider is CapsuleCollider capsule && capsule.direction == 1)
				{
					Vector3 colliderCentner = collider.bounds.center;
					float colliderRadius = collider.bounds.extents.x;
					Vector3 direction = colliderCentner - pos;
					float magnitude = direction.magnitude;
					if(magnitude + colliderRadius < minRadius)
					{
						colliders[i] = null;
						continue;
					}
				}
				else
				{
					colliders[i] = null;
				}
			}
			for(int i = 0 ; i < length ; i++)
			{
				var collider = colliders[i];
				if(collider == null) continue;
				if((collider is SphereCollider sphere) || (collider is CapsuleCollider capsule && capsule.direction == 1))
				{
					Vector3 colliderCentner = collider.bounds.center;
					float colliderRadius = collider.bounds.extents.x;
					Vector3 direction = colliderCentner - pos;
					float magnitude = direction.magnitude;
					if(magnitude < float.Epsilon)
					{
						continue;
					}
					float colliderAngle = Vector3.Angle(Vector3.forward, direction.normalized);
					colliderAngle = colliderAngle >= 0 ? colliderAngle : 360f + colliderAngle;
					if(IsAngleBetween(colliderAngle, minAngle, maxAngle))
					{
						continue;
					}

					float minDot = Vector3.Dot(minAngleDir, direction);
					if(minDot>0)
					{
						Vector3 linePos = minAngleDir * minDot;
						if(Vector3.Distance(linePos, colliderCentner) <= colliderRadius)
						{
							continue;
						}
					}
					float maxDot = Vector3.Dot(maxAngleDir, direction);
					if(maxDot>0)
					{
						Vector3 linePos = maxAngleDir * minDot;
						if(Vector3.Distance(linePos, colliderCentner) <= colliderRadius)
						{
							continue;
						}
					}

					colliders[i] = null;
				}
				else
				{
					colliders[i] = null;
				}
			}
			return colliders;
		}
		private bool IsAngleBetween(float target, float startAngle, float endAngle)
		{
			if(startAngle <= endAngle)
			{
				return target >= startAngle && target <= endAngle;
			}
			else
			{
				// 360도를 넘어가는 경우 처리
				return target >= startAngle || target <= endAngle;
			}
		}
		private void GetRangeType(RangeType rangeType, in Vector2 Radius, in Vector2 minMaxRange, out float minRadius, out float maxRadius)
		{
			float radiusX = 0f;
			float radiusY = 0f;

			if(rangeType == RangeType.MinMaxRange)
			{
				radiusX = Mathf.Abs(Radius.x + minMaxRange.x);
				radiusY = Mathf.Abs(Radius.y + minMaxRange.y);
			}
			else
			{
				radiusX = Mathf.Abs(Radius.x);
				radiusY = Mathf.Abs(Radius.y);
			}

			minRadius = Mathf.Min(radiusX, radiusY);
			maxRadius = Mathf.Max(radiusX, radiusY);

			if(minRadius<0f) minRadius = 0f;
			if(maxRadius<minRadius) maxRadius = minRadius;
		}

		public override bool CheckCharacter(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate, float yHeight = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
		{
			Quaternion rot = Quaternion.Euler(0, yAxisRotate, 0f);
			Vector3 pos = AnchorPos(anchorType, pivot, target, minMaxRange) + rot * Center;

			GetRangeType(rangeType, in Radius, in minMaxRange, out float minRadius, out float maxRadius);

			float minAngle = yAxisRotate + startAngle;
			float maxAngle = yAxisRotate + endedAngle;
			Vector3 minAngleDir = Quaternion.Euler(0f,minAngle,0f) * Vector3.forward;
			Vector3 maxAngleDir = Quaternion.Euler(0f,maxAngle,0f) * Vector3.forward;


			Vector3 point1 = pos + Vector3.down * yHeight;
			Vector3 point2 = pos + Vector3.up * yHeight;

			Collider[] colliders = Physics.OverlapCapsule(point1, point2, maxRadius, layerMask, trigger);
			int length = colliders.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var collider = colliders[i];
				if(collider == null) continue;
				if(collider is SphereCollider sphere)
				{
					Vector3 colliderCentner = collider.bounds.center;
					float colliderRadius = collider.bounds.extents.x;
					Vector3 direction = colliderCentner - pos;
					float magnitude = direction.magnitude;
					if(magnitude + colliderRadius < minRadius)
					{
						colliders[i] = null;
						continue;
					}
					else
					{
						return true;
					}
				}
				else if(collider is CapsuleCollider capsule && capsule.direction == 1)
				{
					Vector3 colliderCentner = collider.bounds.center;
					float colliderRadius = collider.bounds.extents.x;
					Vector3 direction = colliderCentner - pos;
					float magnitude = direction.magnitude;
					if(magnitude + colliderRadius < minRadius)
					{
						colliders[i] = null;
						continue;
					}
					else
					{
						return true;
					}
				}
				else
				{
					colliders[i] = null;
				}
			}
			for(int i = 0 ; i < length ; i++)
			{
				var collider = colliders[i];
				if(collider == null) continue;
				if((collider is SphereCollider sphere) || (collider is CapsuleCollider capsule && capsule.direction == 1))
				{
					Vector3 colliderCentner = collider.bounds.center;
					float colliderRadius = collider.bounds.extents.x;
					Vector3 direction = colliderCentner - pos;
					float magnitude = direction.magnitude;
					if(magnitude < float.Epsilon)
					{
						return true;
					}
					float colliderAngle = Vector3.Angle(Vector3.forward, direction.normalized);
					colliderAngle = colliderAngle >= 0 ? colliderAngle : 360f + colliderAngle;
					if(IsAngleBetween(colliderAngle, minAngle, maxAngle))
					{
						return true;
					}

					float minDot = Vector3.Dot(minAngleDir, direction);
					if(minDot>0)
					{
						Vector3 linePos = minAngleDir * minDot;
						if(Vector3.Distance(linePos, colliderCentner) <= colliderRadius)
						{
							return true;
						}
					}
					float maxDot = Vector3.Dot(maxAngleDir, direction);
					if(maxDot>0)
					{
						Vector3 linePos = maxAngleDir * minDot;
						if(Vector3.Distance(linePos, colliderCentner) <= colliderRadius)
						{
							return true;
						}
					}

					colliders[i] = null;
				}
				else
				{
					colliders[i] = null;
				}
			}

			return false;
		}
	}
}
