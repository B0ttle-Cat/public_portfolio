using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/ExitPlayGame")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "ExitPlayGame", message: "Game 종료 이벤트", category: "Events", id: "2d3a2d251b98ea8f938c267d0ad4fad9")]
public partial class ExitPlayGame : EventChannelBase
{
    public delegate void ExitPlayGameEventHandler();
    public event ExitPlayGameEventHandler Event; 

    public void SendEventMessage()
    {
        Event?.Invoke();
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        Event?.Invoke();
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        ExitPlayGameEventHandler del = () =>
        {
            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as ExitPlayGameEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as ExitPlayGameEventHandler;
    }
}

