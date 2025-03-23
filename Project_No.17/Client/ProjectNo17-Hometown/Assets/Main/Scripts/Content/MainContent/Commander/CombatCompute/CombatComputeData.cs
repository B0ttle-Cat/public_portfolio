//using System;
//using System.Collections.Generic;

//using BC.AIProvider;
//using BC.ODCC;
//using BC.OdccBase;

//using UnityEngine;

//using static BC.AIProvider.IFFProvider;
//using static BC.AIProvider.SensorProvider;

//namespace TFContent
//{
//	public class CombatComputeData : DataObject, IOdccData
//	{
//		[SerializeReference]
//		public List<Data> data = new List<Data>();
//		public T GetData<T>() where T : Data
//		{
//			data ??= new List<Data>();
//			int length = data.Count;
//			for(int i = 0 ; i < length ; i++)
//			{
//				if(data[i] is T t) return t;
//			}
//			return null;
//		}
//		public bool HasData<T>() where T : Data
//		{
//			data ??= new List<Data>();
//			int length = data.Count;
//			for(int i = 0 ; i < length ; i++)
//			{
//				if(data[i] is T t) return true;
//			}
//			return false;
//		}
//		public bool TryGetData<T>(out T t) where T : Data
//		{
//			t = null;
//			data ??= new List<Data>();
//			int length = data.Count;
//			for(int i = 0 ; i < length ; i++)
//			{
//				if(data[i] is T tt)
//				{
//					t = tt;
//					break;
//				}
//			}
//			return t is not null;
//		}
//		public void AddData<T>(T newT) where T : Data
//		{
//			if(newT == null) return;

//			data ??= new List<Data>();
//			int length = data.Count;
//			for(int i = 0 ; i < length ; i++)
//			{
//				if(data[i] is T)
//				{
//					data[i] = newT;
//					return;
//				}
//			}
//			data.Add(newT);
//		}
//		[Serializable]
//		public abstract class Data { }
//		public class AbilityData : Data
//		{
//			private AbilityProvider provider;
//			public AbilityStruct AbilityStruct => provider?.AbilityStruct;
//			public AbilityData(AbilityProvider to)
//			{
//				provider = to;
//			}
//		}
//		public class SensorData : Data
//		{
//			public List<(ObjectBehaviour, SensoringData)> sensorResult;
//			public SensorData(SensorProvider to)
//			{
//			}
//		}
//		public class IFFData : Data
//		{
//			public IFFProvider.IFFData IFFID;
//			public int teamID => IFFID.TeamID;
//			public int unitID => IFFID.UnitID;

//			public IdentifiedData selfResult;
//			public IdentifiedData[] teamList;
//			public IdentifiedData[] allyList;
//			public IdentifiedData[] neutralList;
//			public IdentifiedData[] enemyList;
//			public IdentifiedData[] allResultList;

//			public IFFData(IFFProvider to)
//			{
//				IFFID = to.IFFID;
//			}
//		}
//		public class ROCData : Data
//		{
//			public RelationType rocCollectTarget;
//			public ROCData(ROCProvider to)
//			{
//				rocCollectTarget = to.RulesCollectTarget();
//			}
//		}
//	}
//}