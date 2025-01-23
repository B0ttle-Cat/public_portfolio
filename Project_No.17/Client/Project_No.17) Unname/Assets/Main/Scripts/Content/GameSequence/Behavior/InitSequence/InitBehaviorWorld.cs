using BC.OdccBase;

using TFSystem;

using UnityEngine;

using Object = UnityEngine.Object;

namespace TFContent
{
	public class InitBehaviorWorld : OdccBehaviorTarget
	{
		[SerializeField]
		private AstarPath astarPath;

		private WorldObjectData worldObjectData;

		protected override bool OnActionValid()
		{
			worldObjectData = DataCarrier.GetSeparate<GameContentLoader>().GetData<WorldObjectData>();
			return worldObjectData != null;
		}

		protected override async Awaitable OnActionUpdate()
		{
			await Object.InstantiateAsync<GameObject>(worldObjectData.TerrainObject.LoadAsset(), ThisTransform, Vector3.zero, Quaternion.identity);
			await Object.InstantiateAsync<GameObject>(worldObjectData.PrefabBuilderGroup.LoadAsset(), ThisTransform, Vector3.zero, Quaternion.identity);
			await Object.InstantiateAsync<GameObject>(worldObjectData.OtherObjectGroup.LoadAsset(), ThisTransform, Vector3.zero, Quaternion.identity);

			if(astarPath != null)
			{
				foreach(var progress in astarPath.ScanAsync())
				{
					await Awaitable.NextFrameAsync();
				}
			}
		}

		protected override void OnActionEnd()
		{
			DestroyThis();
			astarPath = null;
			worldObjectData = null;
		}
	}
}