using UnityEngine;

namespace DungeonGeneration
{
    // BSP ���ҿ� ���
    public class Leaf
    {
        public int x, y, w, h;
        public Leaf left, right;
        public RectInt? room;            // ������ �� ����
        public Vector2Int? center;       // �� �߽� ��ǥ

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
