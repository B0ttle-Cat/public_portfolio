using System.Collections.Generic;
using System.Linq;

using BC.Base;
using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;

using UnityEditor;
#endif

namespace BC.OdccBase
{
	public class ProjectileInitData : DataObject, IDataCopy
	{
		[Title("Projectile Common Data")]
		[MinValue(0.1f)]
		public float maxLifeTime = 10f;
		[MinValue(1)]
		public int maxHitCount = 1;
		[MinValue(0f)]
		public float moveSpeed = 30f;
		public UnitRelationType hitUnitRelationType;

		[TitleGroup("Projectile Hit Layer", subtitle: "( Projectile / Blocking / HitEffect ) Layer Filter", TitleAlignments.Split)]
		[HorizontalGroup("Projectile Hit Layer/H")]
		[CustomValueDrawer("ProjectileLayerFilter")]
		public LayerMask projectileLayer;
		[HorizontalGroup("Projectile Hit Layer/H")]
		[CustomValueDrawer("BlockingLayerFilter")]
		public LayerMask blockingLayer;
		[HorizontalGroup("Projectile Hit Layer/H")]
		[CustomValueDrawer("HitEffectLayerFilter")]
		public LayerMask hitEffectLayer;
		DataObject IDataCopy.DataCopy()
		{
			return new ProjectileInitData {
				maxLifeTime = maxLifeTime,
				maxHitCount = maxHitCount,
				moveSpeed = moveSpeed,
				hitUnitRelationType = hitUnitRelationType,
				projectileLayer = projectileLayer,
				blockingLayer = blockingLayer,
				hitEffectLayer = hitEffectLayer,
			};
		}

#if UNITY_EDITOR
		private LayerMask ProjectileLayerFilter(LayerMask value)
		{
			List<string> layerNames = new List<string>();
			for(int i = 0 ; i < 32 ; i++)
			{
				string name = LayerMask.LayerToName(i);
				if(string.IsNullOrWhiteSpace(name)) continue;
				layerNames.Add(name);
			}
			string layer = LayerMask.LayerToName(value.ToLayer());
			int selected = layerNames.FindIndex(value => value == layer);

			selected = SirenixEditorFields.Dropdown(selected, layerNames.ToArray());
			if(selected>=0 && selected < layerNames.Count)
			{
				value.value = 1 << LayerMask.NameToLayer(layerNames[selected]);
			}
			return value;
		}
		private LayerMask BlockingLayerFilter(LayerMask value, GUIContent label)
		{
			var hitLayerMask = projectileLayer.GetHitLayerMask();
			//value = SirenixEditorFields.LayerMaskField(label, value);
			var layers = hitLayerMask.ToLayers();
			int  selectMask = 0;
			string[] selectItem = new string[layers.Count];
			int length = layers.Count;
			if(length == 0) return value;
			for(int i = 0 ; i < length ; i++)
			{
				int layer = layers[i];
				if(value.HasLayer(layer))
				{
					selectMask |= 1 << i;
				}

				selectItem[i] = (LayerMask.LayerToName(layer));
			}
			Rect controlRect = EditorGUILayout.GetControlRect(label != null);
			selectMask = EditorGUI.MaskField(controlRect, selectMask, selectItem);
			int newLayerMask = 0;
			for(int i = 0 ; i < length ; i++)
			{
				if((selectMask & 1<<i) > 0)
				{
					newLayerMask |= 1 << layers[i];
				}
			}
			return newLayerMask;
		}
		private LayerMask HitEffectLayerFilter(LayerMask value, GUIContent label)
		{
			var hitLayerMask = projectileLayer.GetHitLayerMask();
			//value = SirenixEditorFields.LayerMaskField(label, value);
			var layers = hitLayerMask.ToLayers().Where(i=> !blockingLayer.HasLayer(i)).ToList();
			int  selectMask = 0;
			string[] selectItem = new string[layers.Count];
			int length = layers.Count;
			if(length == 0) return value;
			for(int i = 0 ; i < length ; i++)
			{
				int layer = layers[i];
				if(value.HasLayer(layer))
				{
					selectMask |= 1 << i;
				}

				selectItem[i] = (LayerMask.LayerToName(layer));
			}
			Rect controlRect = EditorGUILayout.GetControlRect(label != null);
			selectMask = EditorGUI.MaskField(controlRect, selectMask, selectItem);
			int newLayerMask = 0;
			for(int i = 0 ; i < length ; i++)
			{
				if((selectMask & 1<<i) > 0)
				{
					newLayerMask |= 1 << layers[i];
				}
			}
			return newLayerMask;
		}
#endif
	}
}
