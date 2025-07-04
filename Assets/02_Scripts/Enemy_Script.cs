using UnityEngine;

public class Enemy_Script : Character_Script
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;     // 총알 프리팹
    [SerializeField] private float bulletEndTime = 3f;  // 총알이 사라지는 시간
    private GameObject[] bulletPool;       // 총알 풀
    public float damage = 1f;           // 총알 피해량
    public int poolSize = 5;             // 총알 풀의 크기
    public int bulletType = 0;           // 총알 타입 (이동 형태)


    void Start()
    {
        // 캐릭터의 체력 초기화
        health = maxHealth;

        bulletType = Random.Range(0, 2);

        // 총알 풀 초기화
        bulletPoolInit();
    }

    void Update()
    {

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
    public override void Attack(Vector3 targetPosition, string targetTag = "Player")
    {
        // 총알 발사
        GameObject bullet = GetBulletFromPool();
        if (bullet != null)
        {
            // 이 오브젝트의 위치에서 플레이어의 위치를 향하는 벡터
            Vector3 targetDirection = (targetPosition - this.transform.position).normalized;

            // 총알 설정
            bullet.GetComponent<Bullet_Script>().SetBullet(10f, 5f, 5f, damage, targetTag, bulletEndTime,
                                        targetDirection, this.transform.position, bulletType);
            bullet.SetActive(true); // 총알 활성화

            //Debug.Log("firing bullet)");
        }
    }
    public override void Die()
    {
        if (UI_Manager.Instance != null)
        {
            UI_Manager.Instance.UpdatePlayerScoreUI(100);
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerScore(100);
        }
        // 죽음 처리 로직
        Debug.Log(this.name.ToString() + "이(가) 죽었습니다.");
        // 예를 들어, 오브젝트를 비활성화하거나 파괴할 수 있습니다.
        this.gameObject.SetActive(false);
    }
}
