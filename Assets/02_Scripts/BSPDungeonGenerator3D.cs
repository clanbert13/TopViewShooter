using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

namespace DungeonGeneration
{
    // 3D BSP 던전 생성기 (바닥, 벽, 마커 y 기준: -1)
    public class BSPDungeonGenerator3D : MonoBehaviour
    {
        [Header("Map & BSP Settings")]
        public int width = 80;
        public int depth = 60;
        public int minLeafSize = 6;
        public int maxLeafSize = 20;

        [Header("Prefabs & Dimensions")]
        public GameObject floorPrefab;
        public GameObject wallPrefab;
        public float wallHeight = 3f;
        public GameObject startMarkerPrefab;
        public GameObject bossMarkerPrefab;

        [Header("NavMesh")]
        public NavMeshSurface navMeshSurface;

        // ─── 내부 데이터 ───────────────────────────────────────────
        private int[,] map;
        private List<Leaf> leaves;
        private List<Line> corridors;
        private List<Vector2Int> roomCenters;
        private List<RectInt> rooms;
        private Leaf bossLeaf;

        // 외부 접근용
        public Vector2Int StartRoomCenter { get; private set; }
        public bool IsDungeonGenerated { get; private set; } = false;

        void Start()
        {
            GenerateDungeonData();
            BuildDungeonObjects();

            if (navMeshSurface != null)
                navMeshSurface.BuildNavMesh();
            else
                Debug.LogWarning("NavMeshSurface not assigned.");

            IsDungeonGenerated = true;
        }

        // 1) BSP 분할 → 방·복도 논리 데이터 생성
        void GenerateDungeonData()
        {
            map = new int[width, depth];
            leaves = new List<Leaf>();
            corridors = new List<Line>();
            roomCenters = new List<Vector2Int>();
            rooms = new List<RectInt>();
            bossLeaf = null;

            var root = new Leaf(0, 0, width, depth);
            leaves.Add(root);
            SplitLeaf(root);
            CreateRoomsAndCorridors(root);

            // 모든 방 순차 연결 (고립 방 방지)
            for (int i = 1; i < roomCenters.Count; i++)
                CreateCorridor(roomCenters[i - 1], roomCenters[i]);

            PickStartAndBossRoom();
            if (bossLeaf != null)
                EnlargeBossRoom(bossLeaf);

            foreach (var r in rooms) FillRect(r, 1);
            foreach (var c in corridors) FillLineTwoWide(c, 1);
        }

        // 2) 3D 오브젝트 배치 (y 기준 -1)
        void BuildDungeonObjects()
        {
            // 2-1. 바닥 생성 (map==1 → y = -1)
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (map[x, z] == 1)
                        Instantiate(floorPrefab,
                                    new Vector3(x, -1f, z),
                                    Quaternion.identity,
                                    transform);
                }
            }

