using UnityEngine;

namespace DungeonGeneration
{
    // 복도 연결 정보 (수평 혹은 수직)
    public struct Line
    {
        public int x1, y1, x2, y2;
        public bool isHorizontal => (y1 == y2);

        public Line(int x1, int y1, int x2, int y2)
        {
            this.x1 = x1; this.y1 = y1;
            this.x2 = x2; this.y2 = y2;
        }
    }
}
