#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace BC.Actuator
{
	public partial class ActuatorModule//.Editor
	{
		private void Remove(ScriptableObject module)
		{
			if(module is ActuatorBasic)
			{
				RemoveBasic();
			}
			else if(module is ActuatorPriorityOrder)
			{
				RemovePriority();
			}
			else if(module is ActuatorTarget)
			{
				RemoveTarget();
			}
			else if(module is ActuatorHitShape)
			{
				RemoveHitShape();
			}
			else if(module is ActuatorPlayAnimator)
			{
				RemoveAnimator();
			}
			else if(module is ActuatorFormulaItem)
			{
				RemoveFormula();
			}
			else if(module is ActuatorInstance)
			{
				RemoveInstance();
			}
		}
		private void _AddModule<T>(ref T module, string name = null) where T : ScriptableObject
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
			module.name = string.IsNullOrWhiteSpace(name) ? module.GetType().Name : name;

			AssetDatabase.AddObjectToAsset(module, this);

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}
		private void _RemoveModule<T>(ref T module) where T : ScriptableObject
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
			_AddModule<ActuatorBasic>(ref basic);
			basic.ActuatorName = base.name;
		}
		private void RemoveBasic()
		{
			_RemoveModule<ActuatorBasic>(ref basic);
		}
		private void AddPriority()
		{
			_AddModule<ActuatorPriorityOrder>(ref priority);
		}
		private void RemovePriority()
		{
			_RemoveModule<ActuatorPriorityOrder>(ref priority);
		}
		private void AddTarget()
		{
			_AddModule(ref target);
		}
		private void RemoveTarget()
		{
			_RemoveModule(ref target);
		}
		private void AddHitShape()
		{
			_AddModule(ref hitShape);
		}
		private void RemoveHitShape()
		{
			_RemoveModule(ref hitShape);
		}

		public void Editor_ShowPlayModeRangePreview(Vector3 pivotPosition, float pivotRadius)
		{
			HitShape.Editor_ShowPlayModeRangePreview(pivotPosition, pivotRadius);
		}

		private void AddAnimator()
		{
			_AddModule<ActuatorPlayAnimator>(ref animator, "ActuatorAnimator");
		}
		private void RemoveAnimator()
		{
			_RemoveModule<ActuatorPlayAnimator>(ref animator);
		}
		private void AddFormula()
		{
			_AddModule<ActuatorFormulaItem>(ref formula, nameof(ActuatorFormula));
		}
		private void RemoveFormula()
		{
			_RemoveModule<ActuatorFormulaItem>(ref formula);
		}
		private void AddInstance()
		{
			_AddModule(ref instance);
		}
		private void RemoveInstance()
		{
			_RemoveModule<ActuatorInstance>(ref instance);
		}

		private void Copy(ScriptableObject scriptableObject)
		{
			GUIUtility.systemCopyBuffer = EditorJsonUtility.ToJson(scriptableObject);
		}
		private void Paste(ScriptableObject scriptableObject)
		{
			EditorJsonUtility.FromJsonOverwrite(GUIUtility.systemCopyBuffer, scriptableObject);
		}
	}
}
#endif