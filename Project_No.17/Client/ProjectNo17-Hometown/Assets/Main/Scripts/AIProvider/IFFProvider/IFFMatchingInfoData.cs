using System;
using System.Collections.Generic;
using System.Linq;

using BC.OdccBase;

using Sirenix.OdinInspector;

using Unity.Mathematics;

using UnityEngine;

namespace BC.AIProvider
{
	public interface IGetIFFMatchingType
	{
		public TeamFriendshipType GetTeamFriendship(int teamA, int teamB);
		public TeamFriendshipType GetTeamFriendship(Vector2Int MatchNumber) => GetTeamFriendship(MatchNumber.x, MatchNumber.y);
		public TeamFriendshipType GetTeamFriendship(IFFProvider.IFFData teamA, IFFProvider.IFFData teamB) => GetTeamFriendship(teamA.TeamID, teamB.TeamID);
		public TeamFriendshipType GetTeamFriendship(IFFProvider teamA, IFFProvider teamB) => GetTeamFriendship(teamA.TeamID, teamB.TeamID);

		public UnitRelationType GetUnitRelation(int teamA, int teamB, int unitA, int unitB);
		public UnitRelationType GetUnitRelation(IFFProvider.IFFData teamA, IFFProvider.IFFData teamB) => GetUnitRelation(teamA.TeamID, teamB.TeamID, teamA.UnitID, teamB.UnitID);
		public UnitRelationType GetUnitRelation(IFFProvider teamA, IFFProvider teamB) => GetUnitRelation(teamA.IFFID, teamB.IFFID);
	}

	[CreateAssetMenu(fileName = "IFFMatchingInfoData", menuName = "BC/IFFMatchingInfoData")]
	public class IFFMatchingInfoData : ScriptableObject, IGetIFFMatchingType
	{
		[Serializable]
		public struct IFFMatch
		{
			[HorizontalGroup, HideLabel, InlineProperty]
			public Vector2Int MatchNumber;
			[HorizontalGroup, HideLabel, EnumToggleButtons]
			public TeamFriendshipType Friendship;
			public int2 MatchInt2 => new int2(MatchNumber.x, MatchNumber.y);
		}
		[ValueDropdown("IFFTeamList")]
		public int userControlTeamID;
		[ListDrawerSettings(ShowFoldout = false, HideAddButton = true, HideRemoveButton = true, ShowPaging = false)]
		public IFFMatch[] IFFMatchingInfo;

		public void OnValidate()
		{

		}
#if UNITY_EDITOR
		[ShowInInspector, InlineButton("CreateList"), PropertyOrder(-1)]
		[Range(1,10)]
		private int teamCount = 1;
		private void CreateList()
		{
			List<IFFMatch> oldList = IFFMatchingInfo.ToList();

			var newList = new List<IFFMatch>();
			for(int x = 0 ; x < teamCount ; x++)
			{
				for(int y = 0 ; y < teamCount ; y++)
				{
					if(x >= y) continue;
					Vector2Int MatchNumber = new Vector2Int(x,y);
					TeamFriendshipType friendshipType = TeamFriendshipType.Neutral;
					int index = oldList.FindIndex(i => i.MatchNumber == MatchNumber);
					if(index >= 0)
					{
						friendshipType = oldList[index].Friendship;
					}

					newList.Add(new IFFMatch {
						MatchNumber = MatchNumber,
						Friendship = friendshipType,
					});
				}
			}
			IFFMatchingInfo = newList.ToArray();
		}
		private ValueDropdownList<int> IFFTeamList()
		{
			var list = new ValueDropdownList<int>();

			var infoList = IFFMatchingInfo;
			int max = infoList.Select(i => i.MatchNumber.y).Max();
			for(int i = 0 ; i < max ; i++)
			{
				list.Add(i);
			}
			return list;
		}
#endif
		public TeamFriendshipType GetTeamFriendship(int teamA, int teamB)
		{
			if(teamA == teamB) return TeamFriendshipType.Team;

			if(teamA > teamB)
			{
				int y = teamB;
				teamB = teamA;
				teamA = y;
			}

			int length = IFFMatchingInfo.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var matchNumber = IFFMatchingInfo[i].MatchNumber;
				if(matchNumber.x == teamA && matchNumber.y == teamB)
				{
					return IFFMatchingInfo[i].Friendship;
				}
			}
			return 0;
		}

		public UnitRelationType GetUnitRelation(int teamA, int teamB, int unitA, int unitB)
		{
			TeamFriendshipType friendship = GetTeamFriendship(teamA, teamB);
			return friendship switch {
				TeamFriendshipType.Enemy => OdccBase.UnitRelationType.Enemy,
				TeamFriendshipType.Neutral => OdccBase.UnitRelationType.Neutral,
				TeamFriendshipType.Ally => OdccBase.UnitRelationType.Ally,
				TeamFriendshipType.Team => unitA == unitB ? OdccBase.UnitRelationType.Self : OdccBase.UnitRelationType.Team,
				_ => OdccBase.UnitRelationType.Neutral,
			};
		}
	}
}
