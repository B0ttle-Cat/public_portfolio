using BC.OdccBase;

using TFSystem;

using UnityEngine;

namespace TFContent
{
	public class StartOdccGameObjectDataLoad : OdccBehaviorTarget
	{
		private SaveLoadObjectData saveLoadObjectData;
		protected override bool OnActionValid()
		{
			saveLoadObjectData = DataCarrier.GetSeparate<SaveLoadObjectData>().GetData<SaveLoadObjectData>(saveLoadObjectData);
			return saveLoadObjectData != null;
		}

		protected override async Awaitable OnActionUpdate()
		{
			if(saveLoadObjectData != null) saveLoadObjectData.LoadAllObject();
			await Awaitable.NextFrameAsync();
		}

		protected override void OnActionEnd()
		{
		}
	}
}