            // 2-2. 벽 생성 (map==0 && 인접 바닥 있을 때 → y center = wallHeight/2 -1)
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (map[x, z] == 0 && HasAdjacentFloor(x, z))
                    {
                        float wy = (wallHeight / 2f) - 1f;
                        var wall = Instantiate(wallPrefab,
                                               new Vector3(x, wy, z),
                                               Quaternion.identity,
                                               transform);
                        wall.transform.localScale = new Vector3(1f, wallHeight, 1f);
                    }
                }
            }

            // 2-3. 시작 마커 (y = -1 + offset)
            if (startMarkerPrefab != null)
            {
                var sr = rooms[0];
                var sc = new Vector2Int(sr.x + sr.width / 2, sr.y + sr.height / 2);
                Instantiate(startMarkerPrefab,
                            new Vector3(sc.x, -1f + 1f, sc.y),
                            Quaternion.identity,
                            transform);
            }

            // 2-4. 보스 마커
            if (bossMarkerPrefab != null && bossLeaf != null)
            {
                var br = bossLeaf.room.Value;
                var bc = new Vector2Int(br.x + br.width / 2, br.y + br.height / 2);
                Instantiate(bossMarkerPrefab,
                            new Vector3(bc.x, -1f + 1f, bc.y),
                            Quaternion.identity,
                            transform);
            }
        }

        // ─── 이하 BSP/Room/Corridor 로직 (변경 없음) ─────────────────────────
        void SplitLeaf(Leaf leaf)
        {
            if (leaf.w < maxLeafSize && leaf.h < maxLeafSize) return;
            bool horiz = (leaf.w < leaf.h);
            if (leaf.w == leaf.h) horiz = Random.value > 0.5f;
            float ratio = Random.Range(0.3f, 0.7f);

            if (horiz && leaf.h >= minLeafSize * 2)
            {
                int sy = Mathf.FloorToInt(leaf.h * ratio);
                leaf.left = new Leaf(leaf.x, leaf.y, leaf.w, sy);
                leaf.right = new Leaf(leaf.x, leaf.y + sy, leaf.w, leaf.h - sy);
            }
            else if (!horiz && leaf.w >= minLeafSize * 2)
            {
                int sx = Mathf.FloorToInt(leaf.w * ratio);
                leaf.left = new Leaf(leaf.x, leaf.y, sx, leaf.h);
                leaf.right = new Leaf(leaf.x + sx, leaf.y, leaf.w - sx, leaf.h);
            }
            else return;

            leaves.Add(leaf.left);
            leaves.Add(leaf.right);
            SplitLeaf(leaf.left);
            SplitLeaf(leaf.right);
        }

        void CreateRoomsAndCorridors(Leaf leaf)
        {
            if (leaf.left != null || leaf.right != null)
            {
                if (leaf.left != null) CreateRoomsAndCorridors(leaf.left);
                if (leaf.right != null) CreateRoomsAndCorridors(leaf.right);

                if (leaf.left.center.HasValue && leaf.right.center.HasValue)
                    CreateCorridor(leaf.left.center.Value, leaf.right.center.Value);
            }
            else if (leaf.w > minLeafSize && leaf.h > minLeafSize)
            {
                int rw = Random.Range(minLeafSize / 2, leaf.w - 2);
                int rh = Random.Range(minLeafSize / 2, leaf.h - 2);
                int rx = Random.Range(leaf.x + 1, leaf.x + leaf.w - rw - 1);
                int rz = Random.Range(leaf.y + 1, leaf.y + leaf.h - rh - 1);

                leaf.room = new RectInt(rx, rz, rw, rh);
                leaf.center = new Vector2Int(rx + rw / 2, rz + rh / 2);
                rooms.Add(leaf.room.Value);
                roomCenters.Add(leaf.center.Value);
            }
        }

        void CreateCorridor(Vector2Int a, Vector2Int b)
        {
            if (Random.value > 0.5f)
            {
                corridors.Add(new Line(a.x, a.y, b.x, a.y));
                corridors.Add(new Line(b.x, a.y, b.x, b.y));
            }
            else
            {
                corridors.Add(new Line(a.x, a.y, a.x, b.y));
                corridors.Add(new Line(a.x, b.y, b.x, b.y));
            }
        }

        void PickStartAndBossRoom()
        {
            float md = float.MaxValue; int si = 0;
            for (int i = 0; i < roomCenters.Count; i++)
            {
                float d = Vector2Int.Distance(roomCenters[i], Vector2Int.zero);
                if (d < md) { md = d; si = i; }
            }
            StartRoomCenter = roomCenters[si];
            if (si != 0)
            {
                var tmpR = roomCenters[si];
                roomCenters.RemoveAt(si); roomCenters.Insert(0, tmpR);
                var tmpRoom = rooms[si];
                rooms.RemoveAt(si); rooms.Insert(0, tmpRoom);
            }

            float maxD = -1f; int bi = 0;
            for (int i = 1; i < roomCenters.Count; i++)
            {
                float d = Vector2Int.Distance(StartRoomCenter, roomCenters[i]);
                if (d > maxD) { maxD = d; bi = i; }
            }
            var bossRect = rooms[bi];
            bossLeaf = leaves.Find(l => l.room.HasValue && l.room.Value == bossRect);
        }

        void EnlargeBossRoom(Leaf leaf)
        {
            if (!leaf.room.HasValue) return;
            int nx = leaf.x + 1, nz = leaf.y + 1, nw = leaf.w - 2, nh = leaf.h - 2;
            leaf.room = new RectInt(nx, nz, nw, nh);
            leaf.center = new Vector2Int(nx + nw / 2, nz + nh / 2);
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].x == nx && rooms[i].y == nz)
                {
                    rooms[i] = leaf.room.Value;
                    roomCenters[i] = leaf.center.Value;
                    break;
                }
            }
        }

        void FillRect(RectInt r, int v)
        {
            for (int x = r.x; x < r.x + r.width; x++)
                for (int z = r.y; z < r.y + r.height; z++)
                    map[x, z] = v;
        }

        void FillLineTwoWide(Line l, int v)
        {
            if (l.isHorizontal)
            {
                int z = l.y1, z2 = (z + 1 < depth ? z + 1 : z - 1);
                for (int x = Mathf.Min(l.x1, l.x2); x <= Mathf.Max(l.x1, l.x2); x++)
                {
                    map[x, z] = v; map[x, z2] = v;
                }
            }
            else
            {
                int x = l.x1, x2 = (x + 1 < width ? x + 1 : x - 1);
                for (int z = Mathf.Min(l.y1, l.y2); z <= Mathf.Max(l.y1, l.y2); z++)
                {
                    map[x, z] = v; map[x2, z] = v;
                }
            }
        }

        bool HasAdjacentFloor(int x, int z)
        {
            var dirs = new[]
            {
                new Vector2Int(1,0), new Vector2Int(-1,0),
                new Vector2Int(0,1), new Vector2Int(0,-1)
            };
            foreach (var d in dirs)
                if (x + d.x >= 0 && x + d.x < width && z + d.y >= 0 && z + d.y < depth
                    && map[x + d.x, z + d.y] == 1)
                    return true;
            return false;
        }
    }

}
