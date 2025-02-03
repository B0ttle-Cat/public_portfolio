using System.Linq;

using BC.AISensor;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace TFContent
{
	public partial class CombatManagement//.IFFSystem
	{
		public struct IFFTag : IComponentData
		{
			public int teamID;
			public uint unitID;

			public NativeArray<IFFProvider.IFFData> sensorResult;

			public NativeList<Entity> teamList;
			public NativeList<Entity> allyList;
			public NativeList<Entity> neutralList;
			public NativeList<Entity> enemyList;

			internal void Dispose()
			{
				if(sensorResult != null)
				{
					sensorResult.Dispose();
					teamList.Dispose();
					allyList.Dispose();
					neutralList.Dispose();
					enemyList.Dispose();
				}
			}
		}
		[UpdateInGroup(typeof(SimulationSystemGroup))]
		[UpdateAfter(typeof(CombatSensorUpdate))]
		public partial struct CombatIFFUpdate : ISystem
		{

			public void OnCreate(ref SystemState state)
			{
			}
			public void OnUpdate(ref SystemState state)
			{
				CombatManagement manager = CombatManagement.Manager;
				IFFMatchingInfoData iffMatchingInfo =  manager.iffMatchingInfoData;
				if(iffMatchingInfo == null) return;
				int matchCount = iffMatchingInfo.IFFMatchingInfo.Count;
				NativeHashMap<int2, int> iffFriendshipData = new NativeHashMap<int2, int>(matchCount, Allocator.TempJob);
				NativeHashMap<int2, Entity> entityIffKeyValue = new NativeHashMap<int2, Entity>(matchCount, Allocator.TempJob);
				for(int i = 0 ; i < matchCount ; i++)
				{
					var matchInfo = iffMatchingInfo.IFFMatchingInfo[i];
					iffFriendshipData.Add(matchInfo.MatchInt2, (int)matchInfo.Friendship);
				}

				var sensorMapping = manager.SensorMapping;
				var iffMapping = manager.IFFMapping;
				foreach(var (sensorTag, iffTag, entity) in SystemAPI.Query<RefRO<SensorTag>, RefRW<IFFTag>>().WithEntityAccess())
				{
					if(sensorMapping.TryGetValue(entity, out var sensor))
					{
						if(iffMapping.TryGetValue(entity, out var iff))
						{
							var sensorResult = iff.SensorResultConvertToIFFList(sensor.sensorResult);
							var iffTagRW = iffTag.ValueRW;

							iffTagRW.sensorResult = sensorResult;
							iffTagRW.teamList = new NativeList<Entity>(Allocator.TempJob);
							iffTagRW.allyList = new NativeList<Entity>(Allocator.TempJob);
							iffTagRW.neutralList = new NativeList<Entity>(Allocator.TempJob);
							iffTagRW.enemyList = new NativeList<Entity>(Allocator.TempJob);
							entityIffKeyValue[new int2(iffTagRW.teamID, (int)iffTagRW.unitID)] = entity;

							iffTag.ValueRW = iffTagRW;
						}
					}

				}

				var identificationJob = new IdentificationJob(){
					iffFriendshipData = iffFriendshipData,
					entityIffKeyValue = entityIffKeyValue,
				};
				var identificationJobHandle = identificationJob.ScheduleParallel(state.Dependency);
				identificationJobHandle.Complete();



				foreach(var (iffTag, entity) in SystemAPI.Query<RefRO<IFFTag>>().WithEntityAccess())
				{
					if(iffMapping.TryGetValue(entity, out var iff))
					{
						IFFTag tag = iffTag.ValueRO;
						iff.IFFUpdate(
							tag.teamList.Select(i => iffMapping[i]).ToArray(),
							tag.allyList.Select(i => iffMapping[i]).ToArray(),
							tag.neutralList.Select(i => iffMapping[i]).ToArray(),
							tag.enemyList.Select(i => iffMapping[i]).ToArray()
							);

						tag.Dispose();
					}
				}

				entityIffKeyValue.Dispose();
				iffFriendshipData.Dispose();
			}
		}
		[BurstCompile]
		public partial struct TeamListCreate : IJobEntity
		{
			public NativeHashMap<int, NativeList<int>> teamSensorResult;
			// Translation 컴포넌트를 가진 엔티티의 위치를 업데이트합니다.
			void Execute(ref IFFTag iff)
			{
				teamSensorResult.Add(iff.teamID, new NativeList<int>(Allocator.Temp));
			}
		}

		[BurstCompile]
		public partial struct IdentificationJob : IJobEntity
		{
			public NativeHashMap<int2, int> iffFriendshipData;
			public NativeHashMap<int2, Entity> entityIffKeyValue;
			void Execute(ref IFFTag iff)
			{
				int count = iff.sensorResult.Length;
				for(int i = 0 ; i < count ; i++)
				{
					var sensor = iff.sensorResult[i];

					var entity = entityIffKeyValue[new int2(sensor.TeamID, (int)sensor.UnitID) ];

					if(iff.teamID == sensor.TeamID)
					{
						iff.teamList.Add(entity);
					}
					else
					{
						int2 matchNumber = iff.teamID > sensor.TeamID
							? new int2(iff.teamID, sensor.TeamID)
							: new int2(sensor.TeamID, iff.teamID);

						int friendship = iffFriendshipData[matchNumber];

						if(friendship > 0) iff.allyList.Add(entity);
						else if(friendship < 0) iff.enemyList.Add(entity);
						else iff.neutralList.Add(entity);
					}
				}
			}
		}
	}
}
