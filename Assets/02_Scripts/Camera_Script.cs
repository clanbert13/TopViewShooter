using UnityEngine;
using UnityEngine.InputSystem;

public class Camera_Script : MonoBehaviour
{
    private GameObject player; // 플레이어 오브젝트
    public Vector3 heightOffset = new Vector3(0f, 14f, 0f); // 카메라 높이 오프셋 (원하는 높이로 조절)
    [SerializeField, Range(0f, 1f)] private float lerpVal = 0.57f; // 카메라와 플레이어 사이의 거리가 조절됨

    private Camera mainCamera; // 메인 카메라

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) Debug.LogError("Player not found!");

        mainCamera = GetComponent<Camera>();
        if (mainCamera == null) Debug.LogError("Camera component not found on this GameObject!");
    }

    void LateUpdate()
    {

        // 마우스 스크린 좌표를 월드 좌표로 변환 (플레이어와 같은 z 평면으로 투영)
        Vector3 mousePosition = Pointer.current.position.ReadValue();
        mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, 
                                                    mousePosition.y, this.transform.position.y));



        // 부드럽게 카메라 위치 이동, 중간지점은 플레이어와 마우스 위치의 중간점
        // lerpVal에 따라 카메라와 플레이어 사이의 거리가 조절됨
        this.transform.position = Vector3.Lerp(player.transform.position, 
                (player.transform.position + mousePosition) * 0.5f, lerpVal);
        this.transform.position += heightOffset; // 높이 오프셋 추가
    }
}