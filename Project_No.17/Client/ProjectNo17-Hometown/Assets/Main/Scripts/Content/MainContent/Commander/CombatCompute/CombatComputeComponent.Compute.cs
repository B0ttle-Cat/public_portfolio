using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BC.Actuator;
using BC.AIProvider;
using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;
using Sirenix.Utilities;

using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

namespace TFContent
{
	public interface ICombatVisibleListener : IOdccComponent
	{
		public void OnShowVisible();
		public void OnHideVisible();
	}
	public interface ICombatReactionListener : IOdccComponent
	{
		public void OnInReactionRange(ObjectBehaviour targetUnit);
		public void OnOutReactionRange(ObjectBehaviour targetUnit);
	}

	public partial class CombatComputeComponent // Compute 
	{
		[ShowInInspector]
		private bool usingParallelForEach;
		private bool UsingParallelForEach {
			get {
#if UNITY_EDITOR
				if(System.Diagnostics.Debugger.IsAttached)
				{
					return false;
				}
#endif
				return usingParallelForEach;
			}
		}


		private int AllTeamLength;
		private int AllUnitLength;

		// TeamID : TargetIndex[]
		[ShowInInspector, Sirenix.OdinInspector.ReadOnly]
		private Dictionary<(int,int), UnitPairs> unitPairsList;
		[ShowInInspector, Sirenix.OdinInspector.ReadOnly]
		private Dictionary<int, int[]> computeTeamVisibleIndexList;
		[ShowInInspector, Sirenix.OdinInspector.ReadOnly]
		private Dictionary<int, int[]> computeUnitReactionIndexList;
		private Dictionary<int, ObjectBehaviour[]> computeTeamVisibleList;
		private Dictionary<int, ObjectBehaviour[]> computeUnitReactionList;
		private struct UnitPairs
		{
			public int indexA, indexB;
			public int unitA, unitB;
			public int teamA, teamB;

			public UnitRelationType unitRelationType;
			public float spaceDistance;
			public Vector3 directionA2B;
			public Vector3 directionB2B => -directionA2B;

			public float radiusA;
			public float radiusB;

			public float distanceAndRadius => spaceDistance + radiusA + radiusB;

			public List<ActuatorModule> canUseActuatorListA2B;
			public List<ActuatorModule> canUseActuatorListB2A;

			public UnitPairsTarget ConvertUnitPairsTarget(bool AisThis, ICombatCompute combatCompute)
			{
				UnitPairsTarget unitPairsTarget = new UnitPairsTarget();
				if(AisThis)
				{
					unitPairsTarget.thisTeamID = teamA;
					unitPairsTarget.thisUnitID = unitA;
					unitPairsTarget.thisIndex = indexA;

					unitPairsTarget.targetTeamID = teamB;
					unitPairsTarget.targetUnitID = unitB;
					unitPairsTarget.targetIndex= indexB;

					unitPairsTarget.thisRadius = radiusA;
					unitPairsTarget.targetRadius = radiusB;

					unitPairsTarget.directionToTarget = directionA2B;
					unitPairsTarget.canUseActuatorList = canUseActuatorListA2B;
				}
				else
				{
					unitPairsTarget.thisTeamID = teamB;
					unitPairsTarget.thisUnitID = unitB;
					unitPairsTarget.thisIndex = indexB;

					unitPairsTarget.targetTeamID = teamA;
					unitPairsTarget.targetUnitID = unitA;
					unitPairsTarget.targetIndex= indexA;

					unitPairsTarget.thisRadius = radiusB;
					unitPairsTarget.targetRadius = radiusA;

					unitPairsTarget.directionToTarget = directionB2B;
					unitPairsTarget.canUseActuatorList = canUseActuatorListB2A;
				}
				unitPairsTarget.relationType = unitRelationType;
				unitPairsTarget.spaceDistance = spaceDistance;

				unitPairsTarget.inRangeVisible = combatCompute.IsVisibleTarget(unitPairsTarget.thisIndex, unitPairsTarget.targetIndex);
				unitPairsTarget.inRangeReaction = combatCompute.IsReactionTarget(unitPairsTarget.thisIndex, unitPairsTarget.targetIndex);

				unitPairsTarget.thisObject = combatCompute.FindObjectBehaviour(unitPairsTarget.thisIndex);
				unitPairsTarget.targetObject = combatCompute.FindObjectBehaviour(unitPairsTarget.targetIndex);
				unitPairsTarget.thisPosition = combatCompute.FindReactionTarget(unitPairsTarget.thisIndex).ReactionPosition();
				unitPairsTarget.targetPosition = combatCompute.FindReactionTarget(unitPairsTarget.targetIndex).ReactionPosition();
				unitPairsTarget.thisRotation = combatCompute.FindReactionTarget(unitPairsTarget.thisIndex).ReactionRotation();
				unitPairsTarget.targetRotation = combatCompute.FindReactionTarget(unitPairsTarget.targetIndex).ReactionRotation();

				return unitPairsTarget;
			}
		}
		private struct UnitValueInfo
		{
			public int index;
			public int teamID;
			public int unitID;

