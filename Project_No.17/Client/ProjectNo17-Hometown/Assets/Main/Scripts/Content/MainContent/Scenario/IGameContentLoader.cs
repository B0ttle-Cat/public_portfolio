using BC.ODCC;

using TFContent.SaveLoad;
namespace TFContent
{
	public interface IGameContentLoader : IOdccComponent
	{
		WorldObjectData WorldObjectData { get; }
		CharacterObjectData CharacterObjectData { get; }
		EpisodeObjectData EpisodeObjectData { get; }
		IFFMatchingObjectData IFFMatchingObjectData { get; }
		SaveLoadObjectData SaveLoadObjectData { get; }
	}
}