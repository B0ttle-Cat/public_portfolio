using BC.ODCC;

using UnityEngine;
namespace BC.OdccBase
{
	public interface IProjectileManager : IOdccObject
	{
		public bool SetPoolingProjectile(GameObject prefab);

		public Awaitable<GameObject> InstanceProjectile(string projectileType, bool isActive = true);
		public Awaitable<GameObject[]> InstanceProjectile(string projectileType, int count, bool isActive = true);
		public Awaitable<TProjectile> InstanceProjectile<TProjectile>(string projectileType, bool isActive = true) where TProjectile : class, IProjectileObject;
		public Awaitable<TProjectile[]> InstanceProjectile<TProjectile>(string projectileType, int count, bool isActive = true) where TProjectile : class, IProjectileObject;
	}
}