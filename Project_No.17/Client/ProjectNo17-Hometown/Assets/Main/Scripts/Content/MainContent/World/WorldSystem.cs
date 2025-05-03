using BC.ODCC;

using FOW;
namespace TFContent
{
	public class WorldSystem : ObjectBehaviour//, IOdccUpdate
	{
		public static AstarPath MainPath;
		public static FogOfWarWorld FogOfWar;

		protected override void BaseAwake()
		{
			MainPath = GetComponentInChildren<AstarPath>(true);
			FogOfWar = GetComponentInChildren<FogOfWarWorld>(true);
		}
	}
}