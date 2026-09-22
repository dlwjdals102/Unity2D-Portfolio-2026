using System.Collections.Generic;

namespace JM2D.Logic.Rooms
{
    /// 한 칸에서 모든 칸까지 몇 걸음인지 적어 둔 표.
    /// 적은 이웃 중 숫자가 작은 쪽으로 걸어 시작 칸(플레이어)에게 온다.
    /// 적이 몇 마리든 이 표 하나를 같이 읽어, 마릿수가 늘어도 길 찾는 비용이 늘지 않는다.
    public class FlowField
    {
        /// 갈 수 없는 칸. 격자 밖도 이 값이다.
        public const int Unreachable = -1;

        /// 이웃 여덟. 앞의 넷이 상하좌우, 뒤의 넷이 대각선이다.
        /// 상하좌우만 보면 비스듬한 길도 계단처럼 꺾여, 그대로 걷는 적이 어색하게 움직인다.
        public static readonly int[] OffsetX = { 1, -1, 0, 0, 1, 1, -1, -1 };
        public static readonly int[] OffsetY = { 0, 0, 1, -1, 1, -1, 1, -1 };

        private readonly int[,] _distance;

        /// 다음에 살펴볼 칸. 다시 계산할 때마다 새로 만들지 않고 비워서 쓴다.
        private readonly Queue<(int x, int y)> _queue = new Queue<(int x, int y)>();

        public int Width { get; }
        public int Height { get; }

        public FlowField(int width, int height)
        {
            Width = width;
            Height = height;
            _distance = new int[width, height];
        }

        /// 그 칸까지 몇 걸음인가. 갈 수 없거나 격자 밖이면 Unreachable.
        public int GetDistance(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return Unreachable;

            return _distance[x, y];
        }

        /// 그 칸에서 이웃으로 한 발 옮길 수 있는가.
        /// 대각선은 양옆 두 칸이 모두 비어 있어야 한다. 모서리를 스치며 지나가면 몸이 낀다.
        ///
        ///   . ##      이 모양에서 왼쪽 아래에서 오른쪽 위로 가는 대각선은 막는다.
        ///   ## .
        public static bool CanStep(RoomGrid grid, int x, int y, int stepX, int stepY)
        {
            if (!grid.IsPassable(x + stepX, y + stepY)) return false;

            if (stepX == 0 || stepY == 0) return true;

            return grid.IsPassable(x + stepX, y) && grid.IsPassable(x, y + stepY);
        }

        /// 시작 칸에서 퍼져 나가며 거리를 다시 적는다.
        public void Rebuild(RoomGrid grid, int startX, int startY)
        {
            // 지난 계산이 남지 않게 먼저 전부 지운다.
            // 이 줄이 '아직 안 본 칸' 이라는 표시도 겸한다.
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    _distance[x, y] = Unreachable;

            _queue.Clear();

            if (!grid.IsPassable(startX, startY)) return;

            _distance[startX, startY] = 0;
            _queue.Enqueue((startX, startY));

            while (_queue.Count > 0)
            {
                (int x, int y) = _queue.Dequeue();

                for (int i = 0; i < OffsetX.Length; i++)
                {
                    int nextX = x + OffsetX[i];
                    int nextY = y + OffsetY[i];

                    if (!CanStep(grid, x, y, OffsetX[i], OffsetY[i])) continue;

                    // 이미 적어 둔 칸은 더 짧은 길로 이미 닿은 것이다.
                    if (_distance[nextX, nextY] != Unreachable) continue;

                    _distance[nextX, nextY] = _distance[x, y] + 1;
                    _queue.Enqueue((nextX, nextY));
                }
            }
        }
    }
}
