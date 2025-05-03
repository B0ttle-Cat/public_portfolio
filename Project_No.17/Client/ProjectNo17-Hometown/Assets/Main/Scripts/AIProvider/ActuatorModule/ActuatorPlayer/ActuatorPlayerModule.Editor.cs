//#if UNITY_EDITOR
//using Sirenix.OdinInspector;

//using UnityEditor;

//using UnityEngine;

//namespace BC.AIProvider
//{
//	public partial class ActuatorPlayerModule // Editor
//	{
//		public void OnValidate()
//		{
//			if(!HasAnimator) AddAnimator();

//		}

//		private void AddModule<T>(ref T module) where T : ScriptableObject
//		{
//			if(module != null) return;

//			string assetPath = AssetDatabase.GetAssetPath(this);
//			if(string.IsNullOrEmpty(assetPath))
//			{
//				return;
//			}

//			Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
//			foreach(Object obj in subAssets)
//			{
//				if(obj is T effect)
//				{
//					module = effect;
//					EditorUtility.SetDirty(this);
//					AssetDatabase.SaveAssets();
//					return;
//				}
//			}

//			module = CreateInstance<T>();
//			module.name = module.GetType().Name;

//			AssetDatabase.AddObjectToAsset(module, this);

//			EditorUtility.SetDirty(this);
//			AssetDatabase.SaveAssets();
//		}
//		private void RemoveModule<T>(ref T module) where T : ScriptableObject
//		{
//			module = null;

//			string assetPath = AssetDatabase.GetAssetPath(this);
//			if(string.IsNullOrEmpty(assetPath))
//			{
//				return;
//			}

//			Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
//			foreach(Object subAsset in subAssets)
//			{
//				if(subAsset is T)
//				{
//					AssetDatabase.RemoveObjectFromAsset(subAsset);
//					DestroyImmediate(subAsset, true);
//				}
//			}

//			EditorUtility.SetDirty(this);
//			AssetDatabase.SaveAssets();
//		}
//		[TitleGroup("ButtonGroup", order: 999)]
//		[ResponsiveButtonGroup("ButtonGroup/B", UniformLayout = true)]
//		private void AddAnimator()
//		{
//			AddModule<ActuatorPlayAnimator>(ref animator);
//		}
//		[ResponsiveButtonGroup("ButtonGroup/B")]
//		private void RemoveAnimator()
//		{
//			RemoveModule<ActuatorPlayAnimator>(ref animator);
//		}
//	}
//}
//#endif