using UnityEditor;
using UnityEditor.Animations;

public class BlendTreeCreator
{
	[MenuItem("Assets/Create/Animation/Blend Tree")]
	private static void CreateBlendTree()
	{
		string path = GetSelectedFolderPath();
		if(!string.IsNullOrWhiteSpace(path))
		{
			var asset = new BlendTree();
			path += "/" + $"{ObjectNames.GetUniqueName(new[] { "NewBlendTree" }, "NewBlendTree")}.asset";
			AssetDatabase.CreateAsset(asset, path);
		}
	}

	private static string GetSelectedFolderPath()
	{
		string path = "Assets";
		foreach(UnityEngine.Object obj in Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.Assets))
		{
			string tempPath = AssetDatabase.GetAssetPath(obj);
			if(AssetDatabase.IsValidFolder(tempPath))
			{
				path = tempPath;
				break;
			}
		}
		return path;
	}
}