			public Vector3 position;
			public Quaternion rotation;
			public float radius;

			public float visibleRange;
			public float reactionRange;

			public int reactionLayer;
			//public UnitRelationType canReactionUnitType;
		}

		private void OnCombatCompute()
		{
			BufferListUpdate();

			UnitValueInfoUpdate(out UnitValueInfo[] unitValueInfos);
			UnitPairsListUpdate(unitValueInfos);

			VisibleListComputeRange(unitValueInfos, out var tempTeamVisibleList);
			VisibleListComputeRaycast(unitValueInfos, tempTeamVisibleList);

			ReactionListComputeRange(unitValueInfos, tempTeamVisibleList, out var tempUnitReactionList);
			ReactionListComputeRaycast(unitValueInfos, tempUnitReactionList);
			ReactionListComputeActuator(unitValueInfos, unitPairsList, tempUnitReactionList);

			NewVisibleUpdate(computeTeamVisibleList, tempTeamVisibleList, out var newTeamVisibleList);
			computeTeamVisibleList?.Clear();
			computeTeamVisibleList = newTeamVisibleList;
			newTeamVisibleList = null;
			computeTeamVisibleIndexList?.Clear();
			computeTeamVisibleIndexList = tempTeamVisibleList;
			tempTeamVisibleList = null;

			NewReactionUpdate(computeUnitReactionList, tempUnitReactionList, out var newUnitReactionList);
			computeUnitReactionList?.Clear();
			computeUnitReactionList = newUnitReactionList;
			newUnitReactionList = null;
			computeUnitReactionIndexList?.Clear();
			computeUnitReactionIndexList = tempUnitReactionList;
			tempUnitReactionList = null;
		}
		private (int indexA, int indexB) UnitIndexPairsKey((int index1, int index2) pairsKey)
		{
			return UnitIndexPairsKey(pairsKey.index1, pairsKey.index2);
		}
		private (int indexA, int indexB) UnitIndexPairsKey(int index1, int index2)
		{
			return (Mathf.Min(index1, index2), Mathf.Max(index1, index2));
		}
		private void BufferListUpdate()
		{
			bool updateList = false;
			int length = addBufferList.Count;
			if(length==0) return;

			List<ObjectBehaviour> _ObjectList = new List<ObjectBehaviour>(ObjectList ?? new ObjectBehaviour[0]);
			List<AbilityProvider> _AbilityList = new List<AbilityProvider>(AbilityList ?? new AbilityProvider[0]);
			List<ActuatorProvider> _ActuatorList = new List<ActuatorProvider>(ActuatorList ?? new ActuatorProvider[0]);
			List<IFFProvider> _IFFList = new List<IFFProvider>(IFFList?? new IFFProvider[0]);
			List<ReactionTarget> _AttackingList = new List<ReactionTarget>(ReactionList?? new ReactionTarget[0]);

			if(somethingRemove)
			{
				somethingRemove = false;
				Stack<int> removeIndexStack = new Stack<int>();
				length = _ObjectList.Count;
				for(int i = 0 ; i < length ; i++)
				{
					if(_ObjectList[i] == null) removeIndexStack.Push(i);
				}

				while(removeIndexStack.TryPop(out int removeIndex))
				{
					updateList = true;
					_ObjectList.RemoveAt(removeIndex);
					//_DataList.RemoveAt(removeIndex);
					_AbilityList.RemoveAt(removeIndex);
					_ActuatorList.RemoveAt(removeIndex);
					_IFFList.RemoveAt(removeIndex);
					_AttackingList.RemoveAt(removeIndex);
				}
			}

			while(addBufferList.TryDequeue(out var item))
			{
				var update = item.Item1;
				if(update == null) continue;
				var isAdd = item.Item2;
				if(isAdd)
				{
					int index = _ObjectList.FindIndex(item=>item==update);
					if(index>=0) continue;
					updateList = true;
					_ObjectList.Add(update);
					//_DataList.Add(update.ThisContainer.GetData<UnitCommandData>());
					_AbilityList.Add(update.ThisContainer.GetComponent<AbilityProvider>());
					_ActuatorList.Add(update.ThisContainer.GetComponent<ActuatorProvider>());
					_IFFList.Add(update.ThisContainer.GetComponent<IFFProvider>());
					_AttackingList.Add(update.ThisContainer.GetComponent<ReactionTarget>());
				}
				else
				{
					int index = _ObjectList.FindIndex(item=>item==update);
					if(index<0) continue;
					updateList = true;
					_ObjectList.RemoveAt(index);
					_AbilityList.RemoveAt(index);
					_ActuatorList.RemoveAt(index);
					_IFFList.RemoveAt(index);
					_AttackingList.RemoveAt(index);
				}
			}

			if(updateList)
			{
				ObjectList = _ObjectList.ToArray();
				AbilityList = _AbilityList.ToArray();
				ActuatorList =_ActuatorList.ToArray();
				IFFList = _IFFList.ToArray();
				ReactionList = _AttackingList.ToArray();

				AllUnitLength = ObjectList.Length;
				AllTeamLength = iffMatchingInfoData.IFFMatchingInfo.Length;
			}
		}
		private void UnitValueInfoUpdate(out UnitValueInfo[] unitValueInfos)
		{
			unitValueInfos = new UnitValueInfo[AllUnitLength];
			for(int i = 0 ; i < AllUnitLength ; i++)
			{
				var data = ObjectList[i].ThisContainer.GetData<UnitCommandData>();
				var iff = IFFList[i];
				var actuator = ActuatorList[i];
				var reaction = ReactionList[i];
				var ability = AbilityList[i];
				bool hasIFF = iff != null;
				bool hasReaction = reaction != null;
				bool hasAbility = ability != null;
				bool hasData = data != null;
				unitValueInfos[i] = new UnitValueInfo() {
					index = i,
					teamID = hasIFF ? iff.TeamID : -1,
					unitID = hasIFF ? iff.UnitID : -1,
					position = hasReaction ? reaction.ReactionPosition() : Vector3.zero,
					rotation = hasReaction ? reaction.ReactionRotation() : Quaternion.identity,
					radius = hasReaction ? reaction.Radius : 0,
					visibleRange = hasAbility ? ability.GetVisibleRange() : 0f,
					reactionRange = hasData ? ability.GetReactionRange(data.isEnterCombat) : 0f,
					reactionLayer = hasReaction ? reaction.Layer : 0,
				};
			}
		}
		private void UnitPairsListUpdate(UnitValueInfo[] unitValueInfos)
		{
			//iffMatchingInfoData
			if(unitPairsList == null) unitPairsList = new Dictionary<(int, int), UnitPairs>();
			else unitPairsList.Clear();

			if(UsingParallelForEach) Parallel.ForEach(unitValueInfos, ForEach);
			else unitValueInfos.ForEach(ForEach);

			void ForEach(UnitValueInfo unitValue)
			{
				int length = unitValueInfos.Length;
				for(int i = 0 ; i < length ; i++)
				{
					var relationTarget = unitValueInfos[i];
					if(unitValue.index > relationTarget.index) continue;

					var key = UnitIndexPairsKey(unitValue.index, relationTarget.index);
					int indexA = key.indexA;
					int indexB = key.indexB;
					int teamA = unitValueInfos[indexA].teamID;
					int teamB = unitValueInfos[indexB].teamID;
					int unitA = unitValueInfos[indexA].unitID;
					int unitB = unitValueInfos[indexB].unitID;
					float radiusA = unitValueInfos[indexA].radius;
					float radiusB = unitValueInfos[indexB].radius;
					if(indexA == indexB)
					{
						unitPairsList[key] = new UnitPairs() {
							indexA = key.indexA,
							indexB = key.indexB,
							teamA = teamA,
							teamB = teamB,
							unitA = unitA,
							unitB = unitB,
							radiusA = radiusA,
							radiusB = radiusB,
							unitRelationType = UnitRelationType.Self,
							spaceDistance = 0,
							directionA2B = Vector3.zero,
							canUseActuatorListA2B = new List<ActuatorModule>(),
							canUseActuatorListB2A = new List<ActuatorModule>(),
						};
					}
					else
					{
						Vector3 positionA = unitValueInfos[indexA].position;
						Vector3 positionB = unitValueInfos[indexB].position;

						Vector3 distance = positionB - positionA;
						unitPairsList.Add(key, new UnitPairs() {
							indexA = key.indexA,
							indexB = key.indexB,
							teamA = teamA,
							teamB = teamB,
							unitA = unitA,
							unitB = unitB,
							radiusA = radiusA,
							radiusB = radiusB,
							unitRelationType = iffMatchingInfoData.GetUnitRelation(teamA, teamB, unitA, unitB),
							spaceDistance = distance.magnitude - (radiusA + radiusB),
							directionA2B = distance.normalized,
							canUseActuatorListA2B = new List<ActuatorModule>(),
							canUseActuatorListB2A = new List<ActuatorModule>(),
						});
					}
				}
			}
		}

