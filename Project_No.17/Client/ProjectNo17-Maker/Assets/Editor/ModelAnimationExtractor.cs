#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;

using UnityEditor;

using UnityEngine;

public static class ModelAnimationExtractor
{
	// Unity에서 모델로 인식되는 확장자들
	private static readonly string[] SupportedModelExtensions = new[]
	{
		".fbx", ".dae", ".obj", ".blend", ".glb", ".gltf", ".mb", ".ma"
	};

	[MenuItem("Assets/Extract Animations From Model", true)]
	private static bool ValidateExtractAnimations()
	{
		return Selection.objects != null && Selection.objects.Length > 0;
	}

	[MenuItem("Assets/Extract Animations From Model")]
	private static void ExtractAnimations()
	{
		foreach(var selected in Selection.objects)
		{
			string assetPath = AssetDatabase.GetAssetPath(selected);
			string ext = Path.GetExtension(assetPath).ToLower();

			if(!IsModelExtension(ext))
				continue;

			Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
			List<AnimationClip> clipList = new List<AnimationClip>();
			foreach(var asset in assets)
			{
				if(asset is AnimationClip clip)
				{
					// Preview 또는 임시 파일 무시
					if(clip.name.StartsWith("__preview__"))
						continue;

					// FBX에서 자동 생성되는 루트모션용 빈 clip도 무시 (길이 0 등)
					if(Mathf.Approximately(clip.length, 0f))
						continue;

					clipList.Add(clip);
				}
			}
			bool isOne = clipList.Count == 1;
			foreach(var clip in clipList)
			{
				string directory = Path.GetDirectoryName(assetPath);
				string fileName = isOne
					? $"{selected.name}.anim"
					: $"{selected.name}.{clip.name}.anim";
				string outputPath = Path.Combine(directory,"_animClip", fileName).Replace("\\", "/");


				if(Directory.Exists(Path.Combine(directory, "_animClip")) == false)
				{
					Directory.CreateDirectory(Path.Combine(directory, "_animClip"));
				}
				if(File.Exists(outputPath))
				{
					File.Delete(outputPath);
					string metaPath = outputPath + ".meta";
					if(File.Exists(metaPath))
					{
						File.Delete(metaPath);
					}
				}

				AnimationClip newClip = new AnimationClip();
				EditorUtility.CopySerialized(clip, newClip);
				AssetDatabase.CreateAsset(newClip, outputPath);

				Debug.Log($"✅ {fileName} extracted to: {outputPath}");
			}
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private static bool IsModelExtension(string ext)
	{
		foreach(var modelExt in SupportedModelExtensions)
		{
			if(ext == modelExt)
				return true;
		}
		return false;
	}
}

#endif