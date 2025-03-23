using System.Linq;

using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class ProjectileManager : ObjectBehaviour, IOdccUpdate.Late
	{
		public static ProjectileManager Manager { get; private set; }
		private ProjectilePooling ProjectilePooling { get; set; }

		protected override void BaseAwake()
		{
			Manager = this;
		}

		protected override void BaseStart()
		{
			ProjectilePooling = ThisContainer.GetComponent<ProjectilePooling>();
		}

		void IOdccUpdate.Late.BaseLateUpdate()
		{

		}

		public async Awaitable<TProjectile> InstanceProjectile<TProjectile>(string projectileType, bool isActive = true) where TProjectile : ProjectileObject
		{
			ProjectileObject projectile = ProjectilePooling == null ? null : await ProjectilePooling.PopProjectile(projectileType, isActive);
			if(projectile == null || projectile is not TProjectile tProjectile) return null;
			return tProjectile;
		}
		public async Awaitable<TProjectile[]> InstanceProjectile<TProjectile>(string projectileType, int count, bool isActive = true) where TProjectile : ProjectileObject
		{
			ProjectileObject[] projectiles = ProjectilePooling == null ? (new ProjectileObject[0]) : await ProjectilePooling.PopProjectile(projectileType, count, isActive);
			if(projectiles == null) return null;
			return projectiles.Select((item) => item as TProjectile).ToArray();
		}
		public void DestroyProjectile(ProjectileObject projectileObject)
		{
			if(projectileObject == null) return;
			if(ProjectilePooling == null)
			{
				projectileObject.DestroyThis();
				return;
			}

			ProjectilePooling.PushProjectile(projectileObject);
		}
		public void DestroyProjectile(ProjectileObject[] projectileObjects)
		{
			if(projectileObjects == null) return;
			if(ProjectilePooling == null)
			{
				foreach(var item in projectileObjects)
				{
					item.DestroyThis();
				}
				return;
			}

			ProjectilePooling.PushProjectile(projectileObjects);
		}

		internal bool SetPoolingProjectile(ProjectileObject prefab)
		{
			if(ProjectilePooling == null) return false;
			return ProjectilePooling.SetPoolingProjectile(prefab);
		}
	}
}