		private void VisibleListComputeRange(UnitValueInfo[] unitValueInfos, out Dictionary<int, int[]> newTeamVisibleList)
		{
			newTeamVisibleList = new Dictionary<int, int[]>();

			Dictionary<int, HashSet<int>> teamVisibleParallelList = new Dictionary<int, HashSet<int>>();
			for(int i = -1 ; i < AllTeamLength ; i++)
			{
				teamVisibleParallelList.Add(i, new HashSet<int>());
			}
			if(UsingParallelForEach) Parallel.ForEach(unitPairsList, ForEach);
			else unitPairsList.ForEach(ForEach);

			for(int i = -1 ; i < AllTeamLength ; i++)
			{
				HashSet<int> bag = teamVisibleParallelList[i];
				newTeamVisibleList.Add(i, bag.Count > 0 ? bag.ToArray() : new int[0]);
			}
			void ForEach(KeyValuePair<(int, int), UnitPairs> relationItem)
			{
				UnitPairs relation = relationItem.Value;
				float distance = relation.spaceDistance;

				int indexA = relation.indexA;
				int indexB = relation.indexB;
				int teamA = relation.teamA;
				int teamB = relation.teamB;
				if(teamA < -1) teamA = -1;
				else if(teamA >= AllTeamLength) teamA = AllTeamLength-1;
				if(teamB < -1) teamB = -1;
				else if(teamB >= AllTeamLength) teamB = AllTeamLength-1;

				UnitValueInfo unitValueA = unitValueInfos[indexA];
				UnitValueInfo unitValueB = unitValueInfos[indexB];

				if(unitValueA.visibleRange >= distance)
				{
					if(teamVisibleParallelList.ContainsKey(teamA))
					{
						teamVisibleParallelList[teamA].Add(indexB);
					}
					else
					{
						teamVisibleParallelList[-1].Add(indexB);
					}
				}
				if(unitValueB.visibleRange >= distance)
				{
					if(teamVisibleParallelList.ContainsKey(teamB))
					{
						teamVisibleParallelList[teamB].Add(indexA);
					}
					else
					{
						teamVisibleParallelList[-1].Add(indexA);
					}
				}
			}
		}
		/// TODO:: 위에서는 중심 <-> 중심 간의 Ray만 검사하기때문에
		/// 중심 <-> 외각에 걸치는 경우에 대한 검사는 추가로 진행해야 한다.
		/// teamToUnitRangeVisibleList 에 남앙 있는 항목을 사용해서 추가적인 감사를 진행 해야 한다.
		private void VisibleListComputeRaycast(UnitValueInfo[] unitValueInfos, Dictionary<int, int[]> newTeamVisibleList)
		{
			Dictionary<int, ConcurrentBag<(int,RaycastCommand)>> newTeamRayList = new Dictionary<int, ConcurrentBag<(int,RaycastCommand)>>();
			for(int i = -1 ; i < AllTeamLength ; i++)
			{
				newTeamRayList.Add(i, new ConcurrentBag<(int, RaycastCommand)>());
			}

			var rayQuery = new QueryParameters(blockingVisibleLayerMask.value, false, QueryTriggerInteraction.UseGlobal, false);
			if(UsingParallelForEach) Parallel.ForEach(unitValueInfos, ForEach);
			else unitValueInfos.ForEach(ForEach);

			for(int teamID = -1 ; teamID < AllTeamLength ; teamID++)
			{
				var rayBag = newTeamRayList[teamID];
				var rayBagList = rayBag.ToArray();
				int rayBagCount = rayBagList.Length;
				NativeArray<RaycastCommand> raycastCommand = new NativeArray<RaycastCommand>(rayBagCount, Allocator.TempJob);
				NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(rayBagCount, Allocator.TempJob);
				for(int ii = 0 ; ii < rayBagCount ; ii++)
				{
					raycastCommand[ii] = rayBagList[ii].Item2;
				}
				RaycastCommand.ScheduleBatch(raycastCommand, results, 32, default(JobHandle)).Complete();

				HashSet<int> teamToUnitRangeVisibleList = newTeamVisibleList[teamID].ToHashSet();
				HashSet<int> teamToUnitRayVisibleList = new HashSet<int>(rayBagCount);
				for(int ii = 0 ; ii < rayBagCount ; ii++)
				{
					RaycastHit rayResult = results[ii];
					var hitCollider = rayResult.collider;
					if(hitCollider != null)
					{
						// 충돌된 무언가에 시야가 막힘
						continue;
					}
					var rayBagItem = rayBagList[ii];
					teamToUnitRayVisibleList.Add(rayBagItem.Item1);
					teamToUnitRangeVisibleList.Remove(rayBagItem.Item1);
				}
				raycastCommand.Dispose();
				results.Dispose();

				/// TODO:: 위에서는 중심 <-> 중심 간의 Ray만 검사하기때문에
				/// 중심 <-> 외각에 걸치는 경우에 대한 검사는 추가로 진행해야 한다.
				/// teamToUnitRangeVisibleList 에 남앙 있는 항목을 사용해서 추가적인 감사를 진행 해야 한다.
				///
				///

				newTeamVisibleList[teamID] = teamToUnitRayVisibleList.ToArray();
			}
			void ForEach(UnitValueInfo relationItem)
			{
				int thisTeam = relationItem.teamID;
				int thisIndex = relationItem.index;

				var targetList = newTeamVisibleList[thisTeam];
				int targetCount = targetList.Length;
				for(int i = 0 ; i < targetCount ; i++)
				{
					var target = unitValueInfos[targetList[i]];
					int targetTeam = target.teamID;
					int targetIndex = target.index;
					(int unitA, int unitB) unitPairsKey = UnitIndexPairsKey(thisIndex, targetIndex);
					var relation  = unitPairsList[unitPairsKey];
					Vector3 directionA2B = unitPairsKey.unitA == thisIndex ? relation.directionA2B : -relation.directionA2B;
					float distanceA2B = relation.spaceDistance;
					float distanceAndRadius = relation.distanceAndRadius;

					RaycastCommand raycastCommand = new RaycastCommand(relationItem.position, directionA2B, rayQuery, distanceAndRadius);
					newTeamRayList[thisTeam].Add((targetIndex, raycastCommand));
				}
			}
		}

