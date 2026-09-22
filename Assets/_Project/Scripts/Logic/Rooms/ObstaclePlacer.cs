using System;
using System.Collections.Generic;
using JM2D.Logic.Common;

namespace JM2D.Logic.Rooms
{
    /// 방 안에 장애물을 놓을 칸을 정한다. 실제로 칠하는 일은 하지 않는다.
    /// 놓은 뒤에도 문과 출현 지점에 닿는지 BFS 로 확인하고, 막히면 버리고 다시 놓는다.
    ///
    /// 방 안쪽을 구역 여섯으로 나누고 구역마다 덩어리를 하나씩 놓는다.
    /// 자리를 순전히 난수에 맡기면 한쪽 구석에 몰려 다른 쪽이 텅 빈다.
    public static class ObstaclePlacer
    {
        /// 덩어리 모양. 가로 세로 칸 수다.
        private static readonly (int Width, int Height)[] Shapes =
            { (1, 1), (2, 2), (1, 3), (3, 1) };

        /// 방 안쪽을 이만큼으로 나눈다. 구역 하나에 덩어리 하나다.
        private const int RegionColumns = 3;
        private const int RegionRows = 2;

        private const int MinClusters = 4;
        private const int MaxClusters = 6;
        private const int MaxAttempts = 10;

        /// 한 덩어리를 놓을 자리를 찾는 시도 횟수. 못 찾으면 그 덩어리는 건너뛴다.
        private const int MaxSpotTries = 30;

        /// 벽에서 이만큼 떨어뜨린다. 벽을 따라 도는 길을 늘 남긴다.
        private const int EdgeMargin = 1;

        /// 덩어리끼리 이만큼은 떨어뜨린다. 구역이 달라도 경계에 붙으면 한 덩어리처럼 보인다.
        private const int ClusterSpacing = 4;

        /// 구역 테두리를 이만큼 비운다. 구역 경계에 붙는 자리를 아예 고르지 않게 한다.
        /// 간격을 올리는 것보다 싸다. 뽑아 놓고 버리는 대신 애초에 붙을 수 없는 자리에서만 고른다.
        private const int RegionInset = 1;

        /// 막을 칸 목록을 돌려준다. 열 번을 시도해도 길이 막히면 빈 목록이다.
        public static List<RoomCell> Place(RoomGrid walls, IReadOnlyList<RoomCell> keepClear,
            IReadOnlyList<RoomCell> mustReach, Random random)
        {
            var blocked = new HashSet<RoomCell>(keepClear);
            var placed = new List<RoomCell>();

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                // 시도마다 새 사본이다. 원본에 놓으면 실패한 시도의 장애물이 남는다.
                var grid = new RoomGrid(walls);
                placed.Clear();

                int regionCount = RegionColumns * RegionRows;
                int clusters = Math.Min(random.Next(MinClusters, MaxClusters + 1), regionCount);

                // 구역 고르기는 보상 후보를 뽑는 것과 같은 계산이다. 서로 다른 구역이 나온다.
                foreach (int region in RandomPick.Distinct(regionCount, clusters, random))
                    PlaceOneCluster(grid, blocked, placed, random, region);

                // 다 놓은 뒤에 검사한다. 놓는 중간에는 당연히 길이 살아 있다.
                if (IsAllReachable(grid, mustReach)) return placed;
            }

            return new List<RoomCell>();
        }

        /// 덩어리 하나를 맡은 구역 안에 놓는다. 못 찾으면 아무것도 하지 않는다.
        private static void PlaceOneCluster(RoomGrid grid, HashSet<RoomCell> keepClear,
            List<RoomCell> placed, Random random, int region)
        {
            GetRegion(grid, region, out int minX, out int minY, out int maxX, out int maxY);

            for (int tries = 0; tries < MaxSpotTries; tries++)
            {
                (int width, int height) = Shapes[random.Next(Shapes.Length)];

                // 구역보다 큰 모양은 건너뛴다.
                if (maxX - minX + 1 < width || maxY - minY + 1 < height) continue;

                int x = random.Next(minX, maxX - width + 2);
                int y = random.Next(minY, maxY - height + 2);

                if (!CanPlace(grid, keepClear, x, y, width, height)) continue;
                if (!IsFarFromPlaced(placed, x, y, width, height)) continue;

                for (int dy = 0; dy < height; dy++)
                    for (int dx = 0; dx < width; dx++)
                    {
                        grid.Block(x + dx, y + dy);
                        placed.Add(new RoomCell(x + dx, y + dy));
                    }

                return;
            }
        }

        /// 이미 놓은 덩어리들에서 충분히 떨어져 있는가.
        /// 가로 세로 중 먼 쪽으로 잰다(체스판 거리). 대각선으로 붙은 것도 붙은 것으로 본다.
        private static bool IsFarFromPlaced(List<RoomCell> placed, int x, int y, int width, int height)
        {
            foreach (RoomCell cell in placed)
                for (int dy = 0; dy < height; dy++)
                    for (int dx = 0; dx < width; dx++)
                    {
                        int gapX = Math.Abs(cell.X - (x + dx));
                        int gapY = Math.Abs(cell.Y - (y + dy));

                        if (Math.Max(gapX, gapY) < ClusterSpacing) return false;
                    }

            return true;
        }

        /// 구역 하나가 덮는 칸 범위. 방 안쪽(벽에서 EdgeMargin 만큼 뗀 곳)을 고르게 나눈다.
        /// 나누어떨어지지 않는 나머지는 마지막 구역이 가져간다.
        private static void GetRegion(RoomGrid grid, int index,
            out int minX, out int minY, out int maxX, out int maxY)
        {
            int firstX = EdgeMargin + 1;
            int lastX = grid.Width - EdgeMargin - 2;
            int firstY = EdgeMargin + 1;
            int lastY = grid.Height - EdgeMargin - 2;

            int columnWidth = (lastX - firstX + 1) / RegionColumns;
            int rowHeight = (lastY - firstY + 1) / RegionRows;

            int column = index % RegionColumns;
            int row = index / RegionColumns;

            minX = firstX + column * columnWidth;
            minY = firstY + row * rowHeight;
            maxX = column == RegionColumns - 1 ? lastX : minX + columnWidth - 1;
            maxY = row == RegionRows - 1 ? lastY : minY + rowHeight - 1;

            minX += RegionInset;
            minY += RegionInset;
            maxX -= RegionInset;
            maxY -= RegionInset;
        }

        /// 덩어리가 놓일 칸과 그 둘레 한 칸이 모두 비어 있어야 한다.
        /// 둘레까지 보는 이유는 덩어리끼리 붙어 오목한 모양이 되는 것을 막기 위해서다.
        private static bool CanPlace(RoomGrid grid, HashSet<RoomCell> keepClear,
            int x, int y, int width, int height)
        {
            for (int dy = -1; dy <= height; dy++)
                for (int dx = -1; dx <= width; dx++)
                {
                    var cell = new RoomCell(x + dx, y + dy);

                    if (!grid.IsPassable(cell.X, cell.Y)) return false;
                    if (keepClear.Contains(cell)) return false;
                }

            return true;
        }

        /// 첫 칸에서 출발해 나머지 칸에 모두 닿는가.
        public static bool IsAllReachable(RoomGrid grid, IReadOnlyList<RoomCell> cells)
        {
            if (cells.Count == 0) return true;

            var field = new FlowField(grid.Width, grid.Height);
            field.Rebuild(grid, cells[0].X, cells[0].Y);

            foreach (RoomCell cell in cells)
                if (field.GetDistance(cell.X, cell.Y) == FlowField.Unreachable) return false;

            return true;
        }
    }
}
