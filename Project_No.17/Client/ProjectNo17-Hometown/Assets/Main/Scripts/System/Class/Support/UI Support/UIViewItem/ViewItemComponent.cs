using BC.ODCC;

using TFSystem.UI;
namespace TFSystem
{
	public abstract class ViewItemComponent<TViewItem> : ComponentBehaviour where TViewItem : UIViewItem
	{
		public abstract TViewItem UIViewItem { get; set; }
	}
}