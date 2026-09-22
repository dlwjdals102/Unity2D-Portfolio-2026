using NUnit.Framework;
using JM2D.Logic.Rooms;

namespace JM2D.Tests.Rooms
{
    /// 방 안의 칸 격자. 어느 칸을 지나갈 수 있는지만 안다.
    /// 크기는 읽기 쉬운 8 x 5 를 쓴다. 가로와 세로가 달라야 둘을 바꿔 쓰는 실수가 드러난다.
    public class RoomGridTests
    {
        private const int Width = 8;
        private const int Height = 5;

        [TestCase(0, 0)]
        [TestCase(3, 2)]
        public void 새_격자는_모든_칸을_지나갈_수_있다(int x, int y)
        {
            var grid = new RoomGrid(Width, Height);

            Assert.IsTrue(grid.IsPassable(x, y));
        }

        [Test]
        public void 막은_칸은_지나갈_수_없다()
        {
            var grid = new RoomGrid(Width, Height);

            grid.Block(3, 2);

            Assert.IsFalse(grid.IsPassable(3, 2), "막은 칸");
            Assert.IsTrue(grid.IsPassable(4, 2), "옆 칸은 그대로다");
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(Width, 0)]
        [TestCase(0, Height)]
        public void 격자_밖은_지나갈_수_없다(int x, int y)
        {
            var grid = new RoomGrid(Width, Height);

            Assert.IsFalse(grid.IsPassable(x, y));
        }

        [Test]
        public void 같은_칸을_두_번_막아도_된다()
        {
            var grid = new RoomGrid(Width, Height);

            grid.Block(3, 2);
            grid.Block(3, 2);

            Assert.IsFalse(grid.IsPassable(3, 2));
        }

        [Test]
        public void 복사한_격자는_막힌_칸을_그대로_갖는다()
        {
            var grid = new RoomGrid(Width, Height);
            grid.Block(3, 2);

            var copy = new RoomGrid(grid);

            Assert.IsFalse(copy.IsPassable(3, 2), "막힌 칸");
            Assert.IsTrue(copy.IsPassable(4, 2), "막히지 않은 칸");
        }

        /// 장애물 배치는 실패하면 버리고 다시 놓는다. 사본을 막아도 원본이 더러워지면 안 된다.
        [Test]
        public void 사본을_막아도_원본은_그대로다()
        {
            var grid = new RoomGrid(Width, Height);
            var copy = new RoomGrid(grid);

            copy.Block(3, 2);

            Assert.IsTrue(grid.IsPassable(3, 2));
        }
    }
}
