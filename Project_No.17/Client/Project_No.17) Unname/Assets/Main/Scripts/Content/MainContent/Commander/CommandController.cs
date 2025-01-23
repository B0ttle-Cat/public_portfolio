using System.Collections.Generic;
using System.Linq;

using BC.ODCC;
namespace TFContent
{
	public class CommandController : ComponentBehaviour, ISelectorCommandListener
	{
		private List<ObjectBehaviour> playableCharacter;
		private List<ObjectBehaviour> selectCharacter;

		protected override void BaseAwake()
		{
			playableCharacter = new List<ObjectBehaviour>();
			selectCharacter = new List<ObjectBehaviour>();
		}

		public void GetSelectListUpdate(List<ObjectBehaviour> targets)
		{
			selectCharacter = targets.Select(i => i).ToList();
		}

		public List<ObjectBehaviour> GetSelectList()
		{
			return selectCharacter.Select(i => i).ToList();
		}

		public List<ObjectBehaviour> GetUnselectList()
		{
			return playableCharacter.Where(i => !selectCharacter.Contains(i)).ToList();
		}

		public void GetSelectTargetAndState(int number, out ObjectBehaviour selectingTarget, out bool currentSelectState)
		{
			currentSelectState = false;
			selectingTarget = null;

			int index = number%=10 == 0 ? 10 : number - 1;
			if(index < 0) return;
			if(index >= playableCharacter.Count) return;

			var select = playableCharacter[index];
			currentSelectState = selectCharacter.Contains(select);
		}
	}
}