		private void ReactionListComputeRange(UnitValueInfo[] unitValueInfos, in Dictionary<int, int[]> teamVisibleIndexList, out Dictionary<int, int[]> newUnitReactionList)
		{
			newUnitReactionList = new Dictionary<int, int[]>();
			HashSet<(int order, int target)> computeList = new HashSet<(int order, int target)>();
			Dictionary<int, ConcurrentBag<int>> teamReactionParallelList = new Dictionary<int, ConcurrentBag<int>>();

			for(int i = 0 ; i < AllUnitLength ; i++)
			{
				newUnitReactionList.Add(i, null);
				teamReactionParallelList.Add(i, new ConcurrentBag<int>());

				var thisValue = unitValueInfos[i];
				int[] teamVisible = teamVisibleIndexList[thisValue.teamID];
				int length = teamVisible.Length;
				for(int ii = 0 ; ii < length ; ii++)
				{
					computeList.Add((i, teamVisible[ii]));
				}
			}

			if(UsingParallelForEach) Parallel.ForEach(computeList, ForEach);
			else computeList.ForEach(ForEach);

			for(int i = 0 ; i < AllUnitLength ; i++)
			{
				newUnitReactionList[i] = teamReactionParallelList[i].ToArray();
			}

			void ForEach((int order, int target) orderToTarget)
			{
				var thisValue = unitValueInfos[orderToTarget.order];
				float thisReactionRange = thisValue.reactionRange;

				var targetValue = unitValueInfos[orderToTarget.target];
				float targetReactionRange = targetValue.reactionRange;

				var relation = unitPairsList[UnitIndexPairsKey(orderToTarget)];

				float distanceA2B = relation.spaceDistance;

				if(thisReactionRange >= distanceA2B)
				{
					teamReactionParallelList[orderToTarget.order].Add(orderToTarget.target);
				}
			}
		}

