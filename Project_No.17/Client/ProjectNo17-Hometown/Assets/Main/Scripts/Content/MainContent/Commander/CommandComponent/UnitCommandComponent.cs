using System;

using BC.AIProvider;
using BC.Base;
using BC.ODCC;

using Pathfinding;

using Sirenix.OdinInspector;

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

		[ShowInInspector, ReadOnly]
		private UnitCommandData.UnitCommandStateType UnitCommandStateType {
			get => CommandData?.unitCommandState ?? UnitCommandData.UnitCommandStateType.Idle;
			set {
				if(CommandData != null) { CommandData.unitCommandState = value; }
			}
		}
		[SerializeReference, ShowInInspector, ReadOnly]
		private UnitCommandStateObject UnitCommandState;
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

			UnitCommandData.UnitCommandStateType nextCommandStateType = UnitCommandState.CheckNextStateType();

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
		protected abstract class StateObject<TState>
		{
			public abstract TState CheckNextStateType();
			protected abstract void DisableState();
			protected abstract void EnableState();
			protected abstract void UpdateState();
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