using BC.OdccBase;

using UnityEngine;

namespace BC.AIProvider
{
	public interface IGetIFFMatchingType
	{
		public int UserControlTeamID { get; }
		public TeamFriendshipType GetTeamFriendship(int teamA, int teamB);
		public TeamFriendshipType GetTeamFriendship(Vector2Int MatchNumber) => GetTeamFriendship(MatchNumber.x, MatchNumber.y);
		public TeamFriendshipType GetTeamFriendship(IFFProvider.IFFData teamA, IFFProvider.IFFData teamB) => GetTeamFriendship(teamA.TeamID, teamB.TeamID);
		public TeamFriendshipType GetTeamFriendship(IFFProvider teamA, IFFProvider teamB) => GetTeamFriendship(teamA.TeamID, teamB.TeamID);

		public UnitRelationType GetUnitRelation(int teamA, int teamB, int unitA, int unitB);
		public UnitRelationType GetUnitRelation(IFFProvider.IFFData teamA, IFFProvider.IFFData teamB) => GetUnitRelation(teamA.TeamID, teamB.TeamID, teamA.UnitID, teamB.UnitID);
		public UnitRelationType GetUnitRelation(IFFProvider teamA, IFFProvider teamB) => GetUnitRelation(teamA.IFFID, teamB.IFFID);
	}
}
