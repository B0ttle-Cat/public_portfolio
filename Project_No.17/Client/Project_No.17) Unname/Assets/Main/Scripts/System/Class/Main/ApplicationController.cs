using BC.ODCC;

using UnityEngine;
namespace TFSystem
{
	public class ApplicationController : ObjectBehaviour
	{
		public static ApplicationController AppController;
		public static ISceneController SceneController { get; private set; }
		public static ISystemController SystemController { get; private set; }
		public static IResourcesController ResourcesController { get; private set; }

		[SerializeField, Space]
		private ISceneController.SceneState AppStartState = ISceneController.SceneState.MainMenuState;
#if UNITY_EDITOR
		public ISceneController.SceneState EditorOnly_AppStartState { get => AppStartState; set => AppStartState = value; }
#endif
		protected override void BaseAwake()
		{
			AppController = this;

			SaveGameFileIO.Init();
			//DataCarrier = ThisContainer.TryGetData<DataCarrier>(out var dataCarrier)
			//	? dataCarrier : ThisContainer.AddData<DataCarrier>();

			SceneController = ThisContainer.TryGetComponent<SceneController>(out var sceneController)
				? sceneController : ThisContainer.AddComponent<SceneController>();

			SystemController = ThisContainer.TryGetComponent<SystemController>(out var systemController)
				? systemController : ThisContainer.AddComponent<SystemController>();

			ResourcesController = ThisContainer.TryGetComponent<ResourcesController>(out var resourcesController)
				? resourcesController : ThisContainer.AddComponent<ResourcesController>();

		}
		protected override void BaseStart()
		{
			SceneController.ChangeSceneState(AppStartState, null);
		}

		private void OnApplicationQuit()
		{
			SaveGameFileIO.Save();
		}
	}
}