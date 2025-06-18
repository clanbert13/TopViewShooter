using UnityEngine;
using System.Collections;

namespace DungeonGeneration
{
    // ���� ���� �Ϸ� �� �÷��̾� ���� ������ ����/�̵�
    public class PlayerSpawner3D : MonoBehaviour
    {
        [Header("References")]
        public BSPDungeonGenerator3D dungeonGenerator;
        public GameObject playerPrefab;

        private GameObject spawnedPlayer;

        void Start()
        {
            StartCoroutine(WaitAndSpawn());
        }

        IEnumerator WaitAndSpawn()
        {
            if (dungeonGenerator == null)
            {
                Debug.LogError("PlayerSpawner3D: DungeonGenerator not assigned.");
                yield break;
            }

            // ���� ���� �Ϸ�� ������ ���
            while (!dungeonGenerator.IsDungeonGenerated)
                yield return null;

            SpawnAtStart();
        }

        void SpawnAtStart()
        {
            var gp = dungeonGenerator.StartRoomCenter;
            Vector3 pos = new Vector3(gp.x + 0.5f, 1f, gp.y + 0.5f);

            if (spawnedPlayer == null)
                spawnedPlayer = Instantiate(playerPrefab, pos, Quaternion.identity);
            else
                spawnedPlayer.transform.position = pos;
        }
    }
}
