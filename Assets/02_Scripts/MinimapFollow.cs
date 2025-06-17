using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    [Tooltip("미니맵이 따라다닐 타겟 (Player 인스턴스)")]
    public Transform target;     // ← private → public 으로 변경

    [Tooltip("미니맵 카메라 높이")]
    public float height = 20f;

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = new Vector3(
            target.position.x,
            height,
            target.position.z
        );
    }
}
