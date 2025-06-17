using UnityEngine;

public class EnemyCreator : MonoBehaviour
{
    public GameObject enemyPrefab; // 생성할 적 프리팹
    private Transform playerTransform; // 플레이어의 Transform (적 생성 기준 위치)

    [Header("Spawn Settings")]
    public float maxSpawnRadius = 20f; // 플레이어 주변 최대 몇 미터 반경 내에서 생성할지
    public float minSpawnRadius = 9f;  // 플레이어 주변 최소 몇 미터 반경 밖에서 생성할지
    public float spawnInterval = 3f; // 적을 생성할 주기 (초)

    [Header("Map Boundaries")]
    // 방법 A: 직접 경계 값 설정 (고정된 맵)
    public Vector2 mapMinBounds = new Vector2(-50f, -50f); // 맵의 XZ 최소 좌표
    public Vector2 mapMaxBounds = new Vector2(50f, 50f);   // 맵의 XZ 최대 좌표

    // 방법 B: 맵 콜라이더를 참조하여 경계 가져오기 (맵이 Mesh Collider 등일 경우)
    [SerializeField] private Collider mapBoundaryCollider; 

    private float nextSpawnTime;

    void Start()
    {
        // 플레이어 Transform 초기화
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogError("Player not found! Please ensure your player has the 'Player' tag.", this);
                enabled = false;
                return;
            }
        }
        
        // 스폰 반경 유효성 검사
        if (minSpawnRadius >= maxSpawnRadius)
        {
            Debug.LogError("Min Spawn Radius must be less than Max Spawn Radius!", this);
            enabled = false;
            return;
        }


        if (mapBoundaryCollider == null)
        {
            Debug.LogError("Map Boundary Collider not assigned! Please assign it in the Inspector.", this);
            enabled = false;
            return;
        }
        else
        {
            // mapMinBounds, mapMaxBounds를 mapBoundaryCollider.bounds.min / .max 로 설정
            mapMinBounds = new Vector2(mapBoundaryCollider.bounds.min.x, mapBoundaryCollider.bounds.min.z);
            mapMaxBounds = new Vector2(mapBoundaryCollider.bounds.max.x, mapBoundaryCollider.bounds.max.z);
        }

        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        // playerTransform이 유효한지 매 프레임 확인
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindWithTag("Player")?.transform;
            if (playerTransform == null)
            {
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
        if (playerTransform == null || enemyPrefab == null)
        {
            Debug.LogWarning("Cannot spawn enemy: Player Transform or Enemy Prefab is null.", this);
            return;
        }

        Vector3 spawnPosition;
        float currentDistance;
        int maxAttempts = 50; // 유효한 위치를 찾기 위한 최대 시도 횟수를 늘립니다. (맵 경계까지 고려하므로)
        int attempts = 0;
        bool positionFound = false;

        do
        {
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0;
            randomDirection.Normalize();

            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
            spawnPosition = playerTransform.position + randomDirection * randomDistance;

            // Y축을 프리팹의 Y 위치로 고정
            spawnPosition.y = enemyPrefab.transform.position.y;

            currentDistance = Vector3.Distance(spawnPosition, playerTransform.position);

            // 중요: 맵 경계 내에 있는지 확인
            bool withinMapBounds = IsPositionWithinMapBounds(spawnPosition);

            if (currentDistance >= minSpawnRadius && currentDistance <= maxSpawnRadius && withinMapBounds)
            {
                positionFound = true; // 유효한 위치를 찾음
            }

            attempts++;
        } while (!positionFound && attempts < maxAttempts); // 유효한 위치를 찾거나 최대 시도 횟수에 도달할 때까지 반복

        if (!positionFound) // 최대 시도 횟수 내에 적합한 위치를 찾지 못함
        {
            Debug.LogWarning($"Could not find a valid spawn position after {maxAttempts} attempts. Spawning might be impossible or too difficult in current area.", this);
            // 이 경우, 적을 생성하지 않거나 (안전), 그냥 마지막으로 계산된 위치에 생성할 수 있습니다.
            // 여기서는 안전하게 생성하지 않겠습니다.
            return; 
        }

        GameObject newEnemyGO = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Enemy spawned at: {spawnPosition} (Distance: {Vector3.Distance(spawnPosition, playerTransform.position):F2})");
    }

    // 맵 경계 내에 위치하는지 확인하는 헬퍼 함수
    private bool IsPositionWithinMapBounds(Vector3 pos)
    {
        return pos.x >= mapMinBounds.x && pos.x <= mapMaxBounds.x &&
               pos.z >= mapMinBounds.y && pos.z <= mapMaxBounds.y; // mapMinBounds.y는 Z 좌표에 해당
    }

    // (선택 사항) NavMesh를 사용하는 경우: 생성 위치가 NavMesh 위에 있는지 확인
    // using UnityEngine.AI;
    // private bool IsPositionOnNavMesh(Vector3 pos)
    // {
    //     NavMeshHit hit;
    //     // NavMesh.SamplePosition(position, out hit, maxDistance, areaMask)
    //     // maxDistance: pos로부터 얼마나 멀리까지 NavMesh를 찾을지
    //     // areaMask: 특정 NavMesh Area만 찾을지 (All Areas를 의미하는 -1)
    //     return NavMesh.SamplePosition(pos, out hit, 1.0f, NavMesh.AllAreas);
    // }
}