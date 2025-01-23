﻿using TFSystem.UI;

using TMPro;

using UnityEngine;

namespace TFContent
{
	public class UILoadingHelpText : UIViewComponent, UIViewItem, UIBinding<string>
	{
		public TMP_Text text;

		[Multiline(2), SerializeField]
		private string selectText = "";

		string UIViewItem.ViewItemName { get; set; }

		void UIViewItem.SetupView()
		{
			text = GetComponent<TMP_Text>();
		}

		void UIViewItem.SetupValue()
		{
			if(text == null) return;
			text.text = selectText;
		}

		string UIBinding<string>.GetValue()
		{
			return selectText;
		}

		void UIBinding<string>.SetValue(string setValue, bool _interaction)
		{
			text.text = selectText = setValue;
		}
	}
}
