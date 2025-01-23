using System.Collections.Generic;

using BC.OdccBase;

using UnityEngine;
namespace TFContent
{
	public class InitBehaviorCommand : OdccBehaviorTarget
	{
		[SerializeField]
		private  List<GameObject> initList;
		protected override bool OnActionValid()
		{
			return true;
		}

		protected override async Awaitable OnActionUpdate()
		{
			int count = initList.Count;
			for(int i = 0 ; i < count ; i++)
			{
				initList[i].SetActive(true);
			}
		}

		protected override void OnActionEnd()
		{
			initList.Clear();
			initList = null;
		}
	}
}