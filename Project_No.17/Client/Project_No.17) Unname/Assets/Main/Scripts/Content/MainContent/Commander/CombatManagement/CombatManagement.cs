using System;
using System.Collections.Generic;

using BC.AISensor;
using BC.ODCC;

using Unity.Entities;
namespace TFContent
{
	public partial class CombatManagement : ComponentBehaviour
	{
		public static CombatManagement Manager;
		private World defaultWorld;

		public IFFMatchingInfoData iffMatchingInfoData { get; set; }

		public Dictionary<ObjectBehaviour, Entity> ObjectMapping = new Dictionary<ObjectBehaviour, Entity>();
		public Dictionary<Entity, SensorProvider> SensorMapping = new Dictionary<Entity, SensorProvider>();
		public Dictionary<Entity, IFFProvider> IFFMapping = new Dictionary<Entity, IFFProvider>();
		protected override void BaseAwake()
		{
			Manager = this;
			defaultWorld = null;
		}
		protected override void BaseEnable()
		{
			if(defaultWorld == null)
			{
				defaultWorld = World.DefaultGameObjectInjectionWorld;
				EnableCombatManagementUpdate();
			}
		}
		protected override void BaseDisable()
		{
			if(defaultWorld != null)
			{
				DisableCombatManagementUpdate();
			}
			defaultWorld = null;
		}

		public void UpdateCombatEntity(ObjectBehaviour checkObject, out Entity entity)
		{
			if(!ObjectMapping.ContainsKey(checkObject))
			{
				ObjectMapping[checkObject] = defaultWorld.EntityManager.CreateEntity();
			}

			entity = ObjectMapping[checkObject];

			if(UpdateComponent<SensorProvider, SensorTag>(entity, (to) => new SensorTag(to), out var sensor))
			{
				SensorMapping[entity] = sensor;
			}
			else
			{
				SensorMapping.Remove(entity);
			}
			if(UpdateComponent<IFFProvider, IFFTag>(entity, (to) => new IFFTag { teamID = to.TeamID, unitID = to.UnitID }, out var iff))
			{
				IFFMapping[entity] = iff;
			}
			else
			{
				IFFMapping.Remove(entity);
			}
			bool UpdateComponent<T0, T1>(Entity entity, Func<T0, T1> t1, out T0 t0) where T0 : class, IOdccComponent where T1 : unmanaged, IComponentData
			{
				if(t1 != null && checkObject.ThisContainer.TryGetComponent<T0>(out var _t0))
				{
					if(!defaultWorld.EntityManager.HasComponent<T1>(entity))
					{
						defaultWorld.EntityManager.AddComponentData<T1>(entity, t1(_t0));
					}
					else
					{
						defaultWorld.EntityManager.SetComponentData<T1>(entity, t1(_t0));
					}
					t0 = _t0;
					return true;
				}
				else
				{
					if(defaultWorld.EntityManager.HasComponent<SensorTag>(entity))
					{
						defaultWorld.EntityManager.RemoveComponent<SensorTag>(entity);
					}
					t0 = null;
					return false;
				}
			}
		}
		public void DeleteCombatEntity(ObjectBehaviour checkObject, Entity entity)
		{
			if(!ObjectMapping.ContainsKey(checkObject))
			{
				return;
			}

			SensorMapping.Remove(entity);
			IFFMapping.Remove(entity);
			ObjectMapping.Remove(checkObject);
			defaultWorld.EntityManager.DestroyEntity(entity);
		}


		private void EnableCombatManagementUpdate()
		{
			Create<CombatSensorUpdate>();
			Create<CombatIFFUpdate>();

			void Create<T>() where T : unmanaged, ISystem
			{
				if(defaultWorld.GetExistingSystem<T>() == null)
				{
					defaultWorld.CreateSystem<T>();
				}
			}
		}
		private void DisableCombatManagementUpdate()
		{
			Destroy<CombatSensorUpdate>();
			Destroy<CombatIFFUpdate>();

			void Destroy<T>() where T : unmanaged, ISystem
			{
				var system = defaultWorld.GetExistingSystem<T>();
				if(system != null)
				{
					defaultWorld.DestroySystem(system);
				}
			}
		}
	}
}