using BC.ODCC;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;
namespace TFContent
{
	public class CharacterObject : ObjectBehaviour, ISaveLoadTarget
	{
		[ReadOnly]
		public int prefabIndex;


		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
			data.SaveValue(nameof(prefabIndex), prefabIndex);

			ISaveLoadTarget.SaveThisDataObject(ThisContainer, ref data);
		}

		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			if(data.LoadValue(nameof(prefabIndex), out int _prefabIndex))
			{
				prefabIndex = _prefabIndex;
			}

			ISaveLoadTarget.LoadThisDataObject(ThisContainer, in data);
		}
	}
}