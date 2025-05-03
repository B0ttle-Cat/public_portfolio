using System.Collections.Generic;
using System.Linq;

using BC.Actuator;
using BC.AIProvider;
using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using UnityEngine;

namespace TFContent
{
	public partial class CombatComputeComponent : ComponentBehaviour, IOdccUpdate, ICombatCompute, ISaveLoadTarget
	{
		public IFFMatchingObjectData iffMatchingInfoData;
		private bool isBlockUpdateOnSaveLoad;

		private Queue<(ObjectBehaviour,bool)> addBufferList = new Queue<(ObjectBehaviour,bool)>();
		private ObjectBehaviour[]   ObjectList;
		//private UnitCommandData[]   DataList;
		private AbilityProvider[]   AbilityList;
		private ActuatorProvider[]  ActuatorList;
		private IFFProvider[]       IFFList;
		private ReactionTarget[]    ReactionList;
		private bool somethingRemove;

		[SerializeField]
		private LayerMask blockingVisibleLayerMask;
		[SerializeField]
		private LayerMask blockingReactionLayerMask;
		[ShowInInspector]
		private OdccQuerySystem combatQuery;
		[ShowInInspector]
		private OdccQueryCollector combatCollector;
		public abstract class CombatSystem
		{
			public CombatComputeComponent CombatCompute;
			public abstract void SystemEnable();
			public abstract void SystemDisable();
			public abstract void SystemUpdate();
		}
		protected override void BaseAwake()
		{
			IFFProvider.IFFMatching = iffMatchingInfoData;
			isBlockUpdateOnSaveLoad = false;
			addBufferList = new Queue<(ObjectBehaviour, bool)>();
			combatQuery =  OdccQueryBuilder.CreateQuery().WithAll<CharacterObject, AbilityProvider, IFFProvider, ReactionTarget>().Build();
			combatCollector = OdccQueryCollector.CreateQueryCollector(combatQuery, this)
				.CreateChangedListEvent(UpdateCombatObject)
				.GetCollector();
		}

		private async void UpdateCombatObject(ObjectBehaviour update, bool isAdd)
		{
			addBufferList.Enqueue((update, isAdd));
			if(!isAdd)
			{
				somethingRemove =true;
			}
		}

		protected override void BaseDestroy()
		{
			if(combatCollector != null)
			{
				combatCollector.DeleteChangedListEvent(UpdateCombatObject);
				combatCollector = null;
				combatQuery = null;
			}


			if(addBufferList != null) addBufferList.Clear();
			addBufferList = null;
			ObjectList = null;
			//DataList = null;
			AbilityList = null;
			ActuatorList = null;
			IFFList = null;
		}
		int IOdccUpdate.UpdatePriority => 1;
		void IOdccUpdate.BaseUpdate()
		{
			if(iffMatchingInfoData == null) return;
			if(isBlockUpdateOnSaveLoad) return;
			OnCombatCompute();
		}

