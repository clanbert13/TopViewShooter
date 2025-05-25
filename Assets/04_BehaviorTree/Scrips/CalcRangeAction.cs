using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Calc Range", story: "[Agent] calc [Range] between [Target]", category: "Action", id: "5db4ca5713c088de06cf2c24d725faac")]
public partial class CalcRangeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    protected override Status OnStart()
    {
        if (Agent.Value == null || Target.Value == null)
        {
            Debug.LogWarning("CalculateDistanceAction: Agent 또는 Target이 설정되지 않았습니다.");
            return Status.Failure; // 필요한 값이 없으면 실패
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // 두 오브젝트의 위치를 이용하여 거리 계산
        // 계산된 거리를 블랙보드 변수에 저장
        Range.Value = Vector3.Distance(Agent.Value.transform.position, Target.Value.transform.position);

        // Debug.Log($"Enemy to Player Distance: {distance}");
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

