using System;

using Shapes;

using UnityEngine;

namespace BC.Shape
{
	[Serializable]
	public struct SphereShape
	{
		public Vector3 Center;
		public float Radius;

		public void OnDrawShapes(float offsetY = 0f, float offsetRadius = 0f)
		{
			Vector3 pos = Center + Vector3.up * offsetY;
			Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);

			Draw.Disc(pos, rot, Radius + offsetRadius);
		}
		public void OnDrawRing(float insideThickness, float offsetY = 0f, float offsetRadius = 0)
		{
			Vector3 pos = Center + Vector3.up * offsetY; ;
			Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);

			Draw.Ring(pos, rot, Radius + offsetRadius, insideThickness);
		}
	}
}
