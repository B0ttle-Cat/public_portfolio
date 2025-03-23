using BC.OdccBase;

using FOW;

using TFSystem;

using UnityEngine;

using Object = UnityEngine.Object;

namespace TFContent
{
	public class InitBehaviorWorld : OdccBehaviorTarget
	{
		private WorldObjectData worldObjectData;

		protected override bool OnActionValid()
		{
#if UNITY_EDITOR
			int childCount = transform.childCount;
			for(int i = 0 ; i < childCount ; i++)
			{
				//if(transform.GetChild(i).gameObject.tag =="EditorOnly")
				{
					DestroyImmediate(transform.GetChild(i).gameObject);
					i--;
					childCount--;
				}
			}
#endif
			worldObjectData = DataCarrier.GetSeparate<GameContentLoader>().GetData<WorldObjectData>();
			return worldObjectData != null;
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(!worldObjectData.TerrainObject.IsEmpty)
				await Object.InstantiateAsync<GameObject>(worldObjectData.TerrainObject.LoadAsset(), ThisTransform, Vector3.zero, Quaternion.identity);
			if(!worldObjectData.PrefabBuilderGroup.IsEmpty)
				await Object.InstantiateAsync<GameObject>(worldObjectData.PrefabBuilderGroup.LoadAsset(), ThisTransform, Vector3.zero, Quaternion.identity);
			if(!worldObjectData.OtherObjectGroup.IsEmpty)
				await Object.InstantiateAsync<GameObject>(worldObjectData.OtherObjectGroup.LoadAsset(), ThisTransform, Vector3.zero, Quaternion.identity);
			if(!worldObjectData.AstarPathObject.IsEmpty)
				await Object.InstantiateAsync<GameObject>(worldObjectData.AstarPathObject.LoadAsset(), ThisTransform, Vector3.zero, Quaternion.identity);
			if(!worldObjectData.FogOfWarObject.IsEmpty)
				await Object.InstantiateAsync<GameObject>(worldObjectData.FogOfWarObject.LoadAsset(), ThisTransform, Vector3.zero, Quaternion.identity);

			WorldSystem.FogOfWar = ThisTransform.GetComponentInChildren<FogOfWarWorld>(true);
			if(WorldSystem.FogOfWar != null && WorldSystem.FogOfWar.TryGetComponent<MiniMapFrustum>(out var minimap))
			{
				//minimap.MapCollider = GetComponentInChildren<TerrainCollider>();
			}

			WorldSystem.MainPath = ThisTransform.GetComponentInChildren<AstarPath>(true);
			if(WorldSystem.MainPath != null)
			{
				foreach(var progress in WorldSystem.MainPath.ScanAsync())
				{
					await Awaitable.NextFrameAsync();
				}
			}
		}

		protected override void OnActionEnd()
		{
			worldObjectData = null;
		}
	}
}