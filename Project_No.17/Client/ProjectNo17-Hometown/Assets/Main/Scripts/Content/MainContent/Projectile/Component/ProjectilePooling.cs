using System.Collections.Generic;
using System.Linq;

using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;

using Debug = UnityEngine.Debug;
namespace TFContent
{
	public class ProjectilePooling : ComponentBehaviour
	{
		[ShowInInspector, ReadOnly]
		private Dictionary<string, ProjectileObject> originalProjectileList;
		private Dictionary<string, Stack<ProjectileObject>> projectilePool;

		protected override void BaseAwake()
		{

			projectilePool = new Dictionary<string, Stack<ProjectileObject>>();
		}
		protected override void BaseDestroy()
		{
			if(originalProjectileList != null)
			{
				originalProjectileList.Clear();
				originalProjectileList = null;
			}

			if(projectilePool != null)
			{
				foreach(var stack in projectilePool.Values)
				{
					foreach(var projectileObject in stack)
					{
						projectileObject.DestroyThis();
					}
					stack.Clear();
				}
				projectilePool.Clear();
				projectilePool = null;
			}
		}
		public bool SetPoolingProjectile(ProjectileObject prefab)
		{
			if(prefab == null) return false;

			originalProjectileList ??= new Dictionary<string, ProjectileObject>();

			string projectileType = prefab.name;

			if(!originalProjectileList.ContainsKey(projectileType))
			{
				originalProjectileList.Add(projectileType, prefab);
			}
			return true;
		}
		public async Awaitable<ProjectileObject> PopProjectile(string projectileType, bool isActive = true)
		{
			if(projectilePool != null && projectilePool.TryGetValue(projectileType, out var stack) && stack != null)
			{
				if(stack.TryPop(out var projectileObject))
				{
					projectileObject.gameObject.SetActive(isActive);
					return projectileObject;
				}
			}
			if(originalProjectileList == null) return null;

			bool find = originalProjectileList.TryGetValue(projectileType,out var prefab);
			if(!find)
			{
				Debug.LogError($"Projectile Not Found : {projectileType}");
				return null;
			}
			if(prefab == null)
			{
				Debug.LogError($"Projectile Not Found : {projectileType} Is Null Prefab");
				return null;
			}
			prefab.gameObject.SetActive(isActive);
			ProjectileObject[] newProjectiles = await GameObject.InstantiateAsync<ProjectileObject>(prefab, ThisTransform);
#if UNITY_EDITOR
			prefab.gameObject.SetActive(true);
#endif
			var newProjectile = newProjectiles[0];
			newProjectile.gameObject.name = projectileType;
			newProjectile.gameObject.SetActive(isActive);
			return newProjectile;
		}
		public async Awaitable<ProjectileObject[]> PopProjectile(string projectileType, int count, bool isActive = true)
		{
			if(count <= 1)
			{
				var t = await PopProjectile(projectileType, isActive);
				return new ProjectileObject[] { t };
			}
			ProjectileObject[] projectileObjects = new ProjectileObject[count];

			if(projectilePool != null && projectilePool.TryGetValue(projectileType, out var stack) && stack != null)
			{
				while(count > 0 && stack.TryPop(out var projectileObject))
				{
					projectileObject.gameObject.SetActive(isActive);
					projectileObjects[--count] = projectileObject;
				}
			}
			if(count == 0) return projectileObjects;
			if(originalProjectileList == null) return projectileObjects;

			bool find = originalProjectileList.TryGetValue(projectileType,out var prefab);
			if(!find)
			{
				Debug.LogError($"Projectile Not Found : {projectileType}");
				return projectileObjects;
			}
			if(prefab == null)
			{
				Debug.LogError($"Projectile Not Found : {projectileType} Is Null Prefab");
				return projectileObjects;
			}
			var awaitResult = await GameObject.InstantiateAsync<ProjectileObject>(prefab, count, ThisTransform);
			for(int i = 0 ; i < count ; i++)
			{
				awaitResult[i].gameObject.name = projectileType;
				awaitResult[i].gameObject.SetActive(isActive);
				projectileObjects[i] = awaitResult[i];
			}
			return projectileObjects;
		}
		public void PushProjectile(ProjectileObject push)
		{
			if(push == null) return;
			if(projectilePool == null)
			{
				push.DestroyThis();
				return;
			}

			string projectileType = push.name;

			if(projectilePool.TryGetValue(projectileType, out var stack))
			{
				stack.Push(push);
			}
			else
			{
				stack = new Stack<ProjectileObject>();
				stack.Push(push);
				projectilePool.Add(projectileType, stack);
			}
			push.gameObject.SetActive(false);
		}
		public void PushProjectile(IEnumerable<ProjectileObject> pushs)
		{
			if(pushs == null || pushs.Count() == 0) return;
			if(projectilePool == null)
			{
				foreach(var push in pushs)
				{
					if(push == null) continue;
					push.DestroyThis();
				}
				return;
			}
			foreach(var push in pushs)
			{
				if(push == null) continue;
				string projectileType = push.name;
				if(projectilePool.TryGetValue(projectileType, out var stack))
				{
					stack.Push(push);
				}
				else
				{
					stack = new Stack<ProjectileObject>();
					stack.Push(push);
					projectilePool.Add(projectileType, stack);
				}
				push.gameObject.SetActive(false);
			}
		}
	}
}