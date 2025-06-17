using UnityEngine;

public class EnemyCreator : MonoBehaviour
{
    public GameObject enemyPrefab; // 생성할 적 프리팹
    private Transform playerTransform; // 플레이어의 Transform (적 생성 기준 위치)

    [Header("Spawn Settings")]
    public float maxSpawnRadius = 20f; // 플레이어 주변 최대 몇 미터 반경 내에서 생성할지
    public float minSpawnRadius = 9f;  // 플레이어 주변 최소 몇 미터 반경 밖에서 생성할지 (추가됨)
    public float spawnInterval = 3f; // 적을 생성할 주기 (초)

    private float nextSpawnTime;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogError("Player not found! Please ensure your player has the 'Player' tag.", this);
                enabled = false; // 플레이어를 찾지 못하면 이 스크립트를 비활성화
                return;
            }
        }
        
        if (minSpawnRadius >= maxSpawnRadius)
        {
            Debug.LogError("Min Spawn Radius must be less than Max Spawn Radius!", this);
            enabled = false;
            return;
        }

        nextSpawnTime = Time.time + spawnInterval; // 첫 생성 시간 설정
    }

    void Update()
    {
        // playerTransform이 유효한지 매 프레임 확인 (플레이어가 파괴될 수도 있기 때문)
        if (playerTransform == null)
        {
            // 플레이어를 잃어버렸을 경우 다시 찾거나, 스크립트를 비활성화합니다.
            playerTransform = GameObject.FindWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                // 플레이어를 계속 찾지 못하면 생성 중지
                Debug.LogWarning("Player Transform is null, stopping enemy spawning.");
                return;
            }
        }

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        // 플레이어가 없으면 적을 생성하지 않습니다.
        if (playerTransform == null || enemyPrefab == null)
        {
            Debug.LogWarning("Cannot spawn enemy: Player Transform or Enemy Prefab is null.", this);
            return;
        }

        Vector3 spawnPosition;
        float currentDistance;
        int maxAttempts = 10;
        int attempts = 0;

        do
        {
            // 원점을 기준으로 최대 반경 내 무작위 지점 선택
            Vector3 randomDirection = Random.insideUnitSphere; // 단위 구 내부 (길이 0~1)

            randomDirection.y = 0;
            randomDirection.Normalize(); // 방향 벡터 정규화

            // 최소 반경과 최대 반경 사이의 무작위 거리 (minSpawnRadius ~ maxSpawnRadius)
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);

            spawnPosition = playerTransform.position + randomDirection * randomDistance;

            // Y축을 프리팹의 Y 위치로 고정 (땅에 생성되도록)
            spawnPosition.y = enemyPrefab.transform.position.y;

            // 현재 위치에서 플레이어까지의 실제 거리를 계산
            currentDistance = Vector3.Distance(spawnPosition, playerTransform.position);

            attempts++;
            // 무한 루프 방지를 위해 최대 시도 횟수를 제한
        } while (currentDistance < minSpawnRadius && attempts < maxAttempts); 

        // 만약 유효한 위치를 maxAttempts 내에 찾지 못했더라도 그냥 생성 (최악의 경우)
        if (attempts >= maxAttempts)
        {
            Debug.LogWarning($"Could not find a spawn position outside minSpawnRadius after {maxAttempts} attempts. Spawning anyway.", this);
        }

        GameObject newEnemyGO = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        Debug.Log($"Enemy spawned at: {spawnPosition} (Distance: {Vector3.Distance(spawnPosition, playerTransform.position):F2})");
    }
}