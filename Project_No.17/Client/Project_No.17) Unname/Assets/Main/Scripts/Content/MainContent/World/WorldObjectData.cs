#if UNITY_EDITOR
#endif
using BC.Base;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFContent
{
	[CreateAssetMenu(fileName = "new WorldObjectData", menuName = "BC/WorldObjectData")]
	public class WorldObjectData : ScriptableObject
	{
		[FoldoutGroup("TerrainObject")]
		public ResourcesKey<GameObject> AstarPathObject;

		[FoldoutGroup("TerrainObject")]
		public ResourcesKey<GameObject> TerrainObject;
		[FoldoutGroup("PrefabBuilderGroup")]
		public ResourcesKey<GameObject> PrefabBuilderGroup;
		[FoldoutGroup("OtherObjectGroup")]
		public ResourcesKey<GameObject> OtherObjectGroup;

		[FoldoutGroup("FogOfWarWorld Object")]
		public ResourcesKey<GameObject> FogOfWarObject;

		public void OnValidate()
		{
			AstarPathObject.OnValidate();
			TerrainObject.OnValidate();
			PrefabBuilderGroup.OnValidate();
			OtherObjectGroup.OnValidate();
			FogOfWarObject.OnValidate();
		}
	}
}
