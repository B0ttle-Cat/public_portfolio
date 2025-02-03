using Shapes;

using Unity.Mathematics;

using UnityEngine;

namespace BC.Shape
{
	[System.Serializable]
	public struct ArcShape
	{
		[Range(0f,360f)]
		public float CenterAngle;  // 중심 각도
		[Range(0f,360f)]
		public float AngleOffset;  // 각도 오프셋
		public float MinDistance;  // 최소 거리
		public float MaxDistance;  // 최대 거리
		public float MinAngle => CenterAngle - AngleOffset * 0.5f;  // 최소 각도
		public float MaxAngle => CenterAngle + AngleOffset * 0.5f;  // 최대 각도
		public float CenterDistance => MinDistance + (MaxDistance - MinDistance) * 0.5f;


		public void OnDrawShapes()
		{
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);
			float thickness = MaxDistance - MinDistance;
			float radius = MinDistance + thickness * 0.5f;
			float angleRadStart = (MinAngle-90f) * Mathf.Deg2Rad;
			float angleRadEnd = (MaxAngle-90f) * Mathf.Deg2Rad;
			Draw.Arc(pos, rot, radius, thickness, angleRadStart, angleRadEnd);
		}
		public void OnDrawShapes(float distanceOffset, float angleOffset)
		{
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);
			float thickness = MaxDistance - MinDistance;
			float radius = MinDistance + thickness * 0.5f;
			float angleRadStart = (MinAngle-90f - (angleOffset * 0.5f)) * Mathf.Deg2Rad;
			float angleRadEnd = (MaxAngle-90f + (angleOffset * 0.5f)) * Mathf.Deg2Rad;
			Draw.Arc(pos, rot, radius, thickness + distanceOffset, angleRadStart, angleRadEnd);
		}


		public bool SphereColliderHit(float2 arcCenter, float2 circleCenter, float circleRadius)
		{
			// circleCenterXZ = 로컬 값 이어야 함.
			circleCenter -= arcCenter;

			// 원의 중심
			float circleDistance = math.length(circleCenter);
			float circleAngle = GetAngle(circleCenter);

			// 원의 중심과 가장 가까운 호 위의 점
			float clampDistance = math.clamp(circleDistance, MinDistance, MaxDistance);
			float clampAngle = math.clamp(circleAngle, MinAngle, MaxAngle);

			// 중심이 내부에 위치함;
			if(circleDistance - clampDistance <= float.Epsilon && circleAngle - clampAngle <= float.Epsilon) return true;

			// 검사할 점의 위치를 구함;
			float angleRadians = clampAngle * math.TORADIANS;
			float2 clampCenter = new float2(-math.sin(angleRadians), math.cos(angleRadians));
			clampCenter *= clampDistance;

			// 외부에 중심이 있지만 가장 가까운 지점과 반지름 이하로 떨어져 있음
			if(math.distance(circleCenter, clampCenter) <= circleRadius)
			{
				return true;
			}

			return false;
			float GetAngle(float2 P)
			{
				float2 direction = math.normalize(P);
				float angle = math.atan2(direction.y, direction.x) * math.TODEGREES;
				float adjustedAngle = 90f - angle;
				return adjustedAngle < 0 ? (adjustedAngle += 360f) : adjustedAngle;
			}
		}
		public bool BoxColliderHit(float2 arcCenter, float2 center, float2 size, float rotation)
		{
			center -= arcCenter;

			float angleRadians = rotation * math.TORADIANS;
			float2 dirUp = new float2(-math.sin(angleRadians), math.cos(angleRadians));
			float2 dirRight = new float2(dirUp .y, -dirUp .x);
			float2 halfSize = size * 0.5f;

			float2 LB = center + (-dirUp * halfSize.y + -dirRight * halfSize.x);
			float2 LT = center + (dirUp * halfSize.y + -dirRight * halfSize.x);
			float2 RT = center + (dirUp * halfSize.y + dirRight * halfSize.x);
			float2 RB = center + (-dirUp * halfSize.y + dirRight * halfSize.x);
			float minDistance = MinDistance;
			float maxDistance = MaxDistance;
			float minAngle = MinAngle;
			float maxAngle = MaxAngle;
			if(IsInsidePoint(LB)) return true;
			if(IsInsidePoint(LT)) return true;
			if(IsInsidePoint(RT)) return true;
			if(IsInsidePoint(RB)) return true;

			float minARad = minAngle * math.TORADIANS;
			float maxARad = maxAngle * math.TORADIANS;
			float2 boundaryPoints0 = new float2(math.cos(minARad), math.sin(minARad)) * minDistance;
			float2 boundaryPoints1 = new float2(math.cos(minARad), math.sin(minARad)) * maxDistance;
			float2 boundaryPoints2 = new float2(math.cos(maxARad), math.sin(maxARad)) * minDistance;
			float2 boundaryPoints3 = new float2(math.cos(maxARad), math.sin(maxARad)) * maxDistance;
			if(IsPointInsideRotatedRect(boundaryPoints0, center, size, rotation)) return true;
			if(IsPointInsideRotatedRect(boundaryPoints1, center, size, rotation)) return true;
			if(IsPointInsideRotatedRect(boundaryPoints2, center, size, rotation)) return true;
			if(IsPointInsideRotatedRect(boundaryPoints3, center, size, rotation)) return true;

			if(GetLineArcIntersections(LB, LT)) return true;
			if(GetLineArcIntersections(LT, RT)) return true;
			if(GetLineArcIntersections(RT, RB)) return true;
			if(GetLineArcIntersections(RB, LB)) return true;

			bool IsInsidePoint(float2 corner)
			{
				float distance = math.length(arcCenter);
				float angle = GetAngle(corner);

				bool withinDistance = (distance >= minDistance && distance <= maxDistance);
				bool withinAngle = IsAngleInRange(angle, minAngle, maxAngle);

				if(withinDistance && withinAngle)
				{
					return true;
				}
				return false;
			}
			bool GetLineArcIntersections(float2 A, float2 B)
			{
				{
					if(GetLineCircleIntersections(A, B, maxDistance, out float2? crossA, out float2? crossB))
					{
						if(crossA.HasValue && IsAngleInRange(GetAngle(crossA.Value), minAngle, maxAngle)) return true;
						if(crossB.HasValue && IsAngleInRange(GetAngle(crossB.Value), minAngle, maxAngle)) return true;
					}
				}
				{
					if(GetLineCircleIntersections(A, B, maxAngle, out float2? crossA, out float2? crossB))
					{
						if(crossA.HasValue && IsAngleInRange(GetAngle(crossA.Value), minAngle, maxAngle)) return true;
						if(crossB.HasValue && IsAngleInRange(GetAngle(crossB.Value), minAngle, maxAngle)) return true;
					}
				}
				return false;
			}
			float GetAngle(float2 P)
			{
				float2 direction  = math.normalize(P);
				float angle = math.atan2(direction.y, direction.x) * math.TODEGREES;
				float adjustedAngle = 90f - angle;
				return adjustedAngle < 0 ? (adjustedAngle += 360f) : adjustedAngle;
			}
			bool IsAngleInRange(float angle, float minAngle, float maxAngle)
			{
				angle = (angle + 360) % 360; // 음수 각도 보정
				minAngle = (minAngle + 360) % 360;
				maxAngle = (maxAngle + 360) % 360;

				if(minAngle <= maxAngle)
				{
					return angle >= minAngle && angle <= maxAngle;
				}
				else
				{
					return angle >= minAngle || angle <= maxAngle;
				}
			}
			bool GetLineCircleIntersections(float2 A, float2 B, float radius, out float2? intersection1, out float2? intersection2)
			{
				intersection1 = null;
				intersection2 = null;

				float2 d = B - A;
				float2 f = A;

				float a = math.dot(d, d);
				float b = 2 * math.dot(f, d);
				float c = math.dot(f, f) - radius * radius;

				float discriminant = b * b - 4 * a * c;

				if(discriminant < 0)
					return false; // 교차점 없음

				discriminant = math.sqrt(discriminant);

				float t1 = (-b - discriminant) / (2 * a);
				float t2 = (-b + discriminant) / (2 * a);

				bool hasIntersection = false;

				if(t1 >= 0 && t1 <= 1)
				{
					intersection1 = A + t1 * d;
					hasIntersection = true;
				}

				if(t2 >= 0 && t2 <= 1)
				{
					intersection2 = A + t2 * d;
					hasIntersection = true;
				}

				return hasIntersection;
			}

			//bool IsInsideRect(float2[] arcBoundaryPoints)
			//{
			//	foreach(var point in arcBoundaryPoints)
			//	{
			//		if(IsPointInsideRotatedRect(point, center, size, rotation))
			//		{
			//			return true;
			//		}
			//	}
			//	return false;
			//}

			bool IsPointInsideRotatedRect(float2 point, float2 rectCenter, float2 size, float rotation)
			{
				// 점을 사각형의 중심 기준으로 상대 위치 변환
				float2 localPoint = point - rectCenter;

				// 회전을 반대로 적용하여 로컬 좌표로 변환
				float rad = math.radians(-rotation);
				float cosTheta = math.cos(rad);
				float sinTheta = math.sin(rad);

				float2 rotatedPoint = new float2(
					cosTheta * localPoint.x - sinTheta * localPoint.y,
					sinTheta * localPoint.x + cosTheta * localPoint.y
				);

				// 사각형 내부에 포함되는지 검사
				return math.abs(rotatedPoint.x) <= size.x / 2 && math.abs(rotatedPoint.y) <= size.y / 2;
			}

			return false;
		}
	}
}
