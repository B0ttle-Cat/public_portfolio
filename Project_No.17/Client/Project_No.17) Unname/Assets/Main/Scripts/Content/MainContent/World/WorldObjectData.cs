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
		public ResourcesKey<GameObject> TerrainObject;
		[FoldoutGroup("PrefabBuilderGroup")]
		public ResourcesKey<GameObject> PrefabBuilderGroup;
		[FoldoutGroup("OtherObjectGroup")]
		public ResourcesKey<GameObject> OtherObjectGroup;

		public void OnValidate()
		{
			TerrainObject.OnValidate();
			PrefabBuilderGroup.OnValidate();
			OtherObjectGroup.OnValidate();
		}

		[ButtonGroup("SaveLoad")]
		public string SaveData()
		{
			return JsonUtility.ToJson(this, true);
		}
		[ButtonGroup("SaveLoad")]
		public void LoadData(string jsonData)
		{
			if(string.IsNullOrWhiteSpace(jsonData)) return;
			JsonUtility.FromJsonOverwrite(jsonData, this);
		}
	}
}