		UnitRelationType ICombatCompute.GetRelationType(int thisUnitIndex, int targetUnitIndex)
		{
			if(thisUnitIndex < 0) return UnitRelationType.Neutral;
			if(targetUnitIndex < 0) return UnitRelationType.Neutral;

			return unitPairsList != null && unitPairsList.TryGetValue(UnitIndexPairsKey(thisUnitIndex, targetUnitIndex), out var unitPair)
				? unitPair.unitRelationType
				: UnitRelationType.Neutral;
		}
		bool ICombatCompute.IsVisibleTarget(int thisUnitIndex, int targetUnitIndex)
		{
			if(thisUnitIndex < 0) return false;
			if(targetUnitIndex < 0) return false;

			int thisTeamID = IFFList[thisUnitIndex].TeamID;
			int targetUnitID = IFFList[targetUnitIndex].UnitID;

			if(computeTeamVisibleIndexList.TryGetValue(thisTeamID, out var visibleList))
			{
				int length = visibleList.Length;
				for(int i = 0 ; i < length ; i++)
				{
					var iffInfo = IFFList[visibleList[i]];
					if(iffInfo != null && iffInfo.UnitID == targetUnitID)
					{
						return true;
					}
				}
			}
			return false;
		}
		bool ICombatCompute.IsReactionTarget(int thisUnitIndex, int targetUnitIndex)
		{
			if(thisUnitIndex < 0) return false;
			if(targetUnitIndex < 0) return false;

			int thisTeamID = IFFList[thisUnitIndex].TeamID;
			int targetUnitID = IFFList[targetUnitIndex].UnitID;

			if(computeUnitReactionIndexList.TryGetValue(thisTeamID, out var visibleList))
			{
				int length = visibleList.Length;
				for(int i = 0 ; i < length ; i++)
				{
					var iffInfo = IFFList[visibleList[i]];
					if(iffInfo != null && iffInfo.UnitID == targetUnitID)
					{
						return true;
					}
				}
			}
			return false;
		}
		bool ICombatCompute.GetUnitPairsValue(int thisUnitIndex, int targetUnitIndex, out UnitPairsTarget unitPairsTarget)
		{
			unitPairsTarget = default;
			if(thisUnitIndex < 0) return false;
			if(targetUnitIndex < 0) return false;

			var unitIndexPairsKey = UnitIndexPairsKey(thisUnitIndex, targetUnitIndex);
			if(unitPairsList != null && unitPairsList.TryGetValue(unitIndexPairsKey, out var unitPair))
			{
				unitPairsTarget = unitPair.ConvertUnitPairsTarget(unitIndexPairsKey.indexA == thisUnitIndex, this);
				return true;
			}
			return false;
		}
		bool ICombatCompute.GetUnitPairsValue(int thisUnitIndex, int[] targetUnitIndexs, out UnitPairsTarget[] unitPairsTargets)
		{
			unitPairsTargets = null;
			if(thisUnitIndex < 0) return false;
			if(targetUnitIndexs == null || targetUnitIndexs.Length == 0) return false;

			int length = targetUnitIndexs.Length;
			unitPairsTargets = new UnitPairsTarget[length];

			for(int i = 0 ; i < length ; i++)
			{
				int targetUnitIndex = targetUnitIndexs[i];
				if(targetUnitIndex < 0)
				{
					unitPairsTargets[i] = default;
					continue;
				}

				var unitIndexPairsKey = UnitIndexPairsKey(thisUnitIndex, targetUnitIndex);
				if(unitPairsList != null && unitPairsList.TryGetValue(unitIndexPairsKey, out var unitPair))
				{
					unitPairsTargets[i] = unitPair.ConvertUnitPairsTarget(unitIndexPairsKey.indexA == thisUnitIndex, this);
				}
				else
				{
					unitPairsTargets[i] = default;
				}
			}
			return true;
		}
		bool ICombatCompute.GetUnitAllPairsValue(int thisUnitIndex, out UnitPairsTarget[] allPairsResult)
		{
			allPairsResult = null;
			if(thisUnitIndex < 0) return false;

			allPairsResult = unitPairsList.Where(pair => pair.Key.Item1 == thisUnitIndex || pair.Key.Item2 ==thisUnitIndex)
				.Select(pair => pair.Value.ConvertUnitPairsTarget(pair.Key.Item1 == thisUnitIndex, this))
				.ToArray();

			return allPairsResult == null || allPairsResult.Length == 0;
		}
		public int FindObjectIndex(ObjectBehaviour target)
		{
			if(ObjectList == null || target == null) return -1;
			int length = ObjectList.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var check = ObjectList[i];
				if(check != null && check == target) return i;
			}
			return -1;
		}
		ObjectBehaviour ICombatCompute.FindObjectBehaviour(int index) => ObjectList[index];
		AbilityProvider ICombatCompute.FindAbilityProvider(int index) => AbilityList[index];
		ActuatorProvider ICombatCompute.FindActuatorProvider(int index) => ActuatorList[index];
		IFFProvider ICombatCompute.FindIFFProvider(int index) => IFFList[index];
		ReactionTarget ICombatCompute.FindReactionTarget(int index) => ReactionList[index];


		public void OnSaveComplete()
		{
			isBlockUpdateOnSaveLoad = true;
		}
		void ISaveLoadData.OnSave(ref SaveLoadData data) { }
		void ISaveLoadData.OnLoad(in SaveLoadData data) { }
		public void OnLoadComplete()
		{
			addBufferList?.Clear();
			isBlockUpdateOnSaveLoad = false;
			List<ObjectBehaviour> _ObjectList = combatCollector.GetQueryItemList(true);
			//	List<UnitCommandData> _DataList = new List<UnitCommandData>(DataList ?? new UnitCommandData[0]);
			List<AbilityProvider> _AbilityList = new List<AbilityProvider>(AbilityList ?? new AbilityProvider[0]);
			List<ActuatorProvider> _ActuatorList = new List<ActuatorProvider>(ActuatorList ?? new ActuatorProvider[0]);
			List<IFFProvider> _IFFList = new List<IFFProvider>(IFFList?? new IFFProvider[0]);
			List<ReactionTarget> _AttackingList = new List<ReactionTarget>(ReactionList?? new ReactionTarget[0]);

			ObjectList = _ObjectList.ToArray();
			int length = ObjectList.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var update = ObjectList[i];
				_AbilityList.Add(update.ThisContainer.GetComponent<AbilityProvider>());
				_ActuatorList.Add(update.ThisContainer.GetComponent<ActuatorProvider>());
				_IFFList.Add(update.ThisContainer.GetComponent<IFFProvider>());
				_AttackingList.Add(update.ThisContainer.GetComponent<ReactionTarget>());
			}
			AbilityList = _AbilityList.ToArray();
			ActuatorList =_ActuatorList.ToArray();
			IFFList = _IFFList.ToArray();
			ReactionList = _AttackingList.ToArray();

			AllUnitLength = length;
			AllTeamLength = iffMatchingInfoData.IFFMatchingInfo.Length;
		}
	}
}