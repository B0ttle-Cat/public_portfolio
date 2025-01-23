using UnityEditor;

using UnityEngine;

public class SpriteBorderSetterSelected : EditorWindow
{
	private Vector4 border = new Vector4(16, 16, 16, 16); // 좌, 하, 우, 상 Border 값

	[MenuItem("Tools/Sprite Border Setter (Selected)")]
	public static void ShowWindow()
	{
		GetWindow<SpriteBorderSetterSelected>("Sprite Border Setter");
	}

	private void OnGUI()
	{
		GUILayout.Label("Sprite Border Setter (Selected)", EditorStyles.boldLabel);

		border = EditorGUILayout.Vector4Field("Border (Left, Bottom, Right, Top)", border);

		if(GUILayout.Button("Set Border for Selected Files"))
		{
			SetSpriteBordersForSelected(border);
		}
	}

	private void SetSpriteBordersForSelected(Vector4 newBorder)
	{
		// 선택된 파일들 가져오기
		var selectedObjects = Selection.objects;

		foreach(var obj in selectedObjects)
		{
			string assetPath = AssetDatabase.GetAssetPath(obj);
			TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

			if(importer != null && importer.textureType == TextureImporterType.Sprite)
			{
				importer.spriteBorder = newBorder;
				EditorUtility.SetDirty(importer);
				importer.SaveAndReimport();
				Debug.Log($"Set border for {assetPath} to {newBorder}");
			}
			else
			{
				Debug.LogWarning($"Skipped: {assetPath} is not a Sprite or valid Texture.");
			}
		}

		AssetDatabase.Refresh();
		Debug.Log("Sprite borders updated for selected files!");
	}
}
