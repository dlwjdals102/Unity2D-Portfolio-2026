using System.Collections.Generic;

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

        /// 놓인 아이템을 왼쪽 위 좌표와 함께 돌려준다.
        /// 여러 칸을 덮는 아이템도 한 번만 나온다.
        /// 이 아이템과 상하좌우로 맞닿은 다른 아이템의 수.
        /// 두 칸에서 맞닿아도 하나로 센다. 대각선은 세지 않는다.
        public int CountAdjacent(IGridItem item)
        {
            var neighbors = new HashSet<IGridItem>();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (_cells[x, y] != item) continue;

                    AddNeighbor(neighbors, item, x - 1, y);
                    AddNeighbor(neighbors, item, x + 1, y);
                    AddNeighbor(neighbors, item, x, y - 1);
                    AddNeighbor(neighbors, item, x, y + 1);
                }
            }

            return neighbors.Count;
        }

        /// 그 칸에 다른 아이템이 있으면 명단에 담는다.
        /// 격자 밖이거나, 비었거나, 자기 자신이면 담지 않는다.
        private void AddNeighbor(HashSet<IGridItem> set, IGridItem self, int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;

            IGridItem neighbor = _cells[x, y];

            if (neighbor == null || neighbor == self) return;

            set.Add(neighbor);
        }

        public List<PlacedItem> GetPlacedItems()
        {
            var result = new List<PlacedItem>();
            var seen = new HashSet<IGridItem>();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    IGridItem item = _cells[x, y];

                    if (item == null || !seen.Add(item)) continue;

                    result.Add(new PlacedItem(item, x, y));
                }
            }

            return result;
        }
    }
}
