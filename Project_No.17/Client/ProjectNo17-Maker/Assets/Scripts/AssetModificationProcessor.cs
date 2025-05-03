#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

public class PrefabSaveFilter : AssetModificationProcessor
{
	static string[] OnWillSaveAssets(string[] paths)
	{
		foreach(var path in paths)
		{
			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if(prefab == null) continue;

			var sceneInstances = prefab.GetComponent<VRMConvertToContentComponent>();
			if(sceneInstances != null)
			{
				Debug.Log($"[{prefab.name}] ÇÁ¸®ÆÕ ÀúÀå Àü VRMConvertToContentComponent Á¦°ÅµÊ");
				Object.DestroyImmediate(sceneInstances, true);
			}
		}

		return paths;
	}
}
#endif
