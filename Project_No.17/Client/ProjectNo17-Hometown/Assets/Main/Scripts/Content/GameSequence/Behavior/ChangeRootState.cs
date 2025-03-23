using System;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/ChangeRootState")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "ChangeRootState", message: "Root 상태를 [rootState] 로 변경합니다.", category: "GamePlay", id: "0e04fdcec1e967721628e4b57c6f5af3")]
public partial class ChangeRootState : EventChannelBase
{
	public delegate void ChangeRootStateEventHandler(GamePlayRootState rootState);
	public event ChangeRootStateEventHandler Event;

	public void SendEventMessage(GamePlayRootState rootState)
	{
		Event?.Invoke(rootState);
	}

	public override void SendEventMessage(BlackboardVariable[] messageData)
	{
		BlackboardVariable<GamePlayRootState> rootStateBlackboardVariable = messageData[0] as BlackboardVariable<GamePlayRootState>;
		var rootState = rootStateBlackboardVariable != null ? rootStateBlackboardVariable.Value : default(GamePlayRootState);

		Event?.Invoke(rootState);
	}

	public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
	{
		ChangeRootStateEventHandler del = (rootState) =>
		{
			BlackboardVariable<GamePlayRootState> var0 = vars[0] as BlackboardVariable<GamePlayRootState>;
			if(var0 != null)
				var0.Value = rootState;

			callback();
		};
		return del;
	}

	public override void RegisterListener(Delegate del)
	{
		Event += del as ChangeRootStateEventHandler;
	}

	public override void UnregisterListener(Delegate del)
	{
		Event -= del as ChangeRootStateEventHandler;
	}
}

