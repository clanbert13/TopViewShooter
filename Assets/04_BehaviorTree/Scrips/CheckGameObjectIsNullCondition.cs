using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Check GameObject is null", story: "[gameobject] is NULL", category: "Conditions", id: "42b6c158249d560a30c303bfe994dd0a")]
public partial class CheckGameObjectIsNullCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> gameobject;

    public override bool IsTrue()
    {
        if(gameobject == null || gameobject.Value == null)
        {
            return true;
        }
        return false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
