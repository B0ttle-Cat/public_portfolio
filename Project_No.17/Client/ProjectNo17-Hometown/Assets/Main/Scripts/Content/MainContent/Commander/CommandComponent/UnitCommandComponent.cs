using System;

using BC.AIProvider;
using BC.Base;
using BC.ODCC;

using Pathfinding;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using UnityEngine;
namespace TFContent
{
	public partial class UnitCommandComponent : ComponentBehaviour, IOdccUpdate
	{
		int IOdccUpdate.UpdatePriority => 2;

		public UnitCommandData CommandData;
		public ICombatCompute CombatCompute;

		public AbilityProvider UnitAbility;
		public CombatEffectUpdater EffectUpdater;
		public MovementCommandUpdater MovementUpdater;
		public CombatCommandUpdater CombatUpdater;

		protected override void BaseAwake()
		{
			if(!ThisContainer.TryGetData<UnitCommandData>(out CommandData))
			{
				CommandData = ThisContainer.AddData<UnitCommandData>();
			}
			UnitCommandState = new IdleStateObject(this);
			UnitCommandStateType = UnitCommandData.UnitCommandStateType.Idle;
		}
		protected override void BaseStart()
		{
			UnitAbility = ThisContainer.GetComponent<AbilityProvider>();
			EffectUpdater = ThisContainer.GetComponent<CombatEffectUpdater>();
			MovementUpdater = ThisContainer.GetComponent<MovementCommandUpdater>();
			CombatUpdater = ThisContainer.GetComponent<CombatCommandUpdater>();
		}
		void IOdccUpdate.BaseUpdate()
		{
			if(CombatCompute == null && EventManager.TryGet<ICombatCompute>(out var combatCompute))
			{
				CombatCompute = combatCompute;
			}
			if(CombatCompute?.ThisObject == null)
			{
				CombatCompute = null;
				return;
			}
			if(EffectUpdater != null) EffectUpdater.OnCommandUpdater();
			if(MovementUpdater != null) MovementUpdater.OnCommandUpdater();
			if(CombatUpdater != null) CombatUpdater.OnCommandUpdater();

			UnitCommandData.UnitCommandStateType prevCommandStateType = UnitCommandStateType;
			if(prevCommandStateType == UnitCommandData.UnitCommandStateType.None) return;
			if(UnitCommandState == null) return;

			UnitCommandData.UnitCommandStateType nextCommandStateType = UnitCommandState.CheckNextStateUpdate(in prevCommandStateType);

			if(prevCommandStateType != nextCommandStateType)
			{
				UnitCommandStateObject nextStateObject = NextStateChange(in nextCommandStateType);
				if(nextStateObject != null)
				{
					UnitCommandState.OnDisableState();
					UnitCommandStateType = nextCommandStateType;
					UnitCommandState = nextStateObject;
					UnitCommandState.OnEnableState();
				}
			}
			UnitCommandState.OnUpdateState();
		}
	}
	public partial class UnitCommandComponent // StateObject<TState>
	{
		protected abstract class StateObject<TState> : ISaveLoadData
		{
			public abstract TState CheckNextStateUpdate(in TState prevState);
			protected abstract void DisableState();
			protected abstract void EnableState();
			protected abstract void UpdateState();

			public abstract void OnSave(ref SaveLoadData data);
			public abstract void OnLoad(in SaveLoadData data);
		}
		[Serializable]
		protected abstract class UnitCommandStateObject : StateObject<UnitCommandData.UnitCommandStateType>
		{
			protected UnitCommandComponent UnitCommand { get; private set; }
			protected UnitCommandData CommandData => UnitCommand.CommandData;
			protected MovementCommandUpdater MovementUpdater => UnitCommand.MovementUpdater;
			protected CombatCommandUpdater CombatUpdater => UnitCommand.CombatUpdater;

			public UnitCommandStateObject(UnitCommandComponent unitCommandComponent)
			{
				UnitCommand = unitCommandComponent;
			}

			public void OnDisableState()
			{
				if(UnitCommand == null) return;
				DisableState();
				UnitCommand = null;
			}
			public void OnEnableState()
			{
				if(UnitCommand == null) return;
				EnableState();
			}
			public void OnUpdateState()
			{
				if(UnitCommand == null) return;
				UpdateState();
			}
		}

		[ShowInInspector, ReadOnly]
		private UnitCommandData.UnitCommandStateType UnitCommandStateType {
			get => CommandData?.unitCommandState ?? UnitCommandData.UnitCommandStateType.Idle;
			set {
				if(CommandData != null) { CommandData.unitCommandState = value; }
			}
		}
		[SerializeReference, ShowInInspector, ReadOnly]
		private UnitCommandStateObject UnitCommandState;

		protected virtual UnitCommandStateObject NextStateChange(in UnitCommandData.UnitCommandStateType nextCommandStateType)
		{
			return nextCommandStateType switch {
				UnitCommandData.UnitCommandStateType.Idle => new IdleStateObject(this),
				UnitCommandData.UnitCommandStateType.Move => new MoveStateObject(this),
				UnitCommandData.UnitCommandStateType.Combat => new CombatStateObject(this),
				UnitCommandData.UnitCommandStateType.Neutralize => new NeutralizeStateObject(this),
				_ => null,
			};
		}

	}
	public partial class UnitCommandComponent : ISaveLoadTarget
	{
		void ISaveLoadData.OnSave(ref SaveLoadData data)
		{
			data.SaveValue(nameof(UnitCommandStateType), UnitCommandStateType);

			if(UnitCommandState != null) data.SaveIData(nameof(UnitCommandState), UnitCommandState);
		}
		void ISaveLoadData.OnLoad(in SaveLoadData data)
		{
			if(data.LoadValue<UnitCommandData.UnitCommandStateType>(nameof(UnitCommandStateType), out var _UnitCommandStateType))
			{
				UnitCommandStateType = _UnitCommandStateType;
			}
		}
		void ISaveLoadData.OnLoadComplete()
		{
			if(!ThisContainer.TryGetData<UnitCommandData>(out CommandData))
			{
				CommandData = ThisContainer.AddData<UnitCommandData>();
			}
		}
	}
	public partial class UnitCommandComponent : IMovePositionListener
	{
		void IMovePositionListener.OnMovePosition(in Vector3 position, in Vector3 look)
		{
			CommandData.OnMovePosition(in position, in look);
		}
		void IMovePositionListener.OnSetPosition(in Vector3 position, in Vector3 look)
		{
			CommandData.OnSetPosition(in position, in look);
		}
		bool IMovePositionListener.IsPathPossible(GraphNode node)
		{
			if(MovementUpdater == null) return false;
			return MovementUpdater.IsPathPossible(node);
		}
	}
	public partial class UnitCommandComponent : ICombatVisibleListener, ICombatReactionListener
	{
		void ICombatReactionListener.OnInReactionRange(ObjectBehaviour targetUnit)
		{
			CommandData.OnInReactionRange(targetUnit);
		}

		void ICombatReactionListener.OnOutReactionRange(ObjectBehaviour targetUnit)
		{
			CommandData.OnOutReactionRange(targetUnit);
		}

		void ICombatVisibleListener.OnShowVisible()
		{
		}

		void ICombatVisibleListener.OnHideVisible()
		{
		}
	}
}