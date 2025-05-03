using System;

using BC.ODCC;

using Sirenix.OdinInspector;

using TFSystem.SaveLoad;

using Unity.Cinemachine;

using UnityEngine;

namespace TFContent
{
	public class CameraMainUpdate : ComponentBehaviour, IOdccUpdate.Late, ISaveLoadTarget
	{
		#region 
		[Serializable]
		public struct UpdateList : IDisposable
		{
			[HideLabel]
			public MonoBehaviour target;
			private Action updateCallAction;
#if UNITY_EDITOR
			[HideIf("IsCanUpdate")]
			[HideLabel, ShowInInspector, DisplayAsString]
			string editor_IsCanUpdate => "경고! Update 에 대한 작업이 정의되지 않은 Target 입니다.";
			bool IsCanUpdate() => GetUpdateCall() != null;
#endif
			public void DoUpdate()
			{
				if(target == null)
				{
					updateCallAction = null;
					return;
				}

				if(updateCallAction == null)
				{
					updateCallAction = GetUpdateCall();
					if(updateCallAction != null) return;
				}

				updateCallAction.Invoke();
			}
			#endregion
			Action GetUpdateCall()
			{
				if(target == null) return null;
				switch(target)
				{
					case CinemachineBrain cinemachineBrain:
						cinemachineBrain.UpdateMethod = CinemachineBrain.UpdateMethods.ManualUpdate;
						return cinemachineBrain.ManualUpdate;
					case CinemachineTargetGroup cinemachineTargetGroup:
						cinemachineTargetGroup.enabled = false;
						return () => {
							if(cinemachineTargetGroup.Targets.Count > 0)
								cinemachineTargetGroup.DoUpdate();
						};
					case CameraController cameraController:
						return cameraController.CameraMainUpdate;
				}
				return null;
			}
			#region 
			public void Dispose()
			{
				target = null;
				updateCallAction = null;
			}
		}


		[SerializeField]
		private UpdateList[] updateLists;

		void IOdccUpdate.Late.BaseLateUpdate()
		{
			MainUpdate();
		}
		[Button("CameraMainUpdate")]
		public void MainUpdate()
		{
			if(updateLists != null)
			{
				for(int i = 0 ; i<updateLists.Length ; i++)
				{
					updateLists[i].DoUpdate();
				}
			}
		}

		protected override void BaseDestroy()
		{
			if(updateLists != null)
			{
				for(int i = 0 ; i<updateLists.Length ; i++)
				{
					updateLists[i].Dispose();
				}
			}
		}

		void ISaveLoadData.OnSave(ref SaveLoadData data) { }
		void ISaveLoadData.OnLoad(in SaveLoadData data) { }
		void ISaveLoadData.OnLoadComplete()
		{
			MainUpdate();
		}
		#endregion
	}
}
