using System;

using UnityEngine;
using UnityEngine.UI;

namespace TFSystem
{
	public class HorizontalGroupLeftRightSpace : HorizontalOrVerticalLayoutGroup
	{
		public RectTransform leftRect;
		public RectTransform leftSpace;
		public RectTransform centerRect;
		public RectTransform rightSpace;
		public RectTransform rightRect;

		public void CalculateLeftRightSpace()
		{
			try
			{
				float width = rectTransform.rect.width;
				width -= centerRect.rect.width;

				float leftWidth = width*0.5f;
				float rightWidth = width*0.5f;
				leftWidth -= leftRect.rect.width;
				rightWidth -= rightRect.rect.width;
				if(leftWidth < 0f && rightWidth < 0f)
				{
					leftWidth = 0f;
					rightWidth = 0f;
				}
				else if(leftWidth < 0f)
				{
					rightWidth += leftWidth;
					if(rightWidth < 0f) rightWidth = 0f;
					leftWidth = 0f;
				}
				else if(rightWidth < 0f)
				{
					leftWidth += rightWidth;
					if(leftWidth < 0f) leftWidth = 0f;
					rightWidth = 0f;
				}
				Vector2 sizeDelta = leftSpace.sizeDelta;
				sizeDelta.x = leftWidth;
				leftSpace.sizeDelta = sizeDelta;

				sizeDelta = rightSpace.sizeDelta;
				sizeDelta.x = rightWidth;
				rightSpace.sizeDelta = sizeDelta;
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}

			base.CalculateLayoutInputHorizontal();
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void CalculateLayoutInputHorizontal()
		{
			CalculateLeftRightSpace();
			base.CalculateLayoutInputHorizontal();
			CalcAlongAxis(0, false);
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void CalculateLayoutInputVertical()
		{
			CalcAlongAxis(1, false);
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void SetLayoutHorizontal()
		{
			SetChildrenAlongAxis(0, false);
		}

		/// <summary>
		/// Called by the layout system. Also see ILayoutElement
		/// </summary>
		public override void SetLayoutVertical()
		{
			SetChildrenAlongAxis(1, false);
		}
	}
}
