using TFSystem;
using TFSystem.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
namespace TFContent
{
	public class GamePlayTopMenuViewModel : UIViewModelComponent
	{
		[Header("Left")]
		[SerializeField] private Button openMenuViewButton;
		[SerializeField] private Button openMissionViewButton;
		[SerializeField] private Button openMapViewButton;
		[Header("Center")]
		[SerializeField] private StatusBarViewComponent materialBar;
		[SerializeField] private StatusBarViewComponent batteryBar;
		[SerializeField] private TMP_Text playTimeText;
		[Header("Right")]
		[SerializeField] private TimeSpeedControlViewComponent timeSpeedControl;
		protected override void AwakeUIView(ref ViewItemSetter viewItemSetter)
		{
			openMenuViewButton.onClick.AddListener(async () => await WaitOnAction(OpenMenuViewButton));
			openMissionViewButton.onClick.AddListener(async () => await WaitOnAction(OpenMissionViewButto));
			openMapViewButton.onClick.AddListener(async () => await WaitOnAction(OpenMapViewButton));


			viewItemSetter.Add(materialBar.UIViewItem, nameof(materialBar));
			viewItemSetter.Add(batteryBar.UIViewItem, nameof(batteryBar));

			viewItemSetter.Add(timeSpeedControl.UIViewItem, nameof(timeSpeedControl));
		}
		private async Awaitable OpenMenuViewButton()
		{
			if(ThisContainer.TryGetObject<IUIViewController<GamePlayViewState>>(out var view))
			{
				await view.OnChangeViewState(GamePlayViewState.OpenMenuView);
			}
		}
		private async Awaitable OpenMissionViewButto()
		{
		}
		private async Awaitable OpenMapViewButton()
		{
		}
	}
}
