using Sirenix.OdinInspector;

using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class StatusBarViewComponent : ViewItemComponent<StatusBarView>
	{
		[SerializeField, InlineProperty, HideLabel]
		private StatusBarView uiViewItem;
		public override StatusBarView UIViewItem { get => uiViewItem; set => uiViewItem = value; }
	}
}
