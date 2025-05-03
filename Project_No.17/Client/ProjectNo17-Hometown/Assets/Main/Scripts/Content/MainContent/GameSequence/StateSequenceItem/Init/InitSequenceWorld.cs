using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class InitSequenceWorld : GameSequenceItem
	{
		private WorldObjectData worldObjectData;

		protected override bool OnActionValid()
		{
#if UNITY_EDITOR
			int childCount = transform.childCount;
			for(int i = 0 ; i < childCount ; i++)
			{
				//if(transform.GetChild(i).gameObject.tag =="EditorOnly")
				{
					DestroyImmediate(transform.GetChild(i).gameObject);
					i--;
					childCount--;
				}
			}
#endif
			worldObjectData = DataCarrier.GetSeparate<InitGameContentLoader>().GetData<WorldObjectData>();
			return worldObjectData != null;
		}

		protected override async Awaitable OnActionUpdate()
		{
			await worldObjectData.LoadAllObject(ThisTransform);
		}

		protected override void OnActionEnd()
		{
			worldObjectData = null;
			DestroyThis();
		}
	}
}