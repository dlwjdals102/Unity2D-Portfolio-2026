namespace JM2D.Logic.Rooms
{
    /// 방 안의 칸 격자. 어느 칸을 지나갈 수 있는지만 안다.
    /// 무엇이 막고 있는지(벽인지 장애물인지)는 모른다.
    public class RoomGrid
    {
        private readonly bool[,] _isBlocked;

        public int Width { get; }
        public int Height { get; }

        public RoomGrid(int width, int height)
        {
            Width = width;
            Height = height;

            // bool 배열은 false 로 채워진다. 처음에는 막힌 칸이 없다.
            _isBlocked = new bool[width, height];
        }

        /// 같은 모양의 격자를 하나 더 만든다. 장애물을 시험 삼아 놓아 보고 버릴 때 쓴다.
        public RoomGrid(RoomGrid other) : this(other.Width, other.Height)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (!other.IsPassable(x, y)) Block(x, y);
        }

        /// 격자 안의 칸인가.
        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        /// 지나갈 수 있는가. 격자 밖은 지나갈 수 없다.
        public bool IsPassable(int x, int y)
        {
            return InBounds(x, y) && !_isBlocked[x, y];
        }

        /// 이 칸을 막는다. 이미 막혀 있어도 그대로 둔다.
        public void Block(int x, int y)
        {
            if (!InBounds(x, y)) return;

            _isBlocked[x, y] = true;
        }
    }
}
