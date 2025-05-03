using BC.ODCC;
using BC.OdccBase;

namespace BC.Projectile
{
	public class ProjectileOrderData : ProjectileData
	{
		public ObjectBehaviour orderObject;
		public IProjectileIFF orderIFFProvider;
		public IAbilityProvider abilityStruct;
		public IActuatorModule actuatorModule;
		public IInstanceObject effectorObject;
		protected override void Disposing()
		{
			orderObject = null;
			orderIFFProvider = null;
			actuatorModule = null;
		}

		internal UnitRelationType GetHitTargetRelation(IProjectileIFF hitIffProvider)
		{
			return hitIffProvider == null ? UnitRelationType.Neutral : orderIFFProvider.GetUnitRelation(hitIffProvider);
		}
	}
}
