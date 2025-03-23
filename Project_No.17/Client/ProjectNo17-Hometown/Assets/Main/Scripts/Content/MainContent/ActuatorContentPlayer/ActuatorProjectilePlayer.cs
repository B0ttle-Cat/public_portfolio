using BC.AIProvider;
using BC.Base;

using Sirenix.OdinInspector;

using UnityEngine;

using System.Linq;

using Sirenix.Utilities.Editor;

using System.Collections.Generic;

using BC.OdccBase;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TFContent
{
	public abstract class ProjectileActuatorPlayer<TProjectile> : ActuatorPlayer where TProjectile : ProjectileObject
	{
		//[ShowInInspector, DisplayAsString, EnableGUI, PropertyOrder(-999), HideLabel]
		private string TypeName => $"Projectile: {prefab.name}";
		[SerializeField, LabelText("@TypeName"), PropertyOrder(-999)]
		private TProjectile prefab;
#if UNITY_EDITOR
		[ShowInInspector, PropertyOrder(-998), HideLabel, PreviewField(100, Sirenix.OdinInspector.ObjectFieldAlignment.Center), ReadOnly]
		[ShowIf("@prefab != null")]
		private Object preview => prefab == null ? null : prefab.gameObject;
		protected override void OnValidate()
		{
			base.OnValidate();
		}
#endif
		[Title("Projectile Common Data")]
		[MinValue(0.1f)]
		public float maxLifeTime = 10f;
		[MinValue(1)]
		public int maxHitCount = 1;
		[MinValue(0f)]
		public float moveSpeed = 30f;
		public UnitRelationType hitUnitRelationType;

		[TitleGroup("Projectile Hit Layer Mask", subtitle: "( Projectile / Blocking / HitEffect ) Layer Filter", TitleAlignments.Split)]
		[HorizontalGroup("Projectile Hit Layer Mask/H")]
		[CustomValueDrawer("ProjectileLayerFilter")]
		public LayerMask projectileLayer;
		[HorizontalGroup("Projectile Hit Layer Mask/H")]
		[CustomValueDrawer("BlockingLayerFilter")]
		public LayerMask blockingLayer;
		[HorizontalGroup("Projectile Hit Layer Mask/H")]
		[CustomValueDrawer("HitEffectLayerFilter")]
		public LayerMask hitEffectLayer;

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
		protected bool SetPoolingProjectile()
		{
			return ProjectileManager.Manager.SetPoolingProjectile(prefab);
		}
		protected async Awaitable<TProjectile> InstanceProjectile(bool isActive = true)
		{
			return await ProjectileManager.Manager.InstanceProjectile<TProjectile>(prefab.name, isActive);
		}
		protected async Awaitable<TProjectile[]> InstanceProjectile(int count, bool isActive = true)
		{
			return await ProjectileManager.Manager.InstanceProjectile<TProjectile>(prefab.name, count, isActive);
		}
		protected void DestroyProjectile(ProjectileObject projectileObject)
		{
			ProjectileManager.Manager.DestroyProjectile(projectileObject);
		}
		protected void DestroyProjectile(ProjectileObject[] projectileObjects)
		{
			ProjectileManager.Manager.DestroyProjectile(projectileObjects);
		}

		public abstract void ProjectileHit(TProjectile projectileObject, ReactionTarget reactionTarget);
		public abstract void ShapeEffectHit(ReactionTarget[] reactionTarget);
		public abstract void OnProjectileDead(TProjectile deadProjectile);


		protected override void OnDestroy()
		{
			base.OnDestroy();
		}
	}
}
