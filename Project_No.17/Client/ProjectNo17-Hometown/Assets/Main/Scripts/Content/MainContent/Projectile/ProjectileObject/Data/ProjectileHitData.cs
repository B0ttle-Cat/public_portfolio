using BC.OdccBase;

using UnityEngine;

namespace TFContent
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
