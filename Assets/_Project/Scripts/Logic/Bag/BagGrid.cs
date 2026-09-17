using System;
using System.Collections.Generic;

namespace JM2D.Logic.Bag
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

                    AddOther(neighbors, item, x - 1, y);
                    AddOther(neighbors, item, x + 1, y);
                    AddOther(neighbors, item, x, y - 1);
                    AddOther(neighbors, item, x, y + 1);
                }
            }

            return neighbors.Count;
        }

        /// 그 칸에 다른 아이템이 있으면 명단에 담는다.
        /// 격자 밖이거나, 비었거나, 자기 자신이면 담지 않는다.
        private void AddOther(HashSet<IGridItem> set, IGridItem self, int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;

            IGridItem other = _cells[x, y];

            if (other == null || other == self) return;

            set.Add(other);
        }

        /// 이 아이템이 걸친 행들에 있는 다른 아이템의 수.
        /// 같은 아이템이 두 행에 걸쳐 있어도 하나로 센다.
        public int CountInSameRow(IGridItem item)
        {
            var found = new HashSet<IGridItem>();

            for (int y = 0; y < Height; y++)
            {
                if (!OccupiesRow(item, y)) continue;

                for (int x = 0; x < Width; x++)
                    AddOther(found, item, x, y);
            }

            return found.Count;
        }

        /// 그 행에 이 아이템이 걸쳐 있는가.
        private bool OccupiesRow(IGridItem item, int y)
        {
            for (int x = 0; x < Width; x++)
                if (GetAt(x, y) == item) return true;

            return false;
        }

        /// 이 아이템이 걸친 열들에 있는 다른 아이템의 수.
        /// 같은 행과 대칭이다.
        public int CountInSameColumn(IGridItem item)
        {
            var found = new HashSet<IGridItem>();

            for (int x = 0; x < Width; x++)
            {
                if (!OccupiesColumn(item, x)) continue;

                for (int y = 0; y < Height; y++)
                    AddOther(found, item, x, y);
            }

            return found.Count;
        }

        /// 그 열에 이 아이템이 걸쳐 있는가.
        private bool OccupiesColumn(IGridItem item, int x)
        {
            for (int y = 0; y < Height; y++)
                if (GetAt(x, y) == item) return true;

            return false;
        }

        /// 그리드의 네 귀퉁이 칸 중 이 아이템이 차지한 것의 수.
        public int CountCorners(IGridItem item)
        {
            int lastX = Width - 1;
            int lastY = Height - 1;

            int count = 0;

            if (GetAt(0, 0) == item) count++;           // 왼쪽 위
            if (GetAt(lastX, 0) == item) count++;       // 오른쪽 위
            if (GetAt(0, lastY) == item) count++;      // 왼쪽 아래
            if (GetAt(lastX, lastY) == item) count++;  // 오른쪽 아래

            return count;
        }

        /// 조건에 맞는 세는 메서드를 골라 부른다.
        /// 짝이 뒤바뀌어도 오류가 나지 않으므로 테스트가 지킨다.
        public int Count(IGridItem item, SynergyCondition condition)
        {
            switch (condition)
            {
                case SynergyCondition.Adjacent: return CountAdjacent(item);
                case SynergyCondition.SameRow: return CountInSameRow(item);
                case SynergyCondition.SameColumn: return CountInSameColumn(item);
                case SynergyCondition.Corner: return CountCorners(item);
                default: throw new ArgumentOutOfRangeException(nameof(condition), condition, null);
            }
        }

        /// 놓인 아이템을 왼쪽 위 좌표와 함께 돌려준다.
        /// 여러 칸을 덮는 아이템도 한 번만 나온다.
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
