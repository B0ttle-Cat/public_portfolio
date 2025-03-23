using System;

using BC.ODCC;

using Sirenix.OdinInspector;

using Unity.Mathematics;

using UnityEngine;
namespace BC.AIProvider
{
	public class IFFProvider : ComponentBehaviour//, IOdccUpdate
	{
		public static IGetIFFMatchingType IFFMatching;
		public static int UserControlTeamID;

		[Serializable]
		public struct IFFData
		{
			[SerializeField, HorizontalGroup]
			private int teamID;
			[SerializeField, HorizontalGroup]
			private int unitID;
			public int TeamID { get => teamID; set => teamID = value; }
			public int UnitID { get => unitID; set => unitID = value; }
			public IFFData(int TeamID, int UnitID)
			{
				teamID = TeamID;
				unitID = UnitID;
			}
			public IFFData(int2 int2)
			{
				teamID = int2.x;
				unitID = int2.y;
			}
			public IFFData(Vector2Int vector2Int)
			{
				teamID = vector2Int.x;
				unitID = vector2Int.y;
			}
		}
		[SerializeField, InlineProperty]
		private IFFData iffID;
		public IFFData IFFID => iffID;
		public int TeamID { get => iffID.TeamID; set => iffID.TeamID = value; }
		public int UnitID { get => iffID.UnitID; set => iffID.UnitID = value; }

		public bool IsUserControl => TeamID == IFFProvider.UserControlTeamID;
	}
}