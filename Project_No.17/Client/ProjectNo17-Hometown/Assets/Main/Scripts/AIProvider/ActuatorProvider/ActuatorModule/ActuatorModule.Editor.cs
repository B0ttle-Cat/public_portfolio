#if UNITY_EDITOR
using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

namespace BC.AIProvider
{
	public partial class ActuatorModule//.Editor
	{
		public void OnValidate()
		{
			if(!HasBasic) AddBasic();
			if(!HasEffect) AddEffect();
			if(!HasHitShape) AddHitShape();
			if(!HasPriority) AddPriority();
			if(!HasTarget) AddTarget();
		}

		private void AddModule<T>(ref T module) where T : ScriptableObject
		{
			if(module != null) return;

			string assetPath = AssetDatabase.GetAssetPath(this);
			if(string.IsNullOrEmpty(assetPath))
			{
				return;
			}

			Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
			foreach(Object obj in subAssets)
			{
				if(obj is T effect)
				{
					module = effect;
					EditorUtility.SetDirty(this);
					AssetDatabase.SaveAssets();
					return;
				}
			}

			module = CreateInstance<T>();
			module.name = module.GetType().Name;

			AssetDatabase.AddObjectToAsset(module, this);

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}
		private void RemoveModule<T>(ref T module) where T : ScriptableObject
		{
			module = null;

			string assetPath = AssetDatabase.GetAssetPath(this);
			if(string.IsNullOrEmpty(assetPath))
			{
				return;
			}

			Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
			foreach(Object subAsset in subAssets)
			{
				if(subAsset is T)
				{
					AssetDatabase.RemoveObjectFromAsset(subAsset);
					DestroyImmediate(subAsset, true);
				}
			}

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}
		private void AddBasic()
		{
			AddModule<ActuatorBasic>(ref basic);
		}
		private void RemoveBasic()
		{
			RemoveModule<ActuatorBasic>(ref basic);
		}
		//[ButtonGroup("Effect/Button"), Button("Add Or New")]
		private void AddEffect()
		{
			AddModule<ActuatorEffect>(ref effect);
		}
		[ButtonGroup("SubModule")]
		private void RemoveEffect()
		{
			RemoveModule<ActuatorEffect>(ref effect);
		}
		//[ButtonGroup("Priority/Button"), Button("Add Or New")]
		private void AddPriority()
		{
			AddModule<ActuatorPriorityOrder>(ref priority);
		}
		[ButtonGroup("SubModule")]
		private void RemovePriority()
		{
			RemoveModule<ActuatorPriorityOrder>(ref priority);
		}
		//[ButtonGroup("Target/Button"), Button("Add Or New")]
		private void AddTarget()
		{
			AddModule(ref target);
		}
		[ButtonGroup("SubModule")]
		private void RemoveTarget()
		{
			RemoveModule(ref target);
		}
		//[ButtonGroup("HitRange&Shape/Button"), Button("Add Or New")]
		private void AddHitShape()
		{
			AddModule(ref hitShape);
		}
		[ButtonGroup("SubModule")]
		private void RemoveHitShape()
		{
			RemoveModule(ref hitShape);
		}

		public void Editor_ShowPlayModeRangePreview(Vector3 pivotPosition, float pivotRadius)
		{
			HitShape.Editor_ShowPlayModeRangePreview(pivotPosition, pivotRadius);
		}
	}
}
#endif