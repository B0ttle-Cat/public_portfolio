using System;

using BC.Base;
using BC.ODCC;

using Shapes;

using UnityEditor;

using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace BC.Shape
{
	[ExecuteAlways]
	/// <see cref="Shapes.ImmediateModeShapeDrawer"/>
	public class ShapeRenderer : MonoBehaviour
	{
		void DrawShapesSRP(UnityEngine.Rendering.ScriptableRenderContext ctx, Camera cam)
		{
			switch(cam.cameraType)
			{
				case CameraType.Preview:
				case CameraType.Reflection:
					return; // Don't render in preview windows or in reflection probes in case we run this script in the editor
			}

			using(Draw.Command(cam))
			{
				try
				{
#if UNITY_EDITOR
					if(UnityEditor.Selection.activeObject != null && UnityEditor.Selection.activeObject != UnityEditor.Selection.activeGameObject)
					{
						if(UnityEditor.Selection.activeObject is IShapeRenderer_EditorScriptableObject selectRender)
						{
							selectRender.Editor_IOnDrawShapes(cam);
						}
					}
					else if(UnityEditor.Selection.activeGameObject != null)
					{
						var renderList = UnityEditor.Selection.activeGameObject.GetComponentsInChildren<IShapeRenderer_EditorScriptableObject>();
						foreach(var render in renderList)
						{
							render.Editor_IOnDrawShapes(cam);
						}
					}

					if(!EditorApplication.isPlaying)
					{
						var monoList = Object.FindObjectsByType<ComponentBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
						int length = monoList.Length;
						for(int i = 0 ; i < length ; i++)
						{
							if(monoList[i].TryGetComponent<IShapeRenderer>(out var render))
							{
								render.IOnDrawShapes(cam);
							}
						}

					}
					else
#endif
						EventManager.Call<IShapeRenderer>(render => render.IOnDrawShapes(cam));
				}
				catch(Exception ex) { Debug.LogException(ex); }
			}
		}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void OnBeforeSceneLoad()
		{
			Draw.ResetAllDrawStates();
		}

		public virtual void OnEnable() => UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += DrawShapesSRP;
		public virtual void OnDisable() => UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= DrawShapesSRP;
	}
}
