using BC.ODCC;

using UnityEngine;
using UnityEngine.UI;
namespace TFContent
{
	public class SelectorDraw : ComponentBehaviour//, IOdccUpdate
	{
		[SerializeField] private Canvas drawSelectorCanvas;
		[SerializeField] private RectTransform drawRect;
		[SerializeField] private RectTransform drawPoint;
		[SerializeField] private Image pointImage;
		[SerializeField] private Sprite defaultSprite;
		[SerializeField] private Sprite includeSprite;
		[SerializeField] private Sprite excludeSprite;

		private bool drawFlag;
		private bool isDraw;
		private bool IsDraw {
			get => isDraw;
			set {
				if(isDraw != value)
				{
					drawSelectorCanvas.enabled = value;
				}
				isDraw = value;
			}
		}

		protected override void BaseStart()
		{
			drawSelectorCanvas.enabled = isDraw = drawFlag = false;
		}

		internal void DrawPoint(Vector2 startPoint, float minDragSize, bool includeFlag, bool excludeFlag)
		{
			if(!IsCheckDraw()) return;
			drawFlag = true;

			drawRect.sizeDelta = Vector2.zero;

			drawPoint.pivot = Vector2.one*0.5f;
			drawPoint.anchoredPosition = startPoint;
			drawPoint.sizeDelta = Vector2.one * minDragSize;

			if(pointImage == null) return;
			if(includeFlag == excludeFlag)
			{
				pointImage.sprite = defaultSprite;
			}
			else if(includeFlag)
			{
				pointImage.sprite = includeSprite;
			}
			else if(excludeSprite)
			{
				pointImage.sprite = excludeSprite;
			}
		}
		internal void DrawRect(Vector2 startPoint, Vector2 endPoint, Vector2 size, float minDragSize, float width, float height, bool includeFlag, bool excludeFlag)
		{
			if(!IsCheckDraw()) return;

			drawFlag = true;

			Vector2 pivot = Vector2.zero;
			pivot.x = size.x < 0 ? 1 : 0;
			pivot.y = size.y < 0 ? 1 : 0;
			drawRect.pivot = pivot;
			drawRect.anchoredPosition = startPoint;
			drawRect.sizeDelta = new Vector2(width, height);

			pivot.x = size.x < 0 ? 0 : 1;
			pivot.y = size.y < 0 ? 0 : 1;
			drawPoint.pivot = pivot;
			drawPoint.anchoredPosition = endPoint;
			drawPoint.sizeDelta = Vector2.one * minDragSize;

			if(pointImage == null) return;
			if(includeFlag == excludeFlag)
			{
				pointImage.sprite = defaultSprite;
			}
			else if(includeFlag)
			{
				pointImage.sprite = includeSprite;
			}
			else if(excludeSprite)
			{
				pointImage.sprite = excludeSprite;
			}
		}

		public void LateUpdate()
		{
			if(drawSelectorCanvas == null) return;
			if(drawFlag && IsCheckDraw())
			{
				drawFlag = false;
				IsDraw = true;
			}
			else
			{
				IsDraw = false;
			}
		}

		private bool IsCheckDraw()
		{
			if(drawRect == null || drawPoint == null) return false;
			return true;
		}
	}
}