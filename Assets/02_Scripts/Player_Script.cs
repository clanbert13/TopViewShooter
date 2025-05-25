using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;

public class Player_Script : Character_Script
{
	enum Status
    {
        IDLE,
        MOVE
    }

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction dashAction;
    private InputAction attackAction;
    private Vector3 playerDirection;
    private Status playerStatus;
    private Rigidbody playerRigidbody;
    private Camera playerCamera;
    private Vector3 mouseScreenPosition; // 마우스 스크린 위치

    [SerializeField] private GameObject bulletPrefab;     // 총알 프리팹
    private GameObject[] bulletPool;       // 총알 풀
    public int poolSize = 20;             // 총알 풀의 크기


    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerStatus = Status.IDLE;
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null) Debug.LogError("PlayerInput not found!");

        moveAction = playerInput.actions["MOVE"];
        moveAction.performed += OnMOVEAction_performed;

        attackAction = playerInput.actions["Attack"];
        attackAction.performed += OnAttackAction_performed;

        dashAction = playerInput.actions["Dash"];
        dashAction.performed += OnDashAction_performed;

        playerCamera = Camera.main;   // tag가 MainCamera인 카메라를 찾습니다.
        if (playerCamera == null) Debug.LogError("Camera not found!");
    }

    private void Start()
    {   
        // 캐릭터의 체력 초기화
        health = maxHealth;
        // 총알 풀 초기화
        bulletPoolInit();
    }

    private void Update()
    {
        mouseScreenPosition = Pointer.current.position.ReadValue();
        HeadMousePos();     // 캐릭터가 마우스를 바라보게 함
        if (playerStatus == Status.MOVE)
        {
            Move(playerDirection);
        }
    }

    // 총알 프리팹을 미리 생성하여 풀에 저장
    private void bulletPoolInit()
    {
        bulletPool = new GameObject[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            bulletPool[i] = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bulletPool[i].SetActive(false);     // 비활성화 상태로 초기화
        }
    }
    private GameObject GetBulletFromPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!bulletPool[i].activeInHierarchy)
            {
                return bulletPool[i];
            }
        }
        return null; // 사용할 수 있는 총알이 없음
    }
    public override void Attack(Vector3 targetPosition, string targetTag = "Enemy")
    {
        // 총알 발사
        GameObject bullet = GetBulletFromPool();
        if (bullet != null)
        {
            // 캐릭터의 현재 위치를 기준으로 바라볼 방향 벡터를 계산
            Vector3 targetDirection = playerCamera.ScreenToWorldPoint(new Vector3(targetPosition.x, 
                                        targetPosition.y, playerCamera.transform.position.y
                                                     - transform.position.y)) - transform.position;

            // 총알 설정
            bullet.GetComponent<Bullet_Script>().SetBullet(10f, 5f, 5f, targetTag, 2f,
                                        targetDirection, this.transform.position, 0);
            bullet.SetActive(true); // 총알 활성화
        }
    }


    private void OnMOVEAction_performed(InputAction.CallbackContext context)
    {
        //Debug.Log("OnMOVEAction_performed");
        Vector2 input = context.ReadValue<Vector2>();

        playerStatus = input != Vector2.zero ? Status.MOVE : Status.IDLE;
        playerDirection = new Vector3(input.x, 0f, input.y);
    }

    private void HeadMousePos()     //캐릭터를 마우스 포인터를 바라보도록 회전 시킴
    {
        // 캐릭터의 현재 위치를 기준으로 바라볼 방향 벡터를 계산
        Vector3 lookDirection = playerCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, 
                                        mouseScreenPosition.y, playerCamera.transform.position.y
                                                     - transform.position.y)) - transform.position;
        lookDirection.y = 0f;         // y축은 0으로 고정하여 xz 평면에서 회전하도록 함

        this.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    private void OnAttackAction_performed(InputAction.CallbackContext context)
    { 
        //Debug.Log("OnAttackAction_performed");
        float input = context.ReadValue<float>();
        if (input > 0f)
        {
            Attack(mouseScreenPosition); // 공격 메소드 호출
        }
    }

    // 대시-----------------------------------
    private IEnumerator Dashcooldown()
    {
        // 대시 쿨타임 로직
        yield return new WaitForSeconds(0.15f); // 대시 지속시간은 0.15초 
        playerRigidbody.linearVelocity = Vector3.zero; // 대시 후 속도 초기화

        yield return new WaitForSeconds(0.55f); // 대시 쿨타임
        dashAction.Enable(); // 대시 액션 활성화
    }
    public void Dash(Vector3 dashDirection)
    {
        //Debug.Log(playerDirection);
        dashAction.Disable(); // 대시 액션 비활성화
        dashDirection.Normalize();
        // 대시 속도는 이동 속도의 3배
        playerRigidbody.AddForce(dashDirection * characterSpeed * 3f, ForceMode.Impulse); 
        
        StartCoroutine(Dashcooldown()); // 대시 쿨타임 시작
    }
    private void OnDashAction_performed(InputAction.CallbackContext context)
    {
        //Debug.Log("OnDashAction_performed");
        float input = context.ReadValue<float>();
        if (input > 0f)
        {
            Dash(playerDirection); // 대시 메소드 호출
        }
    }
    // -------------------------------------------------

    public override void GetHit(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            // 게임 오버
            Debug.Log(this.name.ToString() + "의 체력이 0이 되었습니다.");
        }
    }

    public override void Die()
    {
        // 죽음 처리 로직
    }
}
