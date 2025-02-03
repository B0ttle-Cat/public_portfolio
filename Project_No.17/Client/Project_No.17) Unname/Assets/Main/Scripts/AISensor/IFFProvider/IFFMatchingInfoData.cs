using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using Unity.Mathematics;

using UnityEngine;

namespace BC.AISensor
{
	[CreateAssetMenu(fileName = "IFFMatchingInfoData", menuName = "BC/IFFMatchingInfoData")]
	public class IFFMatchingInfoData : ScriptableObject
	{
		[Serializable]
		public struct IFFMatch
		{
			[HorizontalGroup, HideLabel, InlineProperty]
			public Vector2Int MatchNumber;
			[HorizontalGroup, HideLabel, EnumToggleButtons]
			public FriendshipType Friendship;
			public enum FriendshipType : int
			{
				Enemy = -1,
				Neutral = 0,
				Ally  = 1,
			}
			public int2 MatchInt2 => new int2(MatchNumber.x, MatchNumber.y);
		}
		[ValueDropdown("IFFTeamList")]
		public int userControlTeamID;
		[ListDrawerSettings(ShowFoldout = false, HideAddButton = true, HideRemoveButton = true, ShowPaging = false)]
		public List<IFFMatch> IFFMatchingInfo;

#if UNITY_EDITOR
		[ShowInInspector, InlineButton("CreateList"), PropertyOrder(-1)]
		[Range(1,10)]
		private int teamCount;
		private void CreateList()
		{
			List<IFFMatch> oldList = IFFMatchingInfo;

			IFFMatchingInfo = new List<IFFMatch>();
			for(int x = 0 ; x < teamCount ; x++)
			{
				for(int y = 0 ; y < teamCount ; y++)
				{
					if(x >= y) continue;
					Vector2Int MatchNumber = new Vector2Int(x,y);
					IFFMatch.FriendshipType friendshipType = IFFMatch.FriendshipType.Neutral;
					int index = oldList.FindIndex(i => i.MatchNumber == MatchNumber);
					if(index >= 0)
					{
						friendshipType = oldList[index].Friendship;
					}

					IFFMatchingInfo.Add(new IFFMatch {
						MatchNumber = MatchNumber,
						Friendship = friendshipType,
					});
				}
			}
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
		public int Friendship(Vector2Int MatchNumber)
		{
			if(MatchNumber.x == MatchNumber.y) return (int)2;

			if(MatchNumber.x > MatchNumber.y)
			{
				int y = MatchNumber.y;
				MatchNumber.y = MatchNumber.x;
				MatchNumber.x = y;
			}

			int index = IFFMatchingInfo.FindIndex(i => i.MatchNumber == MatchNumber);
			if(index < 0) return 0;
			return (int)IFFMatchingInfo[index].Friendship;
		}
	}
}
