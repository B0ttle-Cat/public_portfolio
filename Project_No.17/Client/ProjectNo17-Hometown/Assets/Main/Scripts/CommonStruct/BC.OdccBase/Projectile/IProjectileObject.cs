using BC.ODCC;
namespace BC.OdccBase
{
	public interface IProjectileObject : IOdccObject
	{
		public bool Init(ObjectBehaviour order, IActuatorModule actuatorModule, IInstanceObject effectorObject, ReactionTargetInfo targetValue);
		public void OnFire();
	}
}