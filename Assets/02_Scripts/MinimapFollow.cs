using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    [Tooltip("�̴ϸ��� ����ٴ� Ÿ�� (Player �ν��Ͻ�)")]
    public Transform target;     // �� private �� public ���� ����

    [Tooltip("�̴ϸ� ī�޶� ����")]
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
