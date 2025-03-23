using System;

using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public abstract class CommandController : ComponentBehaviour { }
	public abstract class CommandController<TListener, TCommand> : CommandController, IPerInputUpdate where TListener : CommandListener<TCommand>
	{
		protected CommandSystem CommandSystem { get; private set; }

		protected bool selectedOnlyOne;
		[SerializeField]
		private OdccQuerySystem querySelectCommandable;
		[SerializeField]
		private OdccQueryCollector collectorSelectCommandable;
		[SerializeField]
		private OdccQueryLooper odccCommandLooper;

		[SerializeField, Space]
		private TCommand value;
		private bool blockLoop;

		public virtual TCommand Value {
			get => value;
			set {
				if(!isActiveAndEnabled || blockLoop) return;
				blockLoop = true;

				try { PreCommandValue(ref value); }
				catch(Exception ex) { Debug.LogException(ex); }

				this.value = value;
				odccCommandLooper?.RunAction();

				try { PostCommandValue(ref this.value); }
				catch(Exception ex) { Debug.LogException(ex); }

				blockLoop = false;
			}
		}

		protected override void BaseAwake()
		{
			blockLoop = false;
			value = default;

			CommandSystem = ThisContainer.GetObject<CommandSystem>();

			var odccQueryBuilder = OdccQueryBuilder.CreateQuery().WithAll<CharacterObject, UnitCommandComponent, SelectingHandle, TListener>();
			OnCreateQuery(ref odccQueryBuilder);
			querySelectCommandable = odccQueryBuilder.Build();
			collectorSelectCommandable = OdccQueryCollector.CreateQueryCollector(querySelectCommandable, ThisScene)
				.CreateActionEvent(nameof(CallCommandListener), out odccCommandLooper)
				.CallForeach<TListener>(CallCommandListener)
				.GetCollector()
				.CreateChangedListEvent(SelectChangedEvent)
				.GetCollector();
		}


		protected override void BaseEnable()
		{
			if(odccCommandLooper != null)
			{
				odccCommandLooper.sleep = false;
			}
		}
		protected override void BaseDisable()
		{
			if(odccCommandLooper != null)
			{
				odccCommandLooper.sleep = true;
			}
		}
		protected override void BaseDestroy()
		{
			if(collectorSelectCommandable != null)
			{
				collectorSelectCommandable.DeleteActionEvent(nameof(CallCommandListener));
				odccCommandLooper = null;
				collectorSelectCommandable = null;
				querySelectCommandable = null;
			}
		}
		protected virtual void CallCommandListener(OdccQueryLooper.LoopInfo loopInfo, TListener tListener)
		{
			tListener.CommandListenerUpdate(this, Value);
		}
		protected virtual void PreCommandValue(ref TCommand commandValue)
		{

		}
		protected virtual void PostCommandValue(ref TCommand commandValue)
		{

		}
		private void SelectChangedEvent(ObjectBehaviour behaviour, bool added)
		{
			selectedOnlyOne = collectorSelectCommandable.GetQueryItemCount() == 1;
		}

		protected virtual void OnCreateQuery(ref OdccQueryBuilder odccQueryBuilder) { }
		public abstract void OnPerInputEnable(UserInputSystem inputActions);
		public abstract void OnPerInputDisable(UserInputSystem inputActions);
		public abstract void OnPerInputUpdate(UserInputSystem inputActions);
	}
}