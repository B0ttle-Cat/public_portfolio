#if UNITY_EDITOR
#endif
using System.IO;

using BC.Base;

using FOW;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFContent
{
	[CreateAssetMenu(fileName = "new WorldObjectData", menuName = "BC/WorldObjectData")]
	public class WorldObjectData : ScenarioObjectData
	{
		[TabGroup("Tab", "WorldObject", TabLayouting = TabLayouting.Shrink)]
		[Title("AstarPathObject")]
		public ResourcesKey<GameObject> AstarPathObject;
		[TabGroup("Tab", "WorldObject", TabLayouting = TabLayouting.Shrink)]
		[Title("TerrainObject")]
		public ResourcesKey<GameObject> TerrainObject;

		[TabGroup("Tab", "PrefabObject", TabLayouting = TabLayouting.Shrink)]
		[Title("PrefabBuilderGroup")]
		public ResourcesKey<GameObject> PrefabBuilderGroup;

		[TabGroup("Tab", "PrefabObject", TabLayouting = TabLayouting.Shrink)]
		[Title("OtherObjectGroup")]
		public ResourcesKey<GameObject> OtherObjectGroup;

		[TabGroup("Tab", "FogObject", TabLayouting = TabLayouting.Shrink)]
		[Title("FogOfWarObject")]
		public ResourcesKey<GameObject> FogOfWarObject;
#if UNITY_EDITOR
		public override void OnValidate()
		{
			string assetPath = Path.GetDirectoryName(UnityEditor.AssetDatabase.GetAssetPath(this));
			AstarPathObject.OnValidate(assetPath);
			TerrainObject.OnValidate(assetPath);
			PrefabBuilderGroup.OnValidate(assetPath);
			OtherObjectGroup.OnValidate(assetPath);
			FogOfWarObject.OnValidate(assetPath);
		}
#endif
		public void SaveAllObject()
		{
			// 나중에 필요해지면 작업.
		}
		public async Awaitable LoadAllObject(Transform thisTransform)
		{
			if(!TerrainObject.IsEmpty)
				await Object.InstantiateAsync<GameObject>(TerrainObject.LoadAsset(), thisTransform, Vector3.zero, Quaternion.identity);
			if(!PrefabBuilderGroup.IsEmpty)
				await Object.InstantiateAsync<GameObject>(PrefabBuilderGroup.LoadAsset(), thisTransform, Vector3.zero, Quaternion.identity);
			if(!OtherObjectGroup.IsEmpty)
				await Object.InstantiateAsync<GameObject>(OtherObjectGroup.LoadAsset(), thisTransform, Vector3.zero, Quaternion.identity);
			if(!AstarPathObject.IsEmpty)
				await Object.InstantiateAsync<GameObject>(AstarPathObject.LoadAsset(), thisTransform, Vector3.zero, Quaternion.identity);
			if(!FogOfWarObject.IsEmpty)
				await Object.InstantiateAsync<GameObject>(FogOfWarObject.LoadAsset(), thisTransform, Vector3.zero, Quaternion.identity);

			if(WorldSystem.FogOfWar == null) WorldSystem.FogOfWar = thisTransform.GetComponentInChildren<FogOfWarWorld>(true);
			if(WorldSystem.FogOfWar != null && WorldSystem.FogOfWar.TryGetComponent<MiniMapFrustum>(out var minimap))
			{
				//minimap.MapCollider = GetComponentInChildren<TerrainCollider>();
			}

			if(WorldSystem.MainPath == null) WorldSystem.MainPath = thisTransform.GetComponentInChildren<AstarPath>(true);
			if(WorldSystem.MainPath != null)
			{
				foreach(var progress in WorldSystem.MainPath.ScanAsync())
				{
					await Awaitable.NextFrameAsync();
				}
			}
		}
	}
}
