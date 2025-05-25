using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set My State", story: "Set [My] [State]", category: "Action", id: "7ac4d22d8d64721ac86732f139881893")]
public partial class SetMyStateAction : Action
{
    [SerializeReference] public BlackboardVariable<enemyState> My;
    [SerializeReference] public BlackboardVariable<enemyState> State;
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        My.Value = State.Value;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

