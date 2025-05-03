using System;

using Sirenix.OdinInspector;

using Unity.Entities;
namespace TFContent
{
	[Serializable]
	public abstract class ProjectileDataBaker
	{
		[ShowInInspector]
		public string ProjectileName {
			get => string.IsNullOrWhiteSpace(projectileName) ? GetType().Name : projectileName;
			set => projectileName = value;
		}

		private string projectileName = "";
		public abstract void OnEntityDataBake(EntityManager entityManager, Entity entity);
	}
}