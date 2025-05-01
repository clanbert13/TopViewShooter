using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Script : Character_Script
{
	enum Status
    {
        IDLE,
        MOVE
    }

    private PlayerInput playerInput;
    private InputAction inputAction;
    private Vector3 playerDirection;
    private Status playerStatus;

    [SerializeField] private Camera playerCamera;
    private void Start()
    {
        playerStatus = Status.IDLE;
        playerInput = GetComponent<PlayerInput>();
        inputAction = playerInput.actions["MOVE"];
        inputAction.performed += OnMOVEAction_performed;

        playerCamera = Camera.main;   // tag가 MainCamera인 카메라를 찾습니다.
        if (playerCamera == null) Debug.LogError("Camera not found!");
    }
    private void Update()
    {
        HeadMousePos();     // 캐릭터가 마우스를 바라보게 함
        if (playerStatus == Status.MOVE)
        {
            Move(playerDirection);
        }
    }
    private void OnMOVEAction_performed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        playerStatus = input != Vector2.zero ? Status.MOVE : Status.IDLE;
        playerDirection = new Vector3(input.x, 0f, input.y);
        //Debug.Log("OnMOVEAction_performed");
    }

    private void HeadMousePos()
    {
        Vector3 mouseScreenPosition = Pointer.current.position.ReadValue();

        // 캐릭터의 현재 위치를 기준으로 바라볼 방향 벡터를 계산
        Vector3 lookDirection = playerCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, 
                                        mouseScreenPosition.y, playerCamera.transform.position.y  - 
                                                        transform.position.y)) - transform.position;
        lookDirection.y = 0f;         // y축은 0으로 고정하여 xz 평면에서 회전하도록 함

        this.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    public override void Attack()
    {
        // 공격 로직
    }

    public override void Die()
    {
        // 죽음 처리 로직
    }
}
