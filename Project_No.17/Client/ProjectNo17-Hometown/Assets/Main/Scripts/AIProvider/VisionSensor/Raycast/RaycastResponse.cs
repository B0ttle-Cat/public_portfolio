using Unity.Entities;
using Unity.Mathematics;

namespace BC.AIProvider
{
	public struct RaycastResponse : IComponentData
	{
		public bool IsHit;
		public float3 Origin;
		public Entity HitEntity;
		public float3 HitPoint;
		public float3 SurfaceNormal;
	}

}
