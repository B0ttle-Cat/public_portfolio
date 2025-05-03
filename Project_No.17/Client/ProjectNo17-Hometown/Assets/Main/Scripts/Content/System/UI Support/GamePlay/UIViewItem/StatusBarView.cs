using System;
using System.Linq;

using BC.Base;

using Sirenix.OdinInspector;

using TFSystem.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace TFContent
{
	[Serializable]
	public struct StatusBarViewData
	{
		[ValueDropdown("Editor_StringDropdownList",AppendNextDrawer = true)]
		public string ViewLabelText;
		[ValueDropdown("Editor_StringDropdownList",AppendNextDrawer = true)]
		public string ViewValueText;

		public float BarValue {
			get {
				float value = 0f;
				int length = BarValues == null ? 0 : BarValues.Length;
				for(int i = 0 ; i < length ; i++)
				{
					float barValue = BarValues[i];
					if(barValue<0f) barValue = 0f;
					value += barValue;
				}
				return value;
			}
		}
		[ReadOnly]
		public Vector2 BarMinMaxValue;
		[ReadOnly]
		public float[] BarValues;

#if UNITY_EDITOR
		ValueDropdownList<string> Editor_StringDropdownList() => ConstString.Editor_StringDropdownList();
#endif
	}

	[Serializable]
	public class StatusBarView : UIViewItem, UIBinding<StatusBarViewData>
	{
		public string ViewItemName { get; set; }

		[InfoBox("BarBoardRect 하위 UI 구성 규칙 : 모두 LayoutElement가 있어야 함", InfoMessageType.None)]
		[SerializeField] private RectTransform barBoardRect;
		private TMPro.TMP_Text labelText;
		private TMPro.TMP_Text valueText;
		private LayoutElement[] layoutElement;
		[SerializeField, InlineProperty, HideLabel] private StatusBarViewData statusBarViewData;
		public void SetupView()
		{
			labelText = null;
			valueText = null;
			var layoutElementList = barBoardRect.GetComponentsInChildren<LayoutElement>(true).ToList();
			for(int i = 0 ; i < layoutElementList.Count ; i++)
			{
				if(layoutElementList[i].transform.parent != barBoardRect.transform)
				{
					layoutElementList.RemoveAt(i--);
					continue;
				}
				else if(layoutElementList[i].TryGetComponent<TMP_Text>(out var text))
				{
					// Text 구성
					if(labelText == null)
					{
						labelText = text;
					}
					else
					{
						valueText = text;
					}
					layoutElementList[i].ignoreLayout = true;
					layoutElementList.RemoveAt(i--);
				}
				else if(i == layoutElementList.Count - 1)
				{
					// 마지막 빈 공간
					layoutElementList[i].ignoreLayout = false;
					layoutElementList[i].minWidth = 0f;
					layoutElementList[i].preferredWidth = 0f;
					layoutElementList[i].flexibleWidth = float.MaxValue;
					layoutElementList.RemoveAt(i--);
				}
				else
				{
					// 일반적인 Bar 구성
					layoutElementList[i].ignoreLayout = false;
					layoutElementList[i].minWidth = 0f;
					layoutElementList[i].preferredWidth = 0f;
					layoutElementList[i].flexibleWidth = 1;
				}
			}
			labelText.text = "";
			valueText.text = "";
			layoutElement = layoutElementList.ToArray();
			int length = layoutElement.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var element = layoutElement[i];
				if(element != null)
				{
					element.minWidth = 0;
					element.preferredWidth = 0f;
					element.flexibleWidth = 1;
				}
			}

			statusBarViewData.BarValues = new float[layoutElement.Length];

		}
		public void SetupValue()
		{
			float value = statusBarViewData.BarValue;
			float[] values = statusBarViewData.BarValues;
			float minValue = statusBarViewData.BarMinMaxValue.x;
			float maxValue = statusBarViewData.BarMinMaxValue.y;
			if(minValue > maxValue)
			{
				minValue = statusBarViewData.BarMinMaxValue.y;
				maxValue = statusBarViewData.BarMinMaxValue.x;
			}
			labelText.text = statusBarViewData.ViewLabelText;
			valueText.text = string.Format(statusBarViewData.ViewValueText, value, maxValue);

			int length1 = values.Length;
			int length2 = layoutElement.Length;
			int length = length1 < length2 ? length1 : length2;
			float width = barBoardRect.rect.width;
			for(int i = 0 ; i < length ; i++)
			{
				var element = layoutElement[i];
				if(element != null)
				{
					element.minWidth = 0f;
					float valueRate = (values[i]-minValue)/(maxValue-minValue);
					element.preferredWidth = (valueRate>0f ? valueRate : 0f) * width;
					element.flexibleWidth = 1f;
				}
			}
		}

		StatusBarViewData UIBinding<StatusBarViewData>.GetValue()
		{
			return statusBarViewData;
		}

		void UIBinding<StatusBarViewData>.SetValue(StatusBarViewData setValue, bool? _interaction)
		{
			statusBarViewData = setValue;
			SetupValue();
		}
	}
}
