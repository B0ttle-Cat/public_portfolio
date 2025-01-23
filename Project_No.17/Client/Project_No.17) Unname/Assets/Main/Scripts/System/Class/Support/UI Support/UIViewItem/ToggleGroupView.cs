﻿using System;
using System.Linq;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace TFSystem.UI
{
	[Serializable, InlineProperty, HideLabel]
	public class ToggleGroupView<T> : UIViewItem, UIBinding<T>, UIEvent_OnChangeValue<T>
	{
		public string ViewItemName { get; set; }

		[BoxGroup("View")]
		[SerializeField, HorizontalGroup("View/Item"), LabelText("Group"), LabelWidth(40)]
		private ToggleGroup toggleGroup;
		[SerializeField, HorizontalGroup("View/Item"), LabelText(" Icon"), LabelWidth(40)]
		private Image toggleIcon;
		[SerializeField, HorizontalGroup("View/Item"), LabelText(" Info"), LabelWidth(40)]
		private TMP_Text toggleInfo;
		[Serializable]
		public struct ToggleState
		{
			[HorizontalGroup("Toggle"),LabelText("Type"), LabelWidth(40)]
			public T type;
			[HorizontalGroup("Toggle"),LabelText(" Toggle"), LabelWidth(45)]
			public Toggle toggle;
			///////////////////////////////////////////////
			[Header("Checkmark")]
			[FoldoutGroup("Show Detail"), HorizontalGroup("Show Detail/Checkmark"), LabelWidth(100)]
			public Image checkmark;
			[ShowIf("checkmark"), FoldoutGroup("Show Detail"), HorizontalGroup("Show Detail/Checkmark"), HideLabel]
			public Sprite checkmarkImage;

			[Header("Label Text")]
			[FoldoutGroup("Show Detail"), HorizontalGroup("Show Detail/Label"), LabelWidth(100)]
			public TMP_Text label;
			[ShowIf("label"), FoldoutGroup("Show Detail"),HorizontalGroup("Show Detail/Label"), HideLabel]
			public string labelText;

			[Header("Sound Path")]
			[FoldoutGroup("Show Detail"), LabelWidth(100)]
			public string clickSoundPath;
			///////////////////////////////////////////////
			[Title("View Setter")]
			[FoldoutGroup("Show Detail"), LabelWidth(100)]
			public Sprite viewImage;
			[FoldoutGroup("Show Detail"), LabelWidth(100)]
			public string viewText;
		}
		[SerializeField]
		[ListDrawerSettings(ShowPaging = false, ShowFoldout = false, ShowItemCount = false, ShowIndexLabels = false)]
		private ToggleState[] itmes;
		public ToggleState[] ItemList { get => itmes; set => itmes=value; }

		[SerializeField, EnumPaging]
		private T initValue;

		public bool interaction { get; set; }
		public Action<T> onValueChanged { get; set; }

#if UNITY_EDITOR
		[Title("Init Preview")]
		[PreviewField(Alignment = ObjectFieldAlignment.Center), ShowInInspector, HideLabel]
		public Sprite PreviewIcon => itmes.Where(i => i.type.Equals(initValue)).Select(t => t.viewImage).FirstOrDefault();
		[DisplayAsString(Alignment = TextAlignment.Center), ShowInInspector, HideLabel, EnableGUI]
		public string PreviewText => itmes.Where(i => i.type.Equals(initValue)).Select(t => t.viewText).FirstOrDefault();

#endif
		public void Init() { SetupView(); SetupValue(); }
		public virtual void SetupView()
		{
			interaction = true;
			int length = itmes.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var state = itmes[i];
				var toggle = state.toggle;
				toggle.group = toggleGroup;

				if(state.checkmark != null)
				{
					state.checkmark.sprite = state.checkmarkImage;
					toggle.graphic = state.checkmark;
				}
				if(state.label != null)
				{
					state.label.text = state.labelText;
				}
			}
			toggleGroup.allowSwitchOff = false;

			for(int i = 0 ; i < length ; i++)
			{
				var state = itmes[i];
				state.toggle.onValueChanged.RemoveAllListeners();
				state.toggle.onValueChanged.AddListener(isOn => {
					if(!interaction) return;

					if(isOn)
					{
						if(toggleIcon) toggleIcon.sprite = state.viewImage;
						if(toggleInfo) toggleInfo.text = state.viewText;

						onValueChanged?.Invoke(state.type);
					}
				});
			}
		}
		public virtual void SetupValue()
		{
			SetValue(initValue);
		}

		public virtual T GetValue()
		{
			int length = itmes.Length;
			for(int i = 0 ; i < length ; i++)
			{
				if(itmes[i].toggle.isOn)
				{
					return itmes[i].type;
				}
			}
			return default;
		}
		public virtual void SetValue(T setValue, bool _interaction = true)
		{
			if(_interaction)
			{
				Set();
			}
			else
			{
				var old = interaction;
				interaction = false;
				Set();
				interaction = old;
			}
			void Set()
			{
				int length = itmes.Length;
				for(int i = 0 ; i < length ; i++)
				{
					var state = itmes[i];
					state.toggle.isOn = state.type.Equals(setValue);
				}
			}
		}
	}
}
