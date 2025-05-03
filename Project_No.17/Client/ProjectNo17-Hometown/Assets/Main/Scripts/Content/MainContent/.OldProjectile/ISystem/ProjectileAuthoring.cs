using System;
using System.Linq;

using BC.OdccBase;

using Sirenix.OdinInspector;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;
namespace TFContent
{
	[Serializable]
	public struct ProjectileBakerTag : IComponentData
	{

	}
	[Serializable]
	public struct ProjectilesBuffer : IBufferElementData
	{
		public Entity entityPrefab;
		public FixedString64Bytes typeName;
	}
	[Serializable]
	public struct ProjectileInstantiateData : IComponentData
	{
		public Entity entityPrefab;
		[HideInInspector]
		public FixedString64Bytes projectileType;

		public float maxLifeTime;
		public int maxHitCount;

		public float3 startPosition;
		public float3 targetPosition;
		public Entity trackingEntity;
		public float3 targetPositionOffset;

		public uint belongsTo;
		public uint collidesWith;

		public float speed;

		public ActuatorEffectType activityType;
		public float activityValue;
	}
	public class ProjectileAuthoring : MonoBehaviour
	{
		[TableList(HideToolbar = true)]
		public ProjectilesItem[] ProjectilesList;
		[Serializable]
		public struct ProjectilesItem
		{
			public string typeName;
			public GameObject gameObject;
		}
		[Button("Refresh"), PropertyOrder(-10)]
		public void OnValidate()
		{
			ProjectileManager projectileManager = FindFirstObjectByType<ProjectileManager>();
			if(projectileManager == null) return;
			if(ProjectilesList == null)
			{
				ProjectilesList = new ProjectilesItem[0];
				return;
			}

			// ProjectileManager에서 유형 목록 가져오기
			string[] typeList = projectileManager.projectileDataBakers.Select(i => i.ProjectileName).ToArray();

			// 기존 리스트를 필터링하여 typeList에 없는 항목 삭제
			ProjectilesList = ProjectilesList.Where(p => typeList.Contains(p.typeName)).ToArray();

			// typeList에 있지만 ProjectilesList에 없는 항목 추가
			var existingTypeNames = ProjectilesList.Select(p => p.typeName).ToHashSet();
			var missingTypes = typeList.Where(type => !existingTypeNames.Contains(type)).ToArray();

			// 새로운 항목 추가 (gameObject는 null로 초기화)
			var newItems = missingTypes.Select(type => new ProjectilesItem { gameObject = null, typeName = type }).ToArray();
			ProjectilesList = ProjectilesList.Concat(newItems).ToArray();

			// typeList 순서에 맞게 정렬
			ProjectilesList = ProjectilesList.OrderBy(p => Array.IndexOf(typeList, p.typeName)).ToArray();
		}
	}

	[System.Serializable]
	public class ProjectileMultiBaker : Baker<ProjectileAuthoring>
	{
		public override void Bake(ProjectileAuthoring authorings)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent<ProjectileBakerTag>(entity);
			var buffer = AddBuffer<ProjectilesBuffer>(entity);
			for(int index = 0, count = authorings?.ProjectilesList?.Length ?? 0 ; index < count ; ++index)
			{
				var item = authorings.ProjectilesList[index];
				var gameObject = item.gameObject;
				var typeName = item.typeName;

				if(gameObject == null) continue;

				if(string.IsNullOrWhiteSpace(typeName))
					typeName = item.gameObject.name;

				Debug.Log($"ProjectileBaker : {typeName}");

				Entity projectileEntity = GetEntity(gameObject, TransformUsageFlags.Dynamic);
				buffer.Add(new ProjectilesBuffer() {
					entityPrefab = projectileEntity,
					typeName = typeName,
				});
			}
		}
	}


	public struct ProjectileAlive : IComponentData
	{
		public FixedString64Bytes TypeName;
		public bool IsAlive;
	}
	public struct ProjectileLifeTime : IComponentData
	{
		public float LifeTime;
	}
	public struct ProjectileLifeHit : IComponentData
	{
		public float HitCount;
	}
	public struct ProjectileTargetPosition : IComponentData
	{
		public float3 StartPosition;
		public float3 TargetPosition;
		public Entity TrackingEntity;
		public float3 TargetPositionOffset;
	}
	public struct HitEffectPrefabComponent : IComponentData
	{
		public Entity Prefab;
	}
	//public struct ProjectileTriggerState : IComponentData
	//{
	//	public FixedList64Bytes<Entity> PreviousHits;
	//}
	public struct ProjectileImpactEvent : IComponentData
	{
		public bool IsEnter;

		public Entity ProjectileEntity; // 충돌한 총알
		public Entity HitEntity; // 충돌한 대상
	}

	public struct TrackingPosition : IComponentData
	{
		public float3 Position;
	}
}