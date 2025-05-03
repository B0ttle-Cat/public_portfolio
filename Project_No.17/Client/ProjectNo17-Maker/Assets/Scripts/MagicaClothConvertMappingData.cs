#if UNITY_EDITOR
using System;
using System.IO;

using MagicaCloth2;

using Sirenix.OdinInspector;


using UnityEditor;

using UnityEngine;

[CreateAssetMenu(fileName = "MagicaClothConvertMappingData", menuName = "Scriptable Objects/MagicaClothConvertMappingData")]
public class MagicaClothConvertMappingData : ScriptableObject
{
	[TableList]
	public BoneMappingData[] boneMappingDatas = new BoneMappingData[0];
	[Serializable]
	public struct BoneMappingData
	{
		public string vrmName;
		public string mc2Name;
		public BoneType mc2BoneType;
		[ValueDropdown("GetMC2PresetName")]
		public string mc2PresetPath;
		public enum BoneType
		{
			BoneCloth = 1,
			BoneSpring = 10,
		}
		private ValueDropdownList<string> GetMC2PresetName()
		{
			ValueDropdownList<string> list = new ValueDropdownList<string>();
			string prefix = "MC2_Preset";
			var guidArray = AssetDatabase.FindAssets($"{prefix} t:{nameof(TextAsset)}");
			foreach(var guid in guidArray)
			{
				var filePath = AssetDatabase.GUIDToAssetPath(guid);
				if(filePath.EndsWith(".json") == false) continue;
				var text = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
				if(text == null) continue;

				var fname = Path.GetFileNameWithoutExtension(filePath);

				fname = fname.Replace(prefix, "");
				if(fname.StartsWith("_")) fname = fname.Remove(0, 1); // 頭の_は削除する

				list.Add(fname, filePath);
			}

			return list;
		}
	}
	[TableList]
	public ColliderMappingDatas[] colliderMappingDatas = new ColliderMappingDatas[0];
	[Serializable]
	public struct ColliderMappingDatas
	{
		public string vrmName;
		[VerticalGroup("ColliderType"), HideLabel]
		public ColliderType colliderType;
		[VerticalGroup("ColliderType"), HideLabel]
		[ShowIf("@colliderType == ColliderType.Capsule")]
		public MagicaCapsuleCollider.Direction capsuleDirection;
		public enum ColliderType
		{
			Sphere,
			Capsule,
		}
	}
}
#endif
