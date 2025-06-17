using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms; // 이 using은 사용되지 않는 것 같습니다. 필요 없다면 제거하세요.

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


    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;     // 총알 프리팹
    [SerializeField] private float bulletLifeTime = 3f;   // 총알이 사라지는 시간
    private GameObject[] bulletPool;      // 총알 풀
    public int poolSize = 20;             // 총알 풀의 크기
    public float bulletDamage = 1f;       // 총알 피해량

    [Header("Dash Settings")]
    // Dash 쿨타임 관련 변수 추가
    [SerializeField] private float dashForceMultiplier = 3f; // 대시 속도 배수
    [SerializeField] private float dashDuration = 0.15f; // 대시 지속 시간
    [SerializeField] private float dashCooldownTime = 0.55f; // 대시 쿨타임
    private bool isDashing = false; // 대시 중인지 여부

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("Player_Script: Rigidbody component not found on Player!", this);
            enabled = false; // Rigidbody 없으면 스크립트 비활성화
            return;
        }

        playerStatus = Status.IDLE;
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null) Debug.LogError("PlayerInput not found!");

        moveAction = playerInput.actions["MOVE"];

        attackAction = playerInput.actions["Attack"];
        attackAction.performed += OnAttackAction_performed;

        dashAction = playerInput.actions["Dash"];
        dashAction.performed += OnDashAction_performed;

        playerCamera = Camera.main;   // tag가 MainCamera인 카메라를 찾습니다.
        if (playerCamera == null) Debug.LogError("Main Camera not found! Please tag your main camera as 'MainCamera'.");
    }

    private void Start()
    {
        // Character_Script에서 상속받은 health 초기화
        health = maxHealth;
        // 총알 풀 초기화
        bulletPoolInit();

        // 체력 UI 업데이트
        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.UpdatePlayerHealthUI();
        }
    }

    private void Update()
    {
        mouseScreenPosition = Pointer.current.position.ReadValue();
        HeadMousePos();      // 캐릭터가 마우스를 바라보게 함

        // 이동 입력 방향 업데이트 (FixedUpdate에서 사용)
        Vector2 inputVector = moveAction.ReadValue<Vector2>();
        playerDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        playerStatus = inputVector != Vector2.zero ? Status.MOVE : Status.IDLE;
    }

    // 물리 업데이트는 FixedUpdate에서 처리합니다.
    private void FixedUpdate()
    {
        // 대시 중이 아닐 때만 일반 이동 처리
        if (!isDashing && playerStatus == Status.MOVE)
        {
            Move(playerDirection); 
        }
    }

    protected override void Move(Vector3 direction)
    {
        // 방향 벡터가 유효한지 확인
        if (direction.magnitude > 0.1f) // 작은 값으로 입력 노이즈 방지
        {
            Vector3 targetPosition = playerRigidbody.position + direction * characterSpeed * Time.fixedDeltaTime;
            playerRigidbody.MovePosition(targetPosition);
        } else {
            // 입력이 없으면 Rigidbody 속도를 0으로 설정하여 정지
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
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
        // 풀이 부족하면 새로 생성
        GameObject newBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        newBullet.SetActive(false);

        return newBullet; 
    }

    public override void Attack(Vector3 targetScreenPosition, string targetTag = "Enemy")
    {
        // 총알 발사
        GameObject bullet = GetBulletFromPool();
        if (bullet != null)
        {
            // Raycast를 사용하여 마우스 위치에서 맵 평면으로 레이를 쏘는 방식
            Ray ray = playerCamera.ScreenPointToRay(targetScreenPosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position); // 플레이어 높이에 있는 수평 평면
            float distance;

            if (groundPlane.Raycast(ray, out distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                Vector3 targetDirection = (worldPoint - transform.position).normalized;
                targetDirection.y = 0f; // Y축은 0으로 고정하여 XZ 평면에서만 공격

                // 총알 설정
                bullet.GetComponent<Bullet_Script>().SetBullet(
                    10f, 5f, 5f, bulletDamage, targetTag, bulletLifeTime,
                    targetDirection, this.transform.position, 0); // startAngle 대신 targetDirection 사용
                bullet.SetActive(true); // 총알 활성화
            }
            else
            {
                Debug.LogWarning("Player_Script: Could not find ground point for mouse position for attack.");
            }
        }
    }

    private void HeadMousePos()      //캐릭터를 마우스 포인터를 바라보도록 회전 시킴
    {
        // Raycast를 사용하여 마우스 위치에서 맵 평면으로 레이를 쏘는 방식
        Ray ray = playerCamera.ScreenPointToRay(mouseScreenPosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position); // 플레이어 높이에 있는 수평 평면
        float distance;

        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            Vector3 lookDirection = (worldPoint - transform.position).normalized;
            lookDirection.y = 0f;         // y축은 0으로 고정하여 xz 평면에서 회전하도록 함

            if (lookDirection.magnitude > 0.1f) // 유효한 방향이 있을 때만 회전
            {
                this.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    private void OnAttackAction_performed(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();
        if (input > 0f)
        {
            Attack(mouseScreenPosition, "Enemy"); // 공격 메소드 호출
        }
    }

    // 대시-----------------------------------
    private IEnumerator Dashcooldown()
    {
        isDashing = true; // 대시 시작 플래그
        dashAction.Disable(); // 대시 액션 비활성화 (쿨타임 동안 대시를 사용할 수 없도록 함)
        
        // 대시 지속 시간
        yield return new WaitForSeconds(dashDuration); 

        // 대시 지속 시간 후 속도 초기화 (물리 엔진에 의한 움직임 정지)
        playerRigidbody.linearVelocity = Vector3.zero; 
        playerRigidbody.angularVelocity = Vector3.zero;
        isDashing = false; // 대시 종료 플래그

        // 대시 쿨타임
        yield return new WaitForSeconds(dashCooldownTime); 
        dashAction.Enable(); // 대시 액션 활성화 (다시 대시할 수 있도록 함)
    }

    public void Dash(Vector3 dashDirection)
    {
        if (isDashing) return; // 이미 대시 중이면 다시 대시하지 않음

        dashDirection.y = 0; // XZ 평면 대시
        dashDirection.Normalize(); // 대시 방향 벡터 정규화

        playerRigidbody.linearVelocity = Vector3.zero; // 기존 속도 초기화 (대시 시작 전)
        playerRigidbody.AddForce(dashDirection * characterSpeed * dashForceMultiplier, ForceMode.Impulse);

        StartCoroutine(Dashcooldown()); // 대시 쿨타임 시작
    }

    private void OnDashAction_performed(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();
        if (input > 0f)
        {
            Dash(playerDirection.magnitude > 0.1f ? playerDirection : transform.forward); // 입력 없으면 캐릭터 앞 방향으로 대시
        }
    }
    // -------------------------------------------------

    public override void GetHit(float damageAmount)
    {
        health -= damageAmount;
        Debug.Log("플레이어 체력 감소 : " + health);
        if (health <= 0)
        {
            health = 0; // 체력이 0보다 낮아지지 않도록
            Debug.Log(this.name.ToString() + "의 체력이 0이 되었습니다.");
            Die();
        }

        // 체력 UI 업데이트
        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.UpdatePlayerHealthUI();
        }
    }

    public override void Die()
    {
        Debug.Log("Player Died! Initiating Game Over sequence.");
        
        // 1. 플레이어 오브젝트 비활성화 (선택 사항)
        gameObject.SetActive(false); 

        // 2. GameManager에 점수 전달 및 게임 오버 처리 요청
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied();
        }
        else
        {
            Debug.LogError("Player_Script: GameManager Instance not found!", this);
            Time.timeScale = 0f; // GameManager가 없어도 최소한 시간은 멈춥니다.
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Player hit a wall! Collision.", this);
            if (!isDashing)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (!isDashing)
            {
                ContactPoint contact = collision.contacts[0];
                Vector3 pushDirection = contact.normal; // 벽에서 밀어내는 방향

                // 현재 입력 방향
                Vector3 currentInputDirection = playerDirection; 

                float dot = Vector3.Dot(currentInputDirection, pushDirection);

                if (dot > 0.01f) // 벽을 향해 이동하려고 할 때 (작은 오차 허용)
                {
                    // 벽과 충돌하는 방향의 움직임을 제거
                    Vector3 adjustedMoveDirection = currentInputDirection - pushDirection * dot;

                    Vector3 velocityInNormalDirection = Vector3.Project(playerRigidbody.linearVelocity, pushDirection);
                    if (Vector3.Dot(playerRigidbody.linearVelocity, pushDirection) > 0) // 벽을 향하는 속도만 제거
                    {
                         playerRigidbody.linearVelocity -= velocityInNormalDirection;
                    }
                    playerRigidbody.angularVelocity = Vector3.zero;

                }
            }
        }
    }
}