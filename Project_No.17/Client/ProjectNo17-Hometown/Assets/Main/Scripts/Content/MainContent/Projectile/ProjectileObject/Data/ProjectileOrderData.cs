using BC.AIProvider;
using BC.ODCC;
using BC.OdccBase;

namespace TFContent
{
	public class ProjectileOrderData : ProjectileData
	{
		public ObjectBehaviour orderObject;
		public IFFProvider orderIFFProvider;
		public ActuatorModule actuatorModule;
		public ActuatorPlayer actuatorPlayer;
		protected override void Disposing()
		{
			orderObject = null;
			orderIFFProvider = null;
			actuatorModule = null;
			actuatorPlayer = null;
		}

		internal UnitRelationType GetHitTargetRelation(IFFProvider hitIffProvider)
		{
			return hitIffProvider == null ? UnitRelationType.Neutral : IFFProvider.IFFMatching.GetUnitRelation(orderIFFProvider, hitIffProvider);
		}
	}
}
