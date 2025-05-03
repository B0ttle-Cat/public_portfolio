using BC.ODCC;
using BC.OdccBase;
namespace BC.Projectile
{
	public interface IProjectileIFF : IOdccComponent
	{
		public int TeamID { get; }
		public int UnitID { get; }
		UnitRelationType GetUnitRelation(IProjectileIFF target);
	}
}