		/// TODO:: 위에서는 중심 <-> 중심 간의 Ray만 검사하기때문에
		/// 중심 <-> 외각에 걸치는 경우에 대한 검사는 추가로 진행해야 한다.
		/// unitToUnitRangeReactionList 에 남앙 있는 항목을 사용해서 추가적인 감사를 진행 해야 한다.
		private void ReactionListComputeRaycast(UnitValueInfo[] unitValueInfos, Dictionary<int, int[]> newUnitReactionList)
		{
			Dictionary<int, ConcurrentBag<(int,RaycastCommand)>> newUnitRayList = new Dictionary<int, ConcurrentBag<(int,RaycastCommand)>>();
			for(int i = 0 ; i < AllUnitLength ; i++)
			{
				newUnitRayList.Add(i, new ConcurrentBag<(int, RaycastCommand)>());
			}

			var rayQuery = new QueryParameters(blockingReactionLayerMask.value, false, QueryTriggerInteraction.UseGlobal, false);
			if(UsingParallelForEach) Parallel.ForEach(unitValueInfos, ForEach);
			else unitValueInfos.ForEach(ForEach);

			for(int unitID = 0 ; unitID < AllUnitLength ; unitID++)
			{
				var rayBag = newUnitRayList[unitID];
				var rayBagList = rayBag.ToArray();
				int rayBagCount = rayBagList.Length;
				NativeArray<RaycastCommand> raycastCommand = new NativeArray<RaycastCommand>(rayBagCount, Allocator.TempJob);
				NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(rayBagCount, Allocator.TempJob);
				for(int ii = 0 ; ii < rayBagCount ; ii++)
				{
					raycastCommand[ii] = rayBagList[ii].Item2;
				}
				RaycastCommand.ScheduleBatch(raycastCommand, results, 32, default(JobHandle)).Complete();

				HashSet<int> unitToUnitRangeReactionList = newUnitReactionList[unitID].ToHashSet();
				HashSet<int> unitToUnitRayReactionList = new HashSet<int>(rayBagCount);
				for(int ii = 0 ; ii < rayBagCount ; ii++)
				{
					RaycastHit rayResult = results[ii];
					var hitCollider = rayResult.collider;
					if(hitCollider != null)
					{
						// 충돌된 무언가에 상호작용이 막힘
						continue;
					}
					var rayBagItem = rayBagList[ii];
					unitToUnitRayReactionList.Add(rayBagItem.Item1);
					unitToUnitRangeReactionList.Remove(rayBagItem.Item1);
				}
				raycastCommand.Dispose();
				results.Dispose();

				/// TODO:: 위에서는 중심 <-> 중심 간의 Ray만 검사하기때문에
				/// 중심 <-> 외각에 걸치는 경우에 대한 검사는 추가로 진행해야 한다.
				/// unitToUnitRangeReactionList 에 남앙 있는 항목을 사용해서 추가적인 감사를 진행 해야 한다.

				newUnitReactionList[unitID] = unitToUnitRayReactionList.ToArray();
			}

			void ForEach(UnitValueInfo relationItem)
			{
				int thisTeam = relationItem.teamID;
				int thisIndex = relationItem.index;

				var targetList = newUnitReactionList[thisIndex];
				int targetCount = targetList.Length;
				for(int i = 0 ; i < targetCount ; i++)
				{
					var target = unitValueInfos[targetList[i]];
					int targetTeam = target.teamID;
					int targetIndex = target.index;
					(int unitA, int unitB) unitPairsKey = UnitIndexPairsKey(thisIndex, targetIndex);
					var relation  = unitPairsList[unitPairsKey];
					Vector3 directionA2B = unitPairsKey.unitA == thisIndex ? relation.directionA2B : -relation.directionA2B;
					float distanceA2B = relation.spaceDistance;
					float distanceAndRadius = relation.distanceAndRadius;

					RaycastCommand raycastCommand = new RaycastCommand(relationItem.position, directionA2B, rayQuery, distanceAndRadius);
					newUnitRayList[thisIndex].Add((targetIndex, raycastCommand));
				}
			}
		}
		private void ReactionListComputeActuator(UnitValueInfo[] unitValueInfos, Dictionary<(int, int), UnitPairs> unitPairsList, Dictionary<int, int[]> newUnitReactionList)
		{
			List<(int,int)> listUnitKey = new List<(int,int)>();
			Dictionary<int, ConcurrentBag<int>> resultReactionList = new Dictionary<int, ConcurrentBag<int>>();
			foreach(KeyValuePair<int, int[]> unitReactionList in newUnitReactionList)
			{
				int length = unitReactionList.Value.Length;
				for(int i = 0 ; i < length ; i++)
				{
					listUnitKey.Add((unitReactionList.Key, unitReactionList.Value[i]));
				}
				resultReactionList[unitReactionList.Key] = new ConcurrentBag<int>();
			}
			if(UsingParallelForEach) Parallel.ForEach(listUnitKey, ForEach);
			else listUnitKey.ForEach(ForEach);
			void ForEach((int index1, int index2) unitPairIndex)
			{
				(int indexA, int indexB) unitPariKey = UnitIndexPairsKey(unitPairIndex);
				if(unitPairsList.TryGetValue(unitPariKey, out var relation))
				{
					var orderActuator = ActuatorList[unitPairIndex.index1];
					var orderAbility = AbilityList[unitPairIndex.index1];
					var targetAbility = AbilityList[unitPairIndex.index2];
					int targetLayer = unitValueInfos[unitPairIndex.index2].reactionLayer;
					UnitRelationType unitRelationType = relation.unitRelationType;
					var pairList = (unitPariKey.indexA == unitPairIndex.index1) ? relation.canUseActuatorListA2B : relation.canUseActuatorListB2A;
					pairList.Clear();
					if(orderActuator.TryGetCanAnyActuatorTarget(orderAbility, targetAbility, unitRelationType, targetLayer, out var list))
					{
						pairList.AddRange(list);
						resultReactionList[unitPairIndex.index1].Add(unitPairIndex.index2);
					}
					unitPairsList[unitPariKey] = relation;
				}
			}
			foreach(var unitReactionList in resultReactionList)
			{
				newUnitReactionList[unitReactionList.Key] = unitReactionList.Value.ToArray();
			}
		}

