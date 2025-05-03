using System;

namespace BC.OdccBase
{
	public enum TeamFriendshipType : int
	{
		Enemy = -1,
		Neutral = 0,
		Ally  = 1,
		Team  = 2,
	}


	[Flags]
	public enum UnitRelationType : int
	{
		None = 0,

		Enemy = 1<<0,
		Neutral = 1<<1,
		Ally  = 1<<2,
		Team  = 1<<3,
		Self = 1<<4,

		All = -1,
	}
}
