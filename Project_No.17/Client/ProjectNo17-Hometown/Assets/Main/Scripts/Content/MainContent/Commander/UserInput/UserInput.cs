using System.Collections.Generic;

using BC.ODCC;

using UnityEngine;
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

		[SerializeField]
		private OdccQuerySystem queryPerInputUpdate;
		[SerializeField]
		private OdccQueryCollector collectorPerInputUpdate;
		private OdccQueryLooper callPerInputUpdate;

		[SerializeField]
		private CursorLockMode cursorLockMode;

		protected override void BaseAwake()
		{
#if UNITY_EDITOR
			cursorLockMode = CursorLockMode.None;
#endif
			Cursor.lockState = cursorLockMode;

			inputActions ??=  new UserInputSystem();
			NumberInput = new NumberInput(inputActions.Number);

			queryPerInputUpdate = OdccQueryBuilder.CreateQuery().WithAll<IPerInputUpdate>().Build(ThisScene);
			collectorPerInputUpdate = OdccQueryCollector.CreateQueryCollector(queryPerInputUpdate, ThisScene)
				.CreateChangedListEvent(ChangeIPerInputUpdateList).GetCollector()
				.CreateActionEvent(nameof(callPerInputUpdate), looper: out callPerInputUpdate)
				.CallForeach<IPerInputUpdate>(PerInputUpdate).GetCollector();
		}
		protected override void BaseStart()
		{

			if(inputActions != null)
			{
				inputActions.Enable();
			}
		}
		protected override void BaseDestroy()
		{
			if(inputActions != null)
			{
				inputActions.Disable();

				inputActions.Dispose();
				inputActions = null;
			}

			if(collectorPerInputUpdate != null)
			{
				collectorPerInputUpdate.DeleteChangedListEvent(ChangeIPerInputUpdateList);
				collectorPerInputUpdate.DeleteActionEvent(nameof(callPerInputUpdate));
				collectorPerInputUpdate = null;
				callPerInputUpdate = null;
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

		private void ChangeIPerInputUpdateList(ObjectBehaviour behaviour, bool added)
		{
			if(added)
			{
				if(behaviour.ThisContainer.TryGetComponentList<IPerInputUpdate>(out var list))
				{
					list.ForEach(item => item.OnPerInputEnable(inputActions));
				}
			}
			else
			{
				if(behaviour.ThisContainer.TryGetComponentList<IPerInputUpdate>(out var list))
				{
					list.ForEach(item => item.OnPerInputDisable(inputActions));
				}
			}
		}
		private void PerInputUpdate(OdccQueryLooper.LoopInfo loopInfo, List<IPerInputUpdate> listener)
		{
			listener.ForEach(i => i.OnPerInputUpdate(inputActions));
		}

	}
}