using System;
using System.Collections.Generic;
using NUnit.Framework;
using JM2D.Logic.Rooms;

namespace JM2D.Tests.Rooms
{
    /// 방 안에 장애물을 놓을 칸을 정한다. 놓은 뒤에도 문과 출현 지점에 닿아야 한다.
    /// 크기는 방 프리팹과 같은 32 x 18 이고, 벽 한 줄과 네 변 가운데의 문 구멍(폭 4)을 그린다.
    public class ObstaclePlacerTests
    {
        private const int Width = 32;
        private const int Height = 18;
        private const int DoorWidth = 4;

        /// 벽만 그려진 격자. 네 변 가운데에 폭 4 의 문 구멍이 뚫려 있다.
        private static RoomGrid 벽과_문만_있는_방()
        {
            var doors = new HashSet<RoomCell>(문_칸들());
            var grid = new RoomGrid(Width, Height);

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    bool isWall = x == 0 || y == 0 || x == Width - 1 || y == Height - 1;

                    if (isWall && !doors.Contains(new RoomCell(x, y)))
                        grid.Block(x, y);
                }

            return grid;
        }

        /// 네 변 가운데의 문 구멍 칸들. 변마다 폭 4 씩이다.
        private static List<RoomCell> 문_칸들()
        {
            int centerX = Width / 2;
            int centerY = Height / 2;
            var cells = new List<RoomCell>();

            for (int i = 0; i < DoorWidth; i++)
            {
                int x = centerX - DoorWidth / 2 + i;
                int y = centerY - DoorWidth / 2 + i;

                cells.Add(new RoomCell(x, 0));
                cells.Add(new RoomCell(x, Height - 1));
                cells.Add(new RoomCell(0, y));
                cells.Add(new RoomCell(Width - 1, y));
            }

            return cells;
        }

        /// 방 프리팹의 출현 지점 여덟을 칸으로 옮긴 것. 방 가운데가 (16, 9) 다.
        private static List<RoomCell> 출현_지점들()
        {
            return new List<RoomCell>
            {
                new RoomCell(4, 15), new RoomCell(16, 15), new RoomCell(28, 15),
                new RoomCell(4, 9), new RoomCell(28, 9),
                new RoomCell(4, 3), new RoomCell(16, 3), new RoomCell(28, 3)
            };
        }

        /// 비워 둘 칸. 문 앞 세 칸과 출현 지점 둘레 한 칸이다.
        private static List<RoomCell> 비워_둘_칸들()
        {
            var cells = new List<RoomCell>();

            foreach (RoomCell door in 문_칸들())
                for (int step = 0; step <= 3; step++)
                {
                    if (door.X == 0) cells.Add(new RoomCell(step, door.Y));
                    else if (door.X == Width - 1) cells.Add(new RoomCell(Width - 1 - step, door.Y));
                    else if (door.Y == 0) cells.Add(new RoomCell(door.X, step));
                    else cells.Add(new RoomCell(door.X, Height - 1 - step));
                }

            foreach (RoomCell spawn in 출현_지점들())
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        cells.Add(new RoomCell(spawn.X + dx, spawn.Y + dy));

            return cells;
        }

        private static List<RoomCell> 닿아야_할_칸들()
        {
            var cells = new List<RoomCell>(문_칸들());
            cells.AddRange(출현_지점들());
            return cells;
        }

        private static List<RoomCell> 놓아_본다(int seed)
        {
            return ObstaclePlacer.Place(벽과_문만_있는_방(), 비워_둘_칸들(), 닿아야_할_칸들(), new Random(seed));
        }

        [Test]
        public void 장애물을_하나_이상_놓는다()
        {
            List<RoomCell> cells = 놓아_본다(7);

            Assert.Greater(cells.Count, 0);
        }

        [TestCase(1)]
        [TestCase(17)]
        [TestCase(12345)]
        public void 같은_시드면_같은_배치가_나온다(int seed)
        {
            CollectionAssert.AreEqual(놓아_본다(seed), 놓아_본다(seed));
        }

        [Test]
        public void 비워_둘_칸에는_놓지_않는다()
        {
            var keepClear = new HashSet<RoomCell>(비워_둘_칸들());

            for (int seed = 0; seed < 50; seed++)
                foreach (RoomCell cell in 놓아_본다(seed))
                    Assert.IsFalse(keepClear.Contains(cell), $"시드 {seed} 에서 비워 둘 칸 {cell} 에 놓았다");
        }

        [Test]
        public void 벽_안쪽에만_놓는다()
        {
            for (int seed = 0; seed < 50; seed++)
                foreach (RoomCell cell in 놓아_본다(seed))
                {
                    Assert.That(cell.X, Is.InRange(1, Width - 2), $"시드 {seed}");
                    Assert.That(cell.Y, Is.InRange(1, Height - 2), $"시드 {seed}");
                }
        }

        /// 이 테스트가 이 계단의 핵심이다. 시드 100개를 돌려 한 번도 길이 막히지 않는지 본다.
        [Test]
        public void 놓은_뒤에도_모든_문과_출현_지점에_닿는다()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                RoomGrid grid = 벽과_문만_있는_방();

