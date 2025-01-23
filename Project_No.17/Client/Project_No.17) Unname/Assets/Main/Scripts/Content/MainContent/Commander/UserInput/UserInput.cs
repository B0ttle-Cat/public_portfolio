using BC.ODCC;
namespace TFContent
{
	public interface IPerInputUpdate : IOdccComponent
	{
		public void OnPerInputEnable(UserInputSystem inputActions);
		public void OnPerInputDisable(UserInputSystem inputActions);
		public void OnPerInputUpdate(UserInputSystem inputActions);
	}

	public class UserInput : ComponentBehaviour, IOdccUpdate.Fast
	{
		public static NumberInput NumberInput { get; private set; }

		private UserInputSystem inputActions;

		private QuerySystem queryPerInputUpdate;
		private OdccQueryCollector collectorPerInputUpdate;
		private OdccQueryLooper callPerInputEnable;
		private OdccQueryLooper callPerInputDisable;
		private OdccQueryLooper callPerInputUpdate;
		#region ODCCFunction
		///Awake 대신 사용.
		protected override void BaseAwake()
		{
			queryPerInputUpdate = QuerySystemBuilder.CreateQuery().WithAll<IPerInputUpdate>().Build(ThisScene);
			collectorPerInputUpdate = OdccQueryCollector.CreateQueryCollector(queryPerInputUpdate, ThisScene)
				.CreateActionEvent(nameof(callPerInputEnable), out callPerInputEnable)
					.CallForeach<IPerInputUpdate>(PerInputEnable).GetCollector()
				.CreateActionEvent(nameof(callPerInputDisable), out callPerInputDisable)
					.CallForeach<IPerInputUpdate>(PerInputDisable).GetCollector()
				.CreateActionEvent(nameof(callPerInputUpdate), out callPerInputUpdate)
					.CallForeach<IPerInputUpdate>(PerInputUpdate).GetCollector();
		}
		private void PerInputEnable(OdccQueryLooper.LoopInfo loopInfo, IPerInputUpdate listener)
		{
			listener.OnPerInputEnable(inputActions);
		}

		private void PerInputDisable(OdccQueryLooper.LoopInfo loopInfo, IPerInputUpdate listener)
		{
			listener.OnPerInputDisable(inputActions);
		}

		private void PerInputUpdate(OdccQueryLooper.LoopInfo loopInfo, IPerInputUpdate listener)
		{
			listener.OnPerInputUpdate(inputActions);
		}

		///Start 대신 사용.
		protected override void BaseStart()
		{
			inputActions ??=  new UserInputSystem();
			if(inputActions != null)
			{
				inputActions.Enable();

				NumberInput = new NumberInput(inputActions.Number);

				callPerInputEnable?.RunAction();
			}
		}

		///OnDestroy 대신 사용
		protected override void BaseDestroy()
		{
			if(inputActions != null)
			{
				callPerInputDisable?.RunAction();

				inputActions.Disable();

				inputActions.Dispose();
				inputActions = null;
			}

			if(queryPerInputUpdate != null)
			{
				collectorPerInputUpdate.DeleteActionEvent(nameof(callPerInputEnable));
				collectorPerInputUpdate.DeleteActionEvent(nameof(callPerInputDisable));
				collectorPerInputUpdate.DeleteActionEvent(nameof(callPerInputUpdate));
				callPerInputEnable = null;
				callPerInputDisable = null;
				callPerInputUpdate = null;
				collectorPerInputUpdate = null;
			}

			if(NumberInput != null)
			{
				NumberInput.Dispose();
				NumberInput = null;
			}
		}

		void IOdccUpdate.Fast.BaseFastUpdate()
		{
			if(inputActions != null)
			{
				callPerInputUpdate?.RunAction();
			}
		}
		#endregion
	}
}