		private void NewVisibleUpdate(Dictionary<int, ObjectBehaviour[]> oldTeamVisibleList, Dictionary<int, int[]> teamVisibleIndexList, out Dictionary<int, ObjectBehaviour[]> newTeamVisibleList)
		{
			if(oldTeamVisibleList == null) oldTeamVisibleList = new Dictionary<int, ObjectBehaviour[]>();
			var tempTeamVisibleList = new Dictionary<int, ObjectBehaviour[]>();
			if(teamVisibleIndexList != null)
			{
				foreach(var item in teamVisibleIndexList)
				{
					tempTeamVisibleList[item.Key] = item.Value?.Select(i => ObjectList[i]).Where(i => i!=null).ToArray() ?? new ObjectBehaviour[0];
				}
			}
			{
				int teamIndex = IFFProvider.UserControlTeamID;
				var oldList = oldTeamVisibleList.TryGetValue(teamIndex, out var _oldList) ? _oldList : new ObjectBehaviour[0];
				var newList = tempTeamVisibleList.TryGetValue(teamIndex, out var _newList) ? _newList : new ObjectBehaviour[0];

				var addedItems = newList.Except(oldList).ToArray();
				foreach(var item in addedItems)
				{
					OnNewItem(item);
				}
				var removedItems = oldList.Except(newList).ToArray();
				foreach(var item in removedItems)
				{
					OnOldItem(item);
				}
			}
			newTeamVisibleList = tempTeamVisibleList;

			void OnNewItem(ObjectBehaviour item)
			{
				if(item.ThisContainer.TryGetComponent<ICombatVisibleListener>(out var listener))
				{
					listener.OnShowVisible();
				}
			}
			void OnOldItem(ObjectBehaviour item)
			{
				if(item.ThisContainer.TryGetComponent<ICombatVisibleListener>(out var listener))
				{
					listener.OnHideVisible();
				}
			}
		}
		private void NewReactionUpdate(Dictionary<int, ObjectBehaviour[]> oldUnitReactionList, Dictionary<int, int[]> unitReactionIndexList, out Dictionary<int, ObjectBehaviour[]> newUnitReactionList)
		{
			if(oldUnitReactionList == null) oldUnitReactionList = new Dictionary<int, ObjectBehaviour[]>();
			var tempUnitReactionList = new Dictionary<int, ObjectBehaviour[]>();
			if(unitReactionIndexList != null)
			{
				foreach(var item in unitReactionIndexList)
				{
					tempUnitReactionList[item.Key] = item.Value?.Select(i => ObjectList[i]).Where(i => i!=null).ToArray() ?? new ObjectBehaviour[0];
				}
			}
			if(UsingParallelForEach) Parallel.For(0, AllUnitLength, ForEach);
			else for(int i = 0 ; i < AllUnitLength ; i++) ForEach(i);
			void ForEach(int unitIndex)
			{
				var orderUnit = ObjectList[unitIndex];
				var oldList = oldUnitReactionList.TryGetValue(unitIndex, out var _oldList) ? _oldList : new ObjectBehaviour[0];
				var newList = tempUnitReactionList.TryGetValue(unitIndex, out var _newList) ? _newList : new ObjectBehaviour[0];

				var addedItems = newList.Except(oldList).ToArray();
				foreach(var item in addedItems)
				{
					OnNewItem(orderUnit, item);
				}
				var removedItems = oldList.Except(newList).ToArray();
				foreach(var item in removedItems)
				{
					OnOldItem(orderUnit, item);
				}
			}
			newUnitReactionList = tempUnitReactionList;

			void OnNewItem(ObjectBehaviour order, ObjectBehaviour target)
			{
				if(order.ThisContainer.TryGetComponent<ICombatReactionListener>(out var listener))
				{
					listener.OnInReactionRange(target);
				}
			}
			void OnOldItem(ObjectBehaviour order, ObjectBehaviour target)
			{
				if(order.ThisContainer.TryGetComponent<ICombatReactionListener>(out var listener))
				{
					listener.OnOutReactionRange(target);
				}
			}
		}
	}
}
