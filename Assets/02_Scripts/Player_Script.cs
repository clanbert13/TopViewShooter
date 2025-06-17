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
    private Vector3 playerDirection; // 입력 방향을 저장하는 변수 (Update에서 입력 받고 FixedUpdate에서 사용)
    private Status playerStatus;
    private Rigidbody playerRigidbody;
    private Camera playerCamera;
    private Vector3 mouseScreenPosition; // 마우스 스크린 위치

    [Header("Movement Settings")]
    // characterSpeed는 Character_Script에 있다면 여기서는 public만 사용.
    // public float characterSpeed = 5f; // Character_Script에서 상속받는다고 가정

    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;     // 총알 프리팹
    [SerializeField] private float bulletLifeTime = 3f;   // 총알이 사라지는 시간 (endTime 대신 lifeTime으로 이름 변경)
    private GameObject[] bulletPool;      // 총알 풀
    public int poolSize = 20;             // 총알 풀의 크기
    public float bulletDamage = 1f;       // 총알 피해량 (damage와 구분)

    // Dash 쿨타임 관련 변수 추가
    [SerializeField] private float dashForceMultiplier = 3f; // 대시 속도 배수
    [SerializeField] private float dashDuration = 0.15f; // 대시 지속 시간
    [SerializeField] private float dashCooldownTime = 0.55f; // 대시 쿨타임
    private bool isDashing = false; // 대시 중인지 여부

    // 벽 충돌로 인한 정지는 Rigidbody가 처리하므로 별도의 isStoppedByCollision 플래그는 제거합니다.

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("Player_Script: Rigidbody component not found on Player!", this);
            enabled = false; // Rigidbody 없으면 스크립트 비활성화
            return;
        }

        // Rigidbody 초기 설정 (Inspector에서 Is Kinematic 해제, Freeze Rotation X,Z 권장)
        // playerRigidbody.isKinematic = false; // Inspector에서 설정
        // playerRigidbody.useGravity = true; // 필요에 따라 설정
        playerRigidbody.freezeRotation = true; // 캐릭터가 넘어지지 않도록 회전 고정

        playerStatus = Status.IDLE;
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null) Debug.LogError("PlayerInput not found!");

        moveAction = playerInput.actions["MOVE"];
        moveAction.performed += OnMOVEAction_performed;
        moveAction.canceled += OnMOVEAction_performed; // 이동 입력이 멈췄을 때도 상태 업데이트

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
        // 입력은 Update에서 처리하여 반응성을 높입니다.
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
            Move(playerDirection); // Move() 함수가 playerRigidbody.MovePosition()을 사용하도록 변경됨
        }
    }

    // 플레이어 이동 로직 (Rigidbody.MovePosition 사용)
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
            bulletPool[i] = Instantiate(bulletPrefab, transform.position, Quaternion.identity, transform.parent); // 부모 설정 (깔끔한 Hierarchy)
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
        // 풀이 부족하면 새로 생성 (선택 사항: 풀 크기 확장 또는 경고)
        Debug.LogWarning("Bullet pool is exhausted! Consider increasing poolSize.");
        GameObject newBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity, transform.parent);
        newBullet.SetActive(false);
        // 새로운 총알을 풀에 추가할 경우 풀 배열 크기 조정 필요.
        // 현재는 단순히 새로 생성하여 반환하지만, 실제 게임에서는 풀을 확장하는 로직 필요
        return newBullet; 
    }

    public override void Attack(Vector3 targetScreenPosition, string targetTag = "Enemy") // targetPosition을 targetScreenPosition으로 변경하여 명확하게
    {
        // 총알 발사
        GameObject bullet = GetBulletFromPool();
        if (bullet != null)
        {
            // 캐릭터의 현재 위치를 기준으로 바라볼 방향 벡터를 계산 (마우스 위치)
            // ScreenToWorldPoint의 Z 값은 카메라에서 얼마나 떨어진 깊이를 나타냅니다.
            // playerCamera.transform.position.y - transform.position.y 로 하면 z값이 0에 가까워질 수 있으므로,
            // 보통 카메라의 깊이에서 플레이어까지의 깊이를 더하거나 (orthographic),
            // Plane.Raycast를 사용하는 것이 더 정확합니다.
            // 여기서는 단순화하여 Z값을 고정된 값으로 사용하거나, Raycast 방식을 고려합니다.

            // Raycast를 사용하여 마우스 위치에서 맵 평면으로 레이를 쏘는 방식 (더 정확)
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


    private void OnMOVEAction_performed(InputAction.CallbackContext context)
    {
        // 이동 상태는 Update에서 Input.GetAxis("Horizontal") 등으로 직접 읽어 판단합니다.
        // 여기서는 단지 콜백이 발생했음을 알리는 용도입니다.
        // Vector2 input = context.ReadValue<Vector2>();
        // playerStatus = input != Vector2.zero ? Status.MOVE : Status.IDLE;
        // playerDirection = new Vector3(input.x, 0f, input.y); // playerDirection도 Update에서 매 프레임 업데이트
    }

    private void HeadMousePos()      //캐릭터를 마우스 포인터를 바라보도록 회전 시킴
    {
        // Raycast를 사용하여 마우스 위치에서 맵 평면으로 레이를 쏘는 방식 (더 정확)
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

        // 대시 지속 시간 동안 힘을 적용하거나 velocity를 직접 설정
        // AddForce(Impulse)는 한 번만 강한 힘을 주므로, 일정 시간동안 속도 유지하려면
        // 코루틴 내에서 직접 velocity를 설정하거나, 지속적으로 AddForce를 Apply
        // 여기서는 Dash()에서 AddForce를 한 번 적용하고, Coroutine에서 속도를 0으로 초기화
        
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
            // 대시 방향은 플레이어의 현재 이동 방향 (playerDirection) 또는 바라보는 방향 (transform.forward) 사용
            // 여기서는 현재 입력받은 이동 방향을 대시 방향으로 사용합니다.
            Dash(playerDirection.magnitude > 0.1f ? playerDirection : transform.forward); // 입력 없으면 캐릭터 앞 방향으로 대시
        }
    }
    // -------------------------------------------------

    public override void GetHit(float damageAmount) // damage 변수명 충돌 피하기 위해 damageAmount로 변경
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
        Debug.Log("Player Died!");
        // 죽음 처리 로직 (예: 오브젝트 비활성화, 게임 오버 화면, 애니메이션 트리거)
        gameObject.SetActive(false); 
    }

    // OnCollisionEnter 및 OnCollisionStay는 이제 Rigidbody.MovePosition()과 잘 연동됩니다.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Player hit a wall! Collision.", this);
            // Rigidbody.MovePosition()은 이미 충돌 해결을 시도하지만,
            // 필요하다면 여기에서 추가적인 정지 로직(velocity=0)을 넣을 수 있습니다.
            // 대시 중이 아니라면 충돌 시 바로 정지
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
            // 대시 중에는 벽을 뚫거나 통과하는 것을 방지하기 위해 물리적 상호작용을 허용
            // 일반 이동 시 벽을 따라 미끄러지는 효과를 원할 경우 이 로직 사용
            if (!isDashing)
            {
                ContactPoint contact = collision.contacts[0];
                Vector3 pushDirection = contact.normal; // 벽에서 밀어내는 방향

                // 현재 입력 방향
                Vector3 currentInputDirection = playerDirection; // Update에서 읽은 playerDirection 사용

                float dot = Vector3.Dot(currentInputDirection, pushDirection);

                if (dot > 0.01f) // 벽을 향해 이동하려고 할 때 (작은 오차 허용)
                {
                    // 벽과 충돌하는 방향의 움직임을 제거
                    Vector3 adjustedMoveDirection = currentInputDirection - pushDirection * dot;

                    // 이 조정된 방향으로 Rigidbody.MovePosition을 다시 호출하여 이동을 제한
                    // Time.fixedDeltaTime 대신 Time.deltaTime을 사용하거나,
                    // Rigidbody.velocity = adjustedMoveDirection * characterSpeed; 를 사용하는 것이 더 자연스러울 수 있습니다.
                    // playerRigidbody.MovePosition(playerRigidbody.position + adjustedMoveDirection * characterSpeed * Time.fixedDeltaTime);
                    
                    // 더 단순하게: 벽을 향하는 속도를 0으로 만듭니다.
                    // 현재 속도에서 벽 법선 방향으로의 속도 성분을 제거합니다.
                    Vector3 velocityInNormalDirection = Vector3.Project(playerRigidbody.linearVelocity, pushDirection);
                    if (Vector3.Dot(playerRigidbody.linearVelocity, pushDirection) > 0) // 벽을 향하는 속도만 제거
                    {
                         playerRigidbody.linearVelocity -= velocityInNormalDirection;
                    }
                    playerRigidbody.angularVelocity = Vector3.zero;

                    // 또는, 입력이 벽을 향할 때는 속도 0
                    // playerRigidbody.velocity = Vector3.zero;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Player left the wall.");
            // 벽에서 떨어졌을 때 특별히 플래그를 재설정할 필요는 없습니다.
            // isStoppedByCollision 플래그는 제거되었으므로.
        }
    }
}