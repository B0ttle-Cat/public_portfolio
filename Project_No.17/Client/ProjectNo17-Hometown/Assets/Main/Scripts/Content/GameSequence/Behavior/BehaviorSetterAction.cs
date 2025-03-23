using System;

using TFContent;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BehaviorSetter", story: "[Setter]", category: "Action", id: "a504db62f934ae541c703a5b2a9dbdf9")]
public partial class BehaviorSetterAction : Action
{
	[SerializeReference] public BlackboardVariable<BehaviorSetter> Setter;

	protected override Status OnStart()
	{
		if(Setter == null) return Status.Failure;
		if(Setter.Value == null) return Status.Failure;

		Setter.Value.Setup();
		return Status.Success;
	}

	protected override Status OnUpdate()
	{
		return Status.Success;
	}

	protected override void OnEnd()
	{
	}
}

