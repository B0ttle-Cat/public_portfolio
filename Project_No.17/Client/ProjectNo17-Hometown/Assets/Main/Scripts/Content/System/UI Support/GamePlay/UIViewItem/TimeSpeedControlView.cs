using System;

using BC.Base;

using Sirenix.OdinInspector;

using TFSystem.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace TFContent
{
	[Serializable]
	public struct TimeSpeedUIData
	{
		public bool pauseFlag;
		public int speedIndex;
	}
	[Serializable]
	public class TimeSpeedControlView : UIViewItem, UIBinding<TimeSpeedUIData>, UIEvent_OnChangeValue<float>
	{
		public string ViewItemName { get; set; }
		[SerializeField]
		public Toggle pauseButton;
		[SerializeField]
		public Button changeSpeed;
		[SerializeField]
		public TMP_Text speedText;
		[SerializeField]
		public Image speedImage;
		[SerializeField, TableList]
		private TimeSpeedViewData[] timeSpeedViewData;
		[SerializeField, InlineProperty,HideLabel]
		private TimeSpeedUIData timeSpeedUIData;
		public bool interaction { get; set; }
		public ObserverValue<float> onValueChanged { get; set; } = new ObserverValue<float>();


		[SerializeField, ReadOnly]
		private float lastSpeed;

		[Serializable]
		private struct TimeSpeedViewData
		{
			[Range(0.1f,3f)]
			public float speed;
			public string text;
			public Sprite texture;
		}
		[Button("InitView")]
		public void Setup() { SetupView(); SetupValue(); }
		public void SetupView()
		{
			pauseButton.isOn = timeSpeedUIData.pauseFlag;
			timeSpeedUIData.speedIndex %= timeSpeedViewData.Length;

			pauseButton.onValueChanged.RemoveAllListeners();
			pauseButton.onValueChanged.AddListener((value) => {
				if(!interaction) return;

				timeSpeedUIData.pauseFlag = value;

				OnChangeTimeSpeed();
				SetupValue();
			});

			changeSpeed.onClick.RemoveAllListeners();
			changeSpeed.onClick.AddListener(() => {
				if(!interaction) return;

				timeSpeedUIData.speedIndex = (timeSpeedUIData.speedIndex+1) % timeSpeedViewData.Length;

				OnChangeTimeSpeed();
				SetupValue();
			});
		}

		public void SetupValue()
		{
			bool isPause = timeSpeedUIData.pauseFlag;
			int speedIndex = timeSpeedUIData.speedIndex % timeSpeedViewData.Length;
			var selectSpeed = timeSpeedViewData[speedIndex];

			if(isPause)
			{
				if(speedText != null) speedText.text = $"{selectSpeed.text}";
				if(speedImage != null) speedImage.sprite = selectSpeed.texture;
				lastSpeed = 0f;
			}
			else
			{
				if(speedText != null) speedText.text = $"{selectSpeed.text}";
				if(speedImage != null) speedImage.sprite = selectSpeed.texture;
				lastSpeed = selectSpeed.speed;
			}
		}

		TimeSpeedUIData UIBinding<TimeSpeedUIData>.GetValue()
		{
			return timeSpeedUIData;
		}

		void UIBinding<TimeSpeedUIData>.SetValue(TimeSpeedUIData setValue, bool? _interaction)
		{
			var old = interaction;
			interaction = _interaction ?? interaction;
			Set();
			interaction = old;

			void Set()
			{
				timeSpeedUIData = setValue;

				bool temp = pauseButton.interactable;
				pauseButton.interactable = false;
				pauseButton.isOn = setValue.pauseFlag;
				pauseButton.interactable = temp;

				OnChangeTimeSpeed();
				SetupValue();
			}
		}

		private void OnChangeTimeSpeed()
		{
			if(!interaction) return;

			float changeSpeed = 0f;
			if(timeSpeedUIData.pauseFlag)
			{
				changeSpeed = 0f;
			}
			else
			{
				int speedIndex = timeSpeedUIData.speedIndex % timeSpeedViewData.Length;
				var selectSpeed = timeSpeedViewData[speedIndex];
				changeSpeed = selectSpeed.speed;
			}

			if(lastSpeed != changeSpeed)
			{
				onValueChanged.Value = changeSpeed;
			}
			lastSpeed = changeSpeed;
		}
	}
}
