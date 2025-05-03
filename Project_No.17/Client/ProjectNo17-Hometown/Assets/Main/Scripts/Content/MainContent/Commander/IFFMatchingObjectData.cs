using System;
using System.Collections.Generic;
using System.Linq;

using BC.AIProvider;
using BC.OdccBase;

using Sirenix.OdinInspector;

using Unity.Mathematics;

using UnityEngine;

namespace TFContent
{

	[CreateAssetMenu(fileName = "IFFMatchingObjectData", menuName = "BC/IFFMatchingObjectData")]
	public class IFFMatchingObjectData : ScenarioObjectData, IGetIFFMatchingType
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
		[ValueDropdown("IFFTeamList"), SerializeField]
		private int userControlTeamID;

		public TeamFriendshipType noMatchIFFType = TeamFriendshipType.Neutral;

		[ListDrawerSettings(ShowFoldout = false, HideAddButton = true, HideRemoveButton = true, ShowPaging = false)]
		public IFFMatch[] IFFMatchingInfo;

		public int UserControlTeamID => userControlTeamID;

		public override void OnValidate()
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
			if(infoList.Length == 0) return list;
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
			return noMatchIFFType;
		}

		public UnitRelationType GetUnitRelation(int teamA, int teamB, int unitA, int unitB)
		{
			TeamFriendshipType friendship = GetTeamFriendship(teamA, teamB);
			return friendship switch {
				TeamFriendshipType.Enemy => UnitRelationType.Enemy,
				TeamFriendshipType.Neutral => UnitRelationType.Neutral,
				TeamFriendshipType.Ally => UnitRelationType.Ally,
				TeamFriendshipType.Team => unitA == unitB ? UnitRelationType.Self : UnitRelationType.Team,
				_ => UnitRelationType.Neutral,
			};
		}

		internal void SaveAllObject()
		{
		}
	}
}
