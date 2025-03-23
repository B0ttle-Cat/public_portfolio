using System;
using System.Collections.Generic;
using System.Linq;

using BC.Base;
using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TFSystem
{
	public class SceneController : ComponentBehaviour, ISceneController
	{
#if UNITY_EDITOR
		private static SceneController Editor_This => FindAnyObjectByType<SceneController>();
#endif
		private bool sceneLoadingNow;
		internal const string ApplicationScene = "ApplicationScene";

		[SerializeField, ReadOnly]
		private ISceneController.SceneState currentState;
		[ShowInInspector, ReadOnly]
		private Stack<ISceneController.SceneState> sceneChangeStack;
		public ISceneController.SceneState CurrentState { get => currentState; private set => currentState = value; }
		public Stack<ISceneController.SceneState> SceneChangeStack { get => sceneChangeStack; private set => sceneChangeStack = value; }


		#region Scene Controller Struct
		[Serializable]
		private struct SceneControlObject
		{
			[HorizontalGroup, HideLabel]
			public SceneState sceneControlObject;
			[HorizontalGroup, ShowInInspector, ReadOnly, HideLabel, PropertyOrder(1)]
			public string sceneName => sceneControlObject == null ? "" : sceneControlObject.TargetSceneName;
			[HorizontalGroup, ShowInInspector, ReadOnly, HideLabel, PropertyOrder(2)]
			private SceneState.SceneStateType CurrentSceneState => sceneControlObject == null ? (SceneState.SceneStateType)(-1) : sceneControlObject.CurrentSceneState;


			public async Awaitable Load()
			{
				if(sceneControlObject == null) return;

				sceneControlObject.enabled = true;
				while(sceneControlObject.CurrentSceneState is not SceneState.SceneStateType.Open)
				{
					await Awaitable.NextFrameAsync();
				}
			}
			public async Awaitable Unload()
			{
				if(sceneControlObject == null) return;

				sceneControlObject.enabled = false;
				while(sceneControlObject.CurrentSceneState is not SceneState.SceneStateType.Close)
				{
					await Awaitable.NextFrameAsync();
				}
			}
		}
		[SerializeField, Space]
		[ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
		private List<SceneControlObject> sceneObjects;

		[Serializable]
		private struct SceneStateGroup
		{
			[HorizontalGroup, HideLabel, PropertyOrder(0)]
			public ISceneController.SceneState sceneState;
#if UNITY_EDITOR
			[HorizontalGroup(Width = 90), HideLabel, PropertyOrder(1), ShowInInspector, EnableGUI, DisplayAsString]
			private string display1 => " Is Load This ";
#endif
			[HorizontalGroup, HideLabel, PropertyOrder(2)]
			public SceneState[] sceneObjects;
			public List<string> SceneNameList()
			{
				if(sceneObjects == null || sceneObjects.Length == 0) return new List<string>();
				return sceneObjects.Select(item => item.TargetSceneName).ToList();
			}
		}
		[SerializeField,Space]
		[ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
		private List<SceneStateGroup> sceneStateGroups;

		[Serializable]
		private struct StateLoadingScene
		{
#if UNITY_EDITOR
			[HorizontalGroup(Width = 35), HideLabel, PropertyOrder(0), ShowInInspector, EnableGUI, DisplayAsString]
			private string display1 => "From";
#endif
			[HorizontalGroup, HideLabel, PropertyOrder(1)]
			public ISceneController.SceneStateMask fromState;
#if UNITY_EDITOR
			[HorizontalGroup(Width = 20), HideLabel, PropertyOrder(2), ShowInInspector, EnableGUI, DisplayAsString]
			private string display2 => " To ";
#endif
			[HorizontalGroup, HideLabel, PropertyOrder(3)]
			public ISceneController.SceneStateMask toState;
#if UNITY_EDITOR
			[HorizontalGroup(Width = 20), HideLabel, PropertyOrder(4), ShowInInspector, EnableGUI, DisplayAsString]
			private string display3 => " Is ";
#endif
			[HorizontalGroup, HideLabel, PropertyOrder(5)]
			public SceneState sceneControlObject;
			public string loadSceneName => sceneControlObject == null ? "" : sceneControlObject.TargetSceneName;

			[HorizontalGroup("Time", order: 1, Width = 110), LabelText("Minimum Time"), ToggleLeft]
			public bool useMinimumShowTime;
			[HorizontalGroup("Time"), HideLabel]
			[EnableIf("@useMinimumShowTime"), UnityEngine.Range(0, 10)]
			public float minimumShowTime;
			public bool IsThis(ISceneController.SceneState _fromState, ISceneController.SceneState _toState)
			{
				ISceneController.SceneStateMask fromStateMask = (ISceneController.SceneStateMask)(1 << (int)_fromState);
				ISceneController.SceneStateMask toStateMask = (ISceneController.SceneStateMask)(1 << (int)_toState);
				bool from = fromState == ISceneController.SceneStateMask.AnyState || fromState.HasFlag(fromStateMask);
				bool to = toState == ISceneController.SceneStateMask.AnyState || toState.HasFlag(toStateMask);
				return from && to;
			}
		}
		[SerializeField, Space]
		[ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
		private List<StateLoadingScene> stateLoadingScenes;
		#endregion

		protected override void BaseAwake()
		{
			currentState = ISceneController.SceneState.NoneState;
			sceneChangeStack = new Stack<ISceneController.SceneState>();
			sceneChangeStack.Push(currentState);
		}

		async Awaitable<bool> ISceneController.ChangeSceneState(ISceneController.SceneState nextState)
		{
			await DelayCommand();
			async Awaitable DelayCommand()
			{
				while(sceneLoadingNow)
				{
					await Awaitable.NextFrameAsync();
				}
				sceneLoadingNow = true;
			}

			if(!CheckNextSceneStateGroupIndex(out int nextGroupIndex))
			{
				sceneLoadingNow = false;
				return false;
			}
			bool CheckNextSceneStateGroupIndex(out int nextGroupIndex)
			{
				nextGroupIndex = -1;
				if(CurrentState == nextState)
				{
					return false;
				}
				nextGroupIndex = sceneStateGroups.FindIndex(f => f.sceneState == nextState);
				return nextGroupIndex >= 0;
			}

			await ChangeSceneState(sceneStateGroups[nextGroupIndex]);
			async Awaitable ChangeSceneState(SceneStateGroup loadStateGroup)
			{
				string loadingScene = "";
				double loadingEndTime = Time.timeAsDouble;
				if(TryGetLoadingScene(CurrentState, nextState, out StateLoadingScene loadingSceneInfo))
				{
					loadingScene = loadingSceneInfo.loadSceneName;
					loadingEndTime = Time.timeAsDouble + loadingSceneInfo.minimumShowTime;
					await LoadScene(loadingScene);
				}

				List<string> nextSceneNames = loadStateGroup.SceneNameList();
				List<string> prevSceneNames = GetDynamicLoadedScene(loadingScene);
				RemoveDuplicatesSceneName(prevSceneNames, nextSceneNames);

				int prevCount = prevSceneNames.Count;
				for(int i = 0 ; i < prevCount ; i++)
				{
					await UnloadScene(prevSceneNames[i]);
				}

				CurrentState = nextState;
				if(SceneChangeStack.Contains(CurrentState))
				{
					while(SceneChangeStack.Peek() != CurrentState)
					{
						SceneChangeStack.Pop();
					}
				}
				else
				{
					SceneChangeStack.Push(CurrentState);
				}

				int nextCount = nextSceneNames.Count;
				for(int i = 0 ; i < nextCount ; i++)
				{
					await LoadScene(nextSceneNames[i]);
				}

				if(loadingScene.IsNotNullOrWhiteSpace())
				{
					while(Time.timeAsDouble < loadingEndTime)
					{
						await Awaitable.NextFrameAsync();
					}
					await UnloadScene(loadingScene);
				}
			}

			sceneLoadingNow = false;
			return true;
		}
		async void ISceneController.ChangeSceneState(ISceneController.SceneState nextState, Action<bool, ISceneController.SceneState> callback)
		{
			ISceneController sceneController =  this;
			bool isChange = await sceneController.ChangeSceneState(nextState);
			callback?.Invoke(isChange, CurrentState);
		}

		#region OtherFunction
		bool TryGetLoadingScene(ISceneController.SceneState fromState, ISceneController.SceneState toState, out StateLoadingScene loadingSceneInfo)
		{
			int length = stateLoadingScenes.Count;
			for(int i = 0 ; i < length ; i++)
			{
				if(stateLoadingScenes[i].IsThis(fromState, toState))
				{
					loadingSceneInfo = stateLoadingScenes[i];
					return true;
				}
			}
			loadingSceneInfo = default;
			return false;
		}
		List<string> GetDynamicLoadedScene(string currentLoadingScene)
		{
			List<string> loadedScene = new List<string>();
			int sceneCount = SceneManager.sceneCount;
			for(int i = 0 ; i < sceneCount ; i++)
			{
				Scene scene = SceneManager.GetSceneAt(i);
				string loadedSceneName = scene.name;

				if(loadedScene.Contains(loadedSceneName))
				{
					continue;
				}

				if(loadedSceneName.Equals(ApplicationScene))
				{
					continue;
				}
				else if(currentLoadingScene.IsNotNullOrWhiteSpace() && loadedSceneName.Equals(currentLoadingScene))
				{
					continue;
				}

				loadedScene.Add(loadedSceneName);
			}
			return loadedScene;
		}
		void RemoveDuplicatesSceneName(List<string> prevSceneNames, List<string> nextSceneNames)
		{
			HashSet<string> duplicates = new HashSet<string>(prevSceneNames);
			duplicates.IntersectWith(nextSceneNames);
			prevSceneNames.RemoveAll(item => duplicates.Contains(item));
			nextSceneNames.RemoveAll(item => duplicates.Contains(item));
		}
		async Awaitable UnloadScene(string sceneName)
		{
			int index = sceneObjects.FindIndex(item => item.sceneName.Equals(sceneName));
			if(index < 0) return;
			await sceneObjects[index].Unload();
		}
		async Awaitable LoadScene(string sceneName)
		{
			int index = sceneObjects.FindIndex(item => item.sceneName.Equals(sceneName));
			if(index < 0) return;
			await sceneObjects[index].Load();
		}
		#endregion
	}
}