                foreach (RoomCell cell in ObstaclePlacer.Place(grid, 비워_둘_칸들(), 닿아야_할_칸들(), new Random(seed)))
                    grid.Block(cell.X, cell.Y);

                Assert.IsTrue(ObstaclePlacer.IsAllReachable(grid, 닿아야_할_칸들()), $"시드 {seed} 에서 길이 막혔다");
            }
        }

        [Test]
        public void 길이_막힌_격자는_걸러진다()
        {
            RoomGrid grid = 벽과_문만_있는_방();

            // 방을 가로로 완전히 가른다. 문 구멍이 있는 양쪽 끝까지 막아야 위아래가 끊긴다.
            for (int x = 0; x < Width; x++)
                grid.Block(x, 9);

            Assert.IsFalse(ObstaclePlacer.IsAllReachable(grid, 닿아야_할_칸들()));
        }

        [Test]
        public void 놓을_곳이_없으면_빈_목록을_돌려준다()
        {
            var everywhere = new List<RoomCell>();

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    everywhere.Add(new RoomCell(x, y));

            List<RoomCell> cells = ObstaclePlacer.Place(
                벽과_문만_있는_방(), everywhere, 닿아야_할_칸들(), new Random(7));

            Assert.AreEqual(0, cells.Count);
        }
        /// 방 안쪽을 가로 3, 세로 2 로 나눈 구역마다 덩어리를 하나씩 놓는다.
        /// 한 덩어리는 많아야 네 칸(2x2)이므로, 한 구역에 다섯 칸 이상이면 둘이 놓인 것이다.
        [Test]
        public void 한_구역에_덩어리_하나만_놓는다()
        {
            const int firstX = 2, lastX = Width - 3, firstY = 2, lastY = Height - 3;
            const int columnWidth = (lastX - firstX + 1) / 3;
            const int rowHeight = (lastY - firstY + 1) / 2;

            for (int seed = 0; seed < 50; seed++)
            {
                var perRegion = new Dictionary<int, int>();

                foreach (RoomCell cell in 놓아_본다(seed))
                {
                    int column = Math.Min((cell.X - firstX) / columnWidth, 2);
                    int row = Math.Min((cell.Y - firstY) / rowHeight, 1);
                    int region = row * 3 + column;

                    perRegion.TryGetValue(region, out int count);
                    perRegion[region] = count + 1;
                }

                foreach (KeyValuePair<int, int> pair in perRegion)
                    Assert.LessOrEqual(pair.Value, 4, $"시드 {seed} 의 구역 {pair.Key} 에 덩어리가 둘 이상이다");
            }
        }
        /// 덩어리끼리 붙어 있으면 한 덩어리처럼 보인다. 서로 네 칸은 떨어져야 한다.
        /// 붙어 있는 칸들을 한 덩어리로 묶은 뒤 덩어리 사이 거리를 잰다.
        [Test]
        public void 덩어리끼리_네_칸_이상_떨어진다()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                List<List<RoomCell>> clusters = 덩어리로_묶는다(놓아_본다(seed));

                for (int i = 0; i < clusters.Count; i++)
                    for (int j = i + 1; j < clusters.Count; j++)
                        Assert.GreaterOrEqual(덩어리_사이_거리(clusters[i], clusters[j]), 4,
                            $"시드 {seed} 의 덩어리 {i} 와 {j} 가 붙어 있다");
            }
        }

        /// 서로 맞닿은(대각선 포함) 칸들을 한 덩어리로 묶는다.
        private static List<List<RoomCell>> 덩어리로_묶는다(List<RoomCell> cells)
        {
            var left = new HashSet<RoomCell>(cells);
            var clusters = new List<List<RoomCell>>();

            while (left.Count > 0)
            {
                var queue = new Queue<RoomCell>();
                var cluster = new List<RoomCell>();

                RoomCell first = 하나_꺼낸다(left);
                queue.Enqueue(first);

                while (queue.Count > 0)
                {
                    RoomCell cell = queue.Dequeue();
                    cluster.Add(cell);

                    for (int dy = -1; dy <= 1; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            var neighbor = new RoomCell(cell.X + dx, cell.Y + dy);

                            if (left.Remove(neighbor)) queue.Enqueue(neighbor);
                        }
                }

                clusters.Add(cluster);
            }

            return clusters;
        }

        private static RoomCell 하나_꺼낸다(HashSet<RoomCell> cells)
        {
            foreach (RoomCell cell in cells)
            {
                cells.Remove(cell);
                return cell;
            }

            throw new InvalidOperationException("비어 있다");
        }

        /// 두 덩어리에서 가장 가까운 두 칸의 체스판 거리.
        private static int 덩어리_사이_거리(List<RoomCell> a, List<RoomCell> b)
        {
            int best = int.MaxValue;

            foreach (RoomCell x in a)
                foreach (RoomCell y in b)
                    best = Math.Min(best, Math.Max(Math.Abs(x.X - y.X), Math.Abs(x.Y - y.Y)));

            return best;
        }
    }
}
