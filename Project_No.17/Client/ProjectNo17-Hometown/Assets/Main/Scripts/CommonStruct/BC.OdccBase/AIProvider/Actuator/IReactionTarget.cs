using BC.ODCC;

using UnityEngine;
namespace BC.OdccBase
{
	public interface IReactionTarget : IOdccComponent
	{
		public Vector3 Position { get; }
		public Quaternion Rotation { get; }
		public float Radius { get; }
		public int Layer { get; }
	}
}