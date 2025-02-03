using BC.ODCC;
using BC.Shape;

using Shapes;

using UnityEngine;
namespace TFContent
{
	public class SelectingHandle : ComponentBehaviour, IShapeRenderer
	{
		public bool IsPreview { get; set; }
		public float radius;

		protected override void BaseAwake()
		{
			base.BaseAwake();
			transform.localPosition = Vector3.up * 0.001f;
			transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
			transform.localScale = Vector3.one;
			radius = 0.5f;

			gameObject.layer = LayerMask.NameToLayer("RenderSelecting");
		}

		void IShapeRenderer.OnDrawShapes(Camera cam)
		{
			Draw.ResetAllDrawStates();
			Draw.Matrix = transform.localToWorldMatrix;
			Draw.Color = Color.green;
			Draw.UseDashes = IsPreview;
			Draw.Radius = 0.55f;
			Draw.Thickness = 0.1f;
			var dashStyle = DashStyle.defaultDashStyleRing;
			dashStyle.type = DashType.Rounded;
			//dashStyle.space = DashSpace.FixedCount;
			//dashStyle.snap = DashSnapping.Tiling;
			//dashStyle.size = 16f;
			//dashStyle.offset = 0f;
			//dashStyle.spacing = 0.5f;
			//dashStyle.shapeModifier = 1;
			Draw.DashStyle = dashStyle;
			Draw.Ring(radius);
		}
	}
}