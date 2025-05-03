using Sirenix.OdinInspector;

using TFSystem;
namespace TFContent
{
	public class TimeSpeedControlViewComponent : ViewItemComponent<TimeSpeedControlView>
	{
		[UnityEngine.SerializeField, InlineProperty, HideLabel]
		private TimeSpeedControlView uiViewItem;
		public override TimeSpeedControlView UIViewItem { get => uiViewItem; set => uiViewItem = value; }
	}
}