using BC.OdccBase;

using UnityEngine;

namespace BC.Projectile
{
	public class ProjectileHitData : ProjectileData
	{
		public LayerMask projectileLayer;
		public LayerMask blockingLayer;
		public LayerMask hitEffectLayer;
		public UnitRelationType hitUnitRelationType;
		protected override void Disposing()
		{
		}
	}
}
