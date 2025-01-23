using System;

using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class SelectingTarget : ComponentBehaviour
	{
		[SerializeField]
		[ColorUsage(true,true)]
		private Color color;

		private Shapes.Disc selectRenderer;

		public Action<bool> onSelect;
		protected override void BaseAwake()
		{
			GameObject renderer = new GameObject();
			renderer.name = "SelectRenderer";
			renderer.transform.SetParent(transform);
			renderer.transform.localPosition = Vector3.up * 0.1f;
			renderer.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
			renderer.transform.localScale = Vector3.one;

			selectRenderer = renderer.AddComponent<Shapes.Disc>();
			selectRenderer.Type = Shapes.DiscType.Ring;
			selectRenderer.Color = color;
			selectRenderer.RadiusSpace = Shapes.ThicknessSpace.Meters;
			selectRenderer.Radius = 0.55f;
			selectRenderer.ThicknessSpace = Shapes.ThicknessSpace.Meters;
			selectRenderer.Thickness= 0.1f;
		}
		protected override void BaseStart()
		{
			OnHideSelecting();
		}


		public void ShowPreviewSelecting()
		{
			OnShowSelecting(true);
		}
		public void ShowSelecting()
		{
			OnShowSelecting(false);
		}
		public void HideSelecting()
		{
			OnHideSelecting();
		}


		void OnShowSelecting(bool isPreview)
		{
			if(selectRenderer == null) return;
			selectRenderer.enabled = true;
			selectRenderer.Dashed = isPreview;

			onSelect?.Invoke(true);
		}
		void OnHideSelecting()
		{
			if(selectRenderer == null) return;
			selectRenderer.enabled = false;

			onSelect?.Invoke(false);
		}
	}
}