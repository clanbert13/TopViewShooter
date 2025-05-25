using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AttacklTarget", story: "[Agent] Fires to [Target]", category: "Action", id: "bb4fbe7c9238d8020e12c8aab688ec7d")]
public partial class AttacklTargetAction : Action
{
    private Enemy_Script enemyScript;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnStart()
    {
        enemyScript = Agent.Value.GetComponent<Enemy_Script>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        enemyScript.Attack(Target.Value.transform.position, Target.Value.tag);
        Debug.Log("firing bullet)");
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

