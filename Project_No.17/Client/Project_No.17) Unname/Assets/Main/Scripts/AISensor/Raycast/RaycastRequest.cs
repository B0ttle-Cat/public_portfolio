using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
namespace BC.AISensor
{
	public struct RaycastRequest : IComponentData
	{
		public float3 StartPosition;
		public float3 EndPosition;
		public CollisionFilter Filter;
	}
}