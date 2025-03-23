//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//using BC.AIProvider;
//using BC.ODCC;
//using BC.OdccBase;

//using static BC.AIProvider.SensorProvider;

//namespace TFContent
//{
//	public partial class CombatComputeComponent //IFFCombatSystem
//	{
//		public class IFFCombatSystem : CombatSystem
//		{
//			public override void SystemEnable()
//			{
//			}
//			public override void SystemDisable()
//			{
//			}

//			public override void SystemUpdate()
//			{
//				var dataMapping = CombatCompute.CombatDataMapping;
//				var iffMapping = CombatCompute.IFFMapping;
//				var iffMatchingInfo = CombatCompute.iffMatchingInfoData;

//				Dictionary<int, ConcurrentBag<(ObjectBehaviour, SensoringData)>> tempIFFSensorResult = new Dictionary<int, ConcurrentBag<(ObjectBehaviour, SensoringData)>>();
//				int iffLength = CombatCompute.iffMatchingInfoData.IFFMatchingInfo.Length;
//				for(int i = 0 ; i < iffLength ; i++)
//				{
//					tempIFFSensorResult.Add(i, new ConcurrentBag<(ObjectBehaviour, SensoringData)>());
//				}

//				Parallel.ForEach(dataMapping, data => {
//					if(!iffMapping.TryGetValue(data.Key, out var iff)) return;
//					if(!data.Value.TryGetData<CombatComputeData.SensorData>(out var sensorData)) return;
//					if(!data.Value.TryGetData<CombatComputeData.IFFData>(out var iffData)) return;

//					ConcurrentBag<(ObjectBehaviour, SensoringData)> sensoringDatas = tempIFFSensorResult[iffData.teamID];
//					int length = sensorData.sensorResult.Count;
//					for(int i = 0 ; i < length ; i++)
//					{
//						var dataValue = sensorData.sensorResult[i];
//						sensoringDatas.Add(dataValue);
//					}
//				});
//				Dictionary<int, (ObjectBehaviour, SensoringData)[]> IFFSensorResult = new Dictionary<int, (ObjectBehaviour, SensoringData)[]>();
//				for(int i = 0 ; i < iffLength ; i++)
//				{
//					IFFSensorResult.Add(i, tempIFFSensorResult[i].ToArray());
//				}
//				tempIFFSensorResult.Clear();
//				tempIFFSensorResult = null;


//				Parallel.ForEach(dataMapping, data => {
//					if(!iffMapping.TryGetValue(data.Key, out var iff)) return;
//					if(!data.Value.TryGetData<CombatComputeData.SensorData>(out var sensorData)) return;
//					if(!data.Value.TryGetData<CombatComputeData.AbilityData>(out var abilityData)) return;
//					if(!data.Value.TryGetData<CombatComputeData.IFFData>(out var iffData)) return;
//					if(!data.Value.TryGetData<CombatComputeData.ROCData>(out var rocData)) return;

//					var list = IFFSensorResult[iff.TeamID];
//					int length = list.Length;

//					var orderIFF = iffData.IFFID;
//					Dictionary<IFFProvider.IFFData, SensoringData> sensorResult = new Dictionary<IFFProvider.IFFData, SensoringData>(length);
//					int teamCapacity = 0;
//					int allyCapacity = 0;
//					int neutralCapacity = 0;
//					int enemyCapacity = 0;
//					TargetType rocCollectType = rocData.rocCollectTarget;
//					bool rocHasSelf = HasFlag(rocCollectType, TargetType.Self);
//					bool rocHasTeam = HasFlag(rocCollectType, TargetType.Team);
//					bool rocHasAlly = HasFlag(rocCollectType, TargetType.All);
//					bool rocHasNeutral = HasFlag(rocCollectType, TargetType.Neutral);
//					bool rocHasEnemy = HasFlag(rocCollectType, TargetType.Enemy);
//					for(int i = 0 ; i < length ; i++)
//					{
//						var helper = list[i].Item1;
//						if(helper == null) continue;
//						var colliderData = list[i].Item2;

//						IFFProvider.IFFData _iff = new IFFProvider.IFFData(colliderData.detectTeamID, colliderData.detectUnitID);
//						if(orderIFF.UnitID != _iff.UnitID && sensorResult.TryAdd(_iff, colliderData))
//						{
//							int friendship = iffMatchingInfo.Friendship(orderIFF, _iff);
//							if(rocHasEnemy   && friendship == -1) enemyCapacity++;
//							else if(rocHasNeutral && friendship == 0) neutralCapacity++;
//							else if(rocHasAlly    && friendship == 1) allyCapacity++;
//							else if(rocHasTeam    && friendship == 2) teamCapacity++;
//						}
//					}

//					iff.IFFParallelUpdate(sensorResult, orderIFF,
//						rocHasSelf, teamCapacity, allyCapacity, neutralCapacity, enemyCapacity,
//						FriendshipFinder, ObjectFinder, AbilityFinder,
//						out var selfResult,
//						out var teamList,
//						out var allyList,
//						out var neutralList,
//						out var enemyList,
//						out var allResultList
//						);

//					iffData.selfResult = selfResult;
//					iffData.allyList = allyList;
//					iffData.neutralList = neutralList;
//					iffData.enemyList = enemyList;
//					iffData.allResultList = allResultList;
//					int FriendshipFinder(int teamA, int teamB)
//					{
//						return iffMatchingInfo?.Friendship(teamA, teamB) ?? 0;
//					}
//					ObjectBehaviour ObjectFinder(IFFProvider.IFFData findIFF)
//					{
//						foreach(var item in iffMapping)
//						{
//							var iff = item.Value;
//							if(iff.IFFID.Equals(findIFF))
//							{
//								return item.Key;
//							}
//						}
//						return null;
//					}
//					AbilityStruct AbilityFinder(ObjectBehaviour objectKey)
//					{
//						if(objectKey==null) return null;
//						if(dataMapping.TryGetValue(objectKey, out var data))
//						{
//							if(data.TryGetData<CombatComputeData.AbilityData>(out var abilityData))
//							{
//								return abilityData.AbilityStruct;
//							}
//						}
//						if(objectKey.ThisContainer.TryGetComponent<AbilityProvider>(out var abilityProvider))
//						{
//							return abilityProvider.AbilityStruct;
//						}
//						return null;
//					}
//				});
//				IFFSensorResult.Clear();
//				IFFSensorResult = null;
//			}
//			private bool HasFlag(TargetType value, TargetType flagCheck)
//			{
//				return (value & flagCheck) == flagCheck;
//			}
//		}
//	}
//}