using TFContent.SaveLoad;

using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class StartGameObjectDataLoad : GameSequenceItem
	{
		private SaveLoadObjectData saveLoadObjectData;

		protected override bool OnActionValid()
		{
			saveLoadObjectData = DataCarrier.GetSeparate<InitGameContentLoader>().GetData<SaveLoadObjectData>(saveLoadObjectData);
			return true;
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(saveLoadObjectData != null) saveLoadObjectData.LoadAllObject();
			await Awaitable.NextFrameAsync();
		}

		protected override void OnActionEnd()
		{
			DestroyThis();
		}
	}
}