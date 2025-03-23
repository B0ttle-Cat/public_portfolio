using BC.OdccBase;

using UnityEngine;
namespace TFContent
{
	public class StartBehaviorCharacter : OdccBehaviorTarget
	{
		protected override bool OnActionValid()
		{
			return true;
		}
		protected override async Awaitable OnActionUpdate()
		{
			var characters = GetComponentsInChildren<CharacterObject>(true);
			int length = characters.Length;
			for(int i = 0 ; i < length ; i++)
			{
				characters[i].gameObject.SetActive(true);
			}
		}
		protected override void OnActionEnd()
		{
		}
	}
}