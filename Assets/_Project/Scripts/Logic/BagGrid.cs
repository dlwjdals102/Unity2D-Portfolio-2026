namespace JM2D.Logic
{
    /// 아이템을 칸에 놓고 겹침과 경계를 판단한다.
    /// 무엇이 놓이는지는 모른다. 크기만 안다.
    public class BagGrid
    {
        private readonly IGridItem[,] _cells;

        public int Width { get; }
        public int Height { get; }

        public BagGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new IGridItem[width, height];
        }

        /// 그 칸에 있는 아이템. 비었으면 null 이다.
        public IGridItem GetAt(int x, int y) => _cells[x, y];

        public bool CanPlace(IGridItem item, int x, int y)
        {
            if (x < 0 || y < 0 || x + item.Width > Width || y + item.Height > Height) return false;

            for (int dy = 0; dy < item.Height; dy++)
                for (int dx = 0; dx < item.Width; dx++)
                    if (GetAt(x + dx, y + dy) != null) return false;

            return true;
        }

        public bool TryPlace(IGridItem item, int x, int y)
        {
            if (!CanPlace(item, x, y)) return false;

            for (int dy = 0; dy < item.Height; dy++)
                for (int dx = 0; dx < item.Width; dx++)
                    _cells[x + dx, y + dy] = item;

            return true;
        }

        public void Remove(IGridItem item)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (GetAt(x, y) == item) _cells[x, y] = null;
        }
    }
}
