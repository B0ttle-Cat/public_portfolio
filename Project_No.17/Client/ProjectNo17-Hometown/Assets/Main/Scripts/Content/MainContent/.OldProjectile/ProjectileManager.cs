using System.Collections.Generic;

using BC.AIProvider;
using BC.Base;
using BC.ODCC;

using Sirenix.OdinInspector;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

using UnityEngine;

using Debug = UnityEngine.Debug;
namespace TFContent
{
	public class ProjectileManager : MonoBehaviour
	{
		public static ProjectileManager Manager { get; private set; }
		private EntityManager entityManager;
		[SerializeField]
		private LayerMask projectileLayerMask;
		[SerializeField, ReadOnly, EnableGUI, InlineButton("UpdateHitTargetLayerMask", " Update ")]
		private LayerMask hitTargetLayerMask;

		// 총알 Entity 프리팹 캐싱
		private Dictionary<string, Entity> projectileEntityCache = new Dictionary<string, Entity>();

		private Dictionary<Transform, Entity> gameObjectToTargetMap = new();
		[SerializeReference]
		public List<ProjectileDataBaker> projectileDataBakers;

		private void Awake()
		{
			if(Manager == null) Manager = this;
			else Destroy(gameObject);

			var DefaultWorld = World.DefaultGameObjectInjectionWorld;
			entityManager = DefaultWorld.EntityManager;

			UpdateHitTargetLayerMask();
		}

		private void Start()
		{
			CacheProjectileEntities();
		}

		private void UpdateHitTargetLayerMask()
		{
			hitTargetLayerMask = 0;
			int layer = projectileLayerMask.ToLayer();
			for(int i = 0 ; i < 32 ; i++)
			{
				if(!Physics.GetIgnoreLayerCollision(layer, i))
				{
					if(LayerMask.LayerToName(i).IsNotNullOrWhiteSpace())
					{
						hitTargetLayerMask += 1<<i;
					}
				}
			}
		}

		private void CacheProjectileEntities()
		{
			EntityQuery query = entityManager.CreateEntityQuery(typeof(ProjectileBakerTag));
			using(var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp))
			{
				foreach(var entity in entities)
				{
					var dataBuffer = entityManager.GetBuffer<ProjectilesBuffer>(entity);
					{
						int length= dataBuffer.Length;
						for(int i = 0 ; i < length ; i++)
						{
							var prefab =  dataBuffer[i].entityPrefab;
							var type =  dataBuffer[i].typeName;

							string typeString = type.Value;
							EntityComponentDataBaker(typeString, prefab);

							projectileEntityCache[typeString] = prefab;
							Debug.Log($"Get ProjectileBaker : {typeString}");
						}
					}
				}
			}
			void EntityComponentDataBaker(string typeString, Entity entity)
			{
				for(int ii = 0, count = projectileDataBakers?.Count ?? 0 ; ii<count ; ii++)
				{
					ProjectileDataBaker item = projectileDataBakers[ii];
					if(item.ProjectileName.Equals(typeString))
					{
						item.OnEntityDataBake(entityManager, entity);
						break;
					}
				}
			}
		}

		public void Fire(string projectileType, ProjectileInstantiateData instantiateData)
		{
			if(!projectileEntityCache.TryGetValue(projectileType, out Entity entityPrefab))
			{
				Debug.LogError($"Projectile type '{projectileType}' not found!");
				return;
			}

			instantiateData.entityPrefab = entityPrefab;
			instantiateData.belongsTo = (uint)projectileLayerMask.value;
			instantiateData.collidesWith = (uint)hitTargetLayerMask.value;

			var prefabLoader = entityManager.CreateEntity();
			entityManager.AddComponentData(prefabLoader, instantiateData);
		}

		public void OnProjectileImpact(ProjectileImpactEvent impactEvent)
		{
			Debug.Log($"Projectile {impactEvent.ProjectileEntity}" +
				$"{(impactEvent.IsEnter ? "<color=blue>hitEnter</color>" : "<color=red>hitExit</color>")}" +
				$"{impactEvent.HitEntity}");
			// ✅ 충돌 정보를 활용하여 필요한 처리 수행 (ex: 피격 이펙트 추가, 데미지 처리)
		}
		public Entity GetTrackingEntity(ObjectBehaviour gameObject)
		{
			if(gameObject == null) return Entity.Null;
			return GetTrackingEntity(gameObject.ThisTransform, gameObject.ThisContainer.GetComponent<ReactionTarget>());
		}
		public Entity GetTrackingEntity(GameObject gameObject)
		{
			if(gameObject == null) return Entity.Null;
			return GetTrackingEntity(gameObject.transform, gameObject.GetComponentInChildren<ReactionTarget>());
		}
		public Entity GetTrackingEntity(Transform transform)
		{
			if(transform == null) return Entity.Null;
			return GetTrackingEntity(transform, transform.gameObject.GetComponentInChildren<ReactionTarget>());
		}
		private Entity GetTrackingEntity(Transform transform, ReactionTarget reactionTarget)
		{
			if(transform == null) return Entity.Null;
			if(!gameObjectToTargetMap.TryGetValue(transform, out var trackingEntity))
			{
				float3 Position = transform.position;

				trackingEntity = entityManager.CreateEntity();
				if(reactionTarget != null)
				{
					PhysicsCollider physicsCollider = reactionTarget.GetPhysicsCollider();
					entityManager.AddComponentData(trackingEntity, physicsCollider);
					unsafe
					{
						var capsuleCollider = ((Unity.Physics.CapsuleCollider*)physicsCollider.ColliderPtr);
						var geometry = capsuleCollider->Geometry;
						Position += geometry.GetCenter();
					}
				}
				entityManager.AddComponentData(trackingEntity, new TrackingPosition { Position = Position });
				gameObjectToTargetMap.Add(transform, trackingEntity);
			}
			return trackingEntity;
		}

		private void Update()
		{
			List<Transform> objectsToRemove = new();

			foreach(var kvp in gameObjectToTargetMap)
			{
				Transform targetTransform = kvp.Key;
				Entity targetEntity = kvp.Value;

				// ✅ Key가 삭제되었으면 Dictionary에서 제거
				if(targetTransform == null)
				{
					objectsToRemove.Add(targetTransform);
					continue;
				}

				// ✅ Entity가 삭제되었으면 Dictionary에서 제거
				if(!entityManager.Exists(targetEntity))
				{
					objectsToRemove.Add(targetTransform);
					continue;
				}

				// ✅ targetTransform 가 비활성화 되었으면 갱신 무시
				if(!targetTransform.gameObject.activeInHierarchy) continue;

				// ✅ Entity의 TargetPosition 업데이트
				float3 Position = targetTransform.position;
				if(entityManager.HasComponent<PhysicsCollider>(targetEntity))
				{
					PhysicsCollider physicsCollider = entityManager.GetComponentData<PhysicsCollider>(targetEntity);
					unsafe
					{
						var capsuleCollider = ((Unity.Physics.CapsuleCollider*)physicsCollider.ColliderPtr);
						var geometry = capsuleCollider->Geometry;
						Position += geometry.GetCenter();
					}
				}
				entityManager.SetComponentData(targetEntity, new TrackingPosition {
					Position = Position
				});
			}

			// ✅ 삭제할 GameObject를 Dictionary에서 제거
			foreach(var obj in objectsToRemove)
			{
				gameObjectToTargetMap.Remove(obj);
			}
		}
	}
}