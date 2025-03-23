using System;
using System.Threading;

using BC.Base;
using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.AIProvider
{
	[Serializable]
	public abstract class ActuatorPlayer : ScriptableObject
	{
#if UNITY_EDITOR
		[ButtonGroup("This Script"), PropertyOrder(-1000)]
		private void PingScript()
		{
			string scriptName = GetType().Name;
			string scriptFileName = $"{scriptName}.cs";

			string[] guids = UnityEditor.AssetDatabase.FindAssets($"{scriptName} t:Script");

			foreach(string guid in guids)
			{
				string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				if(System.IO.Path.GetFileName(path) == scriptFileName)
				{
					// 에디터에서 해당 스크립트를 강조
					UnityEditor.EditorGUIUtility.PingObject(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
					break;
				}
			}
		}
		[ButtonGroup("This Script"), PropertyOrder(-1000)]
		private void OpenScript()
		{
			string scriptName = GetType().Name;
			string scriptFileName = $"{scriptName}.cs";

			string[] guids = UnityEditor.AssetDatabase.FindAssets($"{scriptName} t:Script");

			foreach(string guid in guids)
			{
				string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				if(System.IO.Path.GetFileName(path) == scriptFileName)
				{
					// 에디터에서 해당 파일 열기
					UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
					break;
				}
			}
		}

		protected virtual void OnValidate()
		{

		}
#endif
		protected Func<bool> cancelPlay;

		protected ObjectBehaviour order;
		protected ActuatorModule actuator;
		protected ObjectBehaviour targetObject;
		protected Vector3 targetPosition;
		protected CancellationToken cancellationToken;
		protected bool hasTarget;
		public async Awaitable<bool> OnStart(CancellationToken cancellationToken, ObjectBehaviour order, ActuatorModule actuator, ActuatorModule.ActuatorTargetParameter target, Func<bool> cancelPlay)
		{
			this.cancelPlay = cancelPlay;

			this.order = order;
			this.actuator = actuator;
			this.cancellationToken = cancellationToken;
			targetObject = target.TargetObject;
			targetPosition = target.TargetPosition;

			if(order == null || actuator == null) return false;
			hasTarget = this.targetObject != null;

			return await Start();
		}
		public async Awaitable<bool> OnCasting()
		{
			if(order == null || actuator == null) return false;
			if(hasTarget && targetObject == null) return false;

			return await Casting();
		}
		public async Awaitable<bool> OnAction()
		{
			if(order == null || actuator == null) return false;
			if(hasTarget && targetObject == null) return false;

			return await Action();
		}
		public async Awaitable<bool> OnCooldown()
		{
			if(order == null || actuator == null) return false;

			return await Cooldown();
		}
		protected abstract Awaitable<bool> Start();
		protected abstract Awaitable<bool> Casting();
		protected abstract Awaitable<bool> Action();
		protected abstract Awaitable<bool> Cooldown();

		public async void CancelActuator()
		{
			if(order == null || actuator == null) return;
			await OnCancelActuator();
			await AwaitableUtility.WaitTrue(() => OnCheckIdDestroy());
		}
		public async void EndedActuator()
		{
			if(order == null || actuator == null) return;
			await OnEndedActuator();
			await AwaitableUtility.WaitTrue(() => OnCheckIdDestroy());
		}
		protected abstract Awaitable OnCancelActuator();
		protected abstract Awaitable OnEndedActuator();
		protected abstract bool OnCheckIdDestroy();

		protected virtual void OnDestroy()
		{
			cancelPlay = null;
			order = null;
			actuator = null;
			targetObject = null;
			targetPosition = Vector3.zero;
			hasTarget = false;
		}
	}
}
