using System.Collections.Generic;
using System.Linq;

using BC.ODCC;
namespace TFContent
{
	public interface ISelectorCommandListener : IOdccObject
	{
		public List<ObjectBehaviour> GetSelectList();
		public List<ObjectBehaviour> GetUnselectList();
		public void GetSelectTargetAndState(int number, out ObjectBehaviour selectingTarget, out bool currentSelectState);

		public List<TComponent> GetSelectList<TComponent>() where TComponent : ComponentBehaviour
		{
			return GetSelectList().Select(i => i.ThisContainer.GetComponent<TComponent>()).Where(i => i != null).ToList();
		}
		public List<TComponent> GetUnselectList<TComponent>() where TComponent : ComponentBehaviour
		{
			return GetUnselectList().Select(i => i.ThisContainer.GetComponent<TComponent>()).Where(i => i != null).ToList();
		}
		public void GetSelectTargetAndState<TComponent>(int number, out TComponent selectingTarget, out bool currentSelectState) where TComponent : ComponentBehaviour
		{
			GetSelectTargetAndState(number, out ObjectBehaviour target, out currentSelectState);
			selectingTarget = target == null ? null : target.ThisContainer.GetComponent<TComponent>();
		}
	}
}