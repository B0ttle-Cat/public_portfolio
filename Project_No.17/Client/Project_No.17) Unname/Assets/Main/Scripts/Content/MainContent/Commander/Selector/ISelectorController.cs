using System.Collections.Generic;
using System.Linq;

using BC.ODCC;
namespace TFContent
{
	public interface ISelectorController : IOdccComponent
	{
		public void SelectPoint(SelectingTarget target, bool includeFlag, bool excludeFlag);
		public void SelectRect(HashSet<SelectingTarget> targets, bool includeFlag, bool excludeFlag);

		public void SelectPoint(ObjectBehaviour target, bool includeFlag, bool excludeFlag)
		{
			SelectPoint(target.GetComponent<SelectingTarget>(), includeFlag, excludeFlag);
		}
		public void SelectRect(HashSet<ObjectBehaviour> targets, bool includeFlag, bool excludeFlag)
		{
			var selects = targets.Select(i => i.GetComponent<SelectingTarget>()).Where(i=>i != null).ToHashSet();
			SelectRect(selects, includeFlag, excludeFlag);
		}
	}
}