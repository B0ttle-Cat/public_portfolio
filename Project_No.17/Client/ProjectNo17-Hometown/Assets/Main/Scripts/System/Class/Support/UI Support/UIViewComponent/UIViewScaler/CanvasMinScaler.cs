using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

namespace TFSystem
{
	public class CanvasMinScaler : MonoBehaviour
	{
		[SerializeField]
		private CanvasScaler canvasScaler;
		[SerializeField]
		private RectTransform[] minSizeRects;
		[SerializeField]
		private Vector2 minCanvasScale;
		private enum ClampRect
		{
			None,
			Width,
			Height,
		}
		[SerializeField]
		private ClampRect minClamp = ClampRect.None;
		private float retryTime = 5f;
		private float retryTimeMax = 5f;
		private void OnEnable()
		{
			retryTime = 0f;
		}

		private void Update()
		{
			if(canvasScaler == null)
			{
				enabled = false;
				return;
			}
			if(minClamp == ClampRect.None) return;
			if(retryTime > 0)
			{
				retryTime -= Time.deltaTime;
				return;
			}
			retryTime = retryTimeMax;

			CanvasUpdate();
		}
		[Button]
		private void CanvasUpdate()
		{
			if(canvasScaler == null)
			{
				return;
			}

			int length = minSizeRects==null ? 0 : minSizeRects.Length;
			if(length == 0) return;

			Vector2 vector2 = canvasScaler.referenceResolution;
			if(minClamp == ClampRect.Width)
			{
				float minClmapSize = 0f;
				for(int i = 0 ; i < length ; i++)
				{
					minClmapSize  += minSizeRects[i].rect.width;
				}
				if(Mathf.Abs(vector2.x - minClmapSize) < 0.01f) return;
				if(minCanvasScale.x > minClmapSize) minClmapSize = minCanvasScale.x;
				vector2.x = minClmapSize;
			}
			else if(minClamp == ClampRect.Height)
			{
				float minClmapSize = 0f;
				for(int i = 0 ; i < length ; i++)
				{
					minClmapSize  += minSizeRects[i].rect.height;
				}
				if(Mathf.Abs(vector2.y - minClmapSize) < 0.01f) return;
				if(minCanvasScale.y > minClmapSize) minClmapSize = minCanvasScale.y;
				vector2.y = minClmapSize;
			}
			canvasScaler.referenceResolution = vector2;
		}
	}
}
