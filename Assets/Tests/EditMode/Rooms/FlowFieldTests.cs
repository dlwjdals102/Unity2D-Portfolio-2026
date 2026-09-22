using NUnit.Framework;
using JM2D.Logic.Rooms;

namespace JM2D.Tests.Rooms
{
    /// 한 칸에서 모든 칸까지 몇 걸음인지 적어 둔 표.
    /// 적은 이웃 중 숫자가 작은 쪽으로 걸어 플레이어에게 온다.
    /// 이웃은 대각선까지 여덟이고, 대각선은 양옆 두 칸이 모두 비어 있어야 지나간다.
    ///
    /// 8 x 5 방에 기둥을 세우고 (1, 2) 에서 시작한 경우가 이렇다. 위가 y 가 큰 쪽이다.
    ///   2  2  2  3  4  5  6  7
    ///   1  1  1  #  5  5  6  7
    ///   1  0  1  #  6  6  6  7
    ///   1  1  1  #  5  5  6  7
    ///   2  2  2  3  4  5  6  7
    public class FlowFieldTests
    {
        private const int Width = 8;
        private const int Height = 5;
        private const int StartX = 1;
        private const int StartY = 2;

        /// 가로 한가운데(x = 3)를 위아래로 막은 기둥. 위아래 끝은 트여 있어 돌아갈 수 있다.
        private static RoomGrid 기둥이_선_방()
        {
            var grid = new RoomGrid(Width, Height);

            grid.Block(3, 1);
            grid.Block(3, 2);
            grid.Block(3, 3);

            return grid;
        }

        [Test]
        public void 시작_칸은_0_이다()
        {
            var field = new FlowField(Width, Height);

            field.Rebuild(new RoomGrid(Width, Height), StartX, StartY);

            Assert.AreEqual(0, field.GetDistance(StartX, StartY));
        }

        [TestCase(5, 2, 4)]
        [TestCase(1, 4, 2)]
        [TestCase(7, 0, 6)]
        public void 빈_방에서는_가로세로_중_먼_쪽의_칸_수와_같다(int x, int y, int expected)
        {
            var field = new FlowField(Width, Height);

            field.Rebuild(new RoomGrid(Width, Height), StartX, StartY);

            Assert.AreEqual(expected, field.GetDistance(x, y));
        }

        /// 이 테스트가 이 기능의 핵심이다. 곧장 가면 4 걸음인데 기둥을 돌아 6 걸음이다.
        [Test]
        public void 기둥을_돌아가는_거리가_직선보다_크다()
        {
            var field = new FlowField(Width, Height);

            field.Rebuild(기둥이_선_방(), StartX, StartY);

            Assert.AreEqual(6, field.GetDistance(5, 2));
        }

        /// 대각선으로 모서리를 스치며 지나가면 몸이 낀다. 양옆이 모두 비어야 지나간다.
        /// 두 칸을 대각선으로만 이어 두면, 돌아가는 먼 길의 거리가 나와야 한다.
        [Test]
        public void 대각선으로_모서리를_뚫지_않는다()
        {
            var grid = new RoomGrid(Width, Height);

            grid.Block(2, 2);
            grid.Block(1, 1);

            var field = new FlowField(Width, Height);
            field.Rebuild(grid, StartX, StartY);

            Assert.AreEqual(6, field.GetDistance(2, 1), "모서리를 뚫으면 1 이 된다");
        }

        [Test]
        public void 닿을_수_없는_칸은_마이너스_1_이다()
        {
            var grid = new RoomGrid(Width, Height);

            // x = 6 을 위아래로 다 막으면 x = 7 줄에는 갈 수 없다.
            for (int y = 0; y < Height; y++)
                grid.Block(6, y);

            var field = new FlowField(Width, Height);
            field.Rebuild(grid, StartX, StartY);

            Assert.AreEqual(FlowField.Unreachable, field.GetDistance(7, 2));
        }

        [Test]
        public void 막힌_칸_자체도_마이너스_1_이다()
        {
            var field = new FlowField(Width, Height);

            field.Rebuild(기둥이_선_방(), StartX, StartY);

            Assert.AreEqual(FlowField.Unreachable, field.GetDistance(3, 2));
        }

        [TestCase(-1, 0)]
        [TestCase(Width, 0)]
        public void 격자_밖을_물으면_마이너스_1_이다(int x, int y)
        {
            var field = new FlowField(Width, Height);

            field.Rebuild(new RoomGrid(Width, Height), StartX, StartY);

            Assert.AreEqual(FlowField.Unreachable, field.GetDistance(x, y));
        }

        /// 표를 다시 쓰므로 지난 계산이 남으면 안 된다.
        [Test]
        public void 다시_계산하면_옛_값이_남지_않는다()
        {
            var blocked = new RoomGrid(Width, Height);

            for (int y = 0; y < Height; y++)
                blocked.Block(6, y);

            var field = new FlowField(Width, Height);

            field.Rebuild(blocked, StartX, StartY);
            Assert.AreEqual(FlowField.Unreachable, field.GetDistance(7, 2), "준비: 막힌 방에서는 갈 수 없다");

            field.Rebuild(new RoomGrid(Width, Height), StartX, StartY);

            Assert.AreEqual(6, field.GetDistance(7, 2));
        }
    }
}
