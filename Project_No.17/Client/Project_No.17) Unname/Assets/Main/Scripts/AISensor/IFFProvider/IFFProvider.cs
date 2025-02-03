using System;
using System.Collections.Generic;

using BC.ODCC;

using Sirenix.OdinInspector;

using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
namespace BC.AISensor
{
	public class IFFProvider : ComponentBehaviour//, IOdccUpdate
	{
		[Serializable]
		public struct IFFData
		{
			[SerializeField, HorizontalGroup]
			private int teamID;
			[SerializeField, HorizontalGroup]
			private uint unitID;
			public int TeamID { get => teamID; set => teamID = value; }
			public uint UnitID { get => unitID; set => unitID = value; }
			public IFFData(int TeamID, uint UnitID)
			{
				teamID = TeamID;
				unitID = UnitID;
			}
			public IFFData(int2 int2)
			{
				teamID = int2.x;
				unitID = (uint)int2.y;
			}
			public IFFData(Vector2Int vector2Int)
			{
				teamID = vector2Int.x;
				unitID = (uint)vector2Int.y;
			}
		}
		[SerializeField]
		private IFFData iffData;
		public int TeamID { get => iffData.TeamID; set => iffData.TeamID = value; }
		public uint UnitID { get => iffData.UnitID; set => iffData.UnitID = value; }

		public NativeArray<IFFData> SensorResultConvertToIFFList(List<Collider> sensorResult)
		{
			int length = sensorResult.Count;
			var inputTargetList = new NativeArray<IFFData>(length, Allocator.Temp);
			for(int i = 0 ; i < length ; i++)
			{
				if(sensorResult[i].TryGetComponent<ComponentBehaviour>(out var componet))
				{
					if(componet.ThisContainer.TryGetComponent<IFFProvider>(out var targetIFF))
					{
						inputTargetList[i] = targetIFF.iffData;
					}
				}
			}
			return inputTargetList;
		}

		public void IFFUpdate(IFFProvider[] teamList, IFFProvider[] allyList, IFFProvider[] neutralList, IFFProvider[] enemyList)
		{

		}
	}
}