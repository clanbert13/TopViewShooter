using UnityEngine;

namespace DungeonGeneration
{
    // BSP 분할용 노드
    public class Leaf
    {
        public int x, y, w, h;
        public Leaf left, right;
        public RectInt? room;            // 생성된 방 영역
        public Vector2Int? center;       // 방 중심 좌표

        public Leaf(int x, int y, int w, int h)
        {
            this.x = x; this.y = y;
            this.w = w; this.h = h;
            left = right = null;
            room = null;
            center = null;
        }
    }
}
