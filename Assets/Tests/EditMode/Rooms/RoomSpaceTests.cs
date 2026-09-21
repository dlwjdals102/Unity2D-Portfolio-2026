using NUnit.Framework;
using JM2D.Logic.Rooms;

namespace JM2D.Tests.Rooms
{
    /// 칸 좌표와 월드 위치 사이의 변환. 방 크기는 지금 방 프리팹과 같은 32 x 18 이다.
    /// 가로와 세로가 달라야 둘을 바꿔 쓰는 실수가 드러난다.
    public class RoomSpaceTests
    {
        private const int Width = 32;
        private const int Height = 18;
        private const float Tolerance = 0.0001f;

        [TestCase(0, 0, 0f, 0f)]
        [TestCase(1, 0, 32f, 0f)]
        [TestCase(0, 1, 0f, 18f)]
        [TestCase(-1, 2, -32f, 36f)]
        public void 칸의_가운데를_월드_위치로_바꾼다(int cellX, int cellY, float x, float y)
        {
            RoomSpace.ToWorld(new RoomCell(cellX, cellY), Width, Height, out float worldX, out float worldY);

            Assert.AreEqual(x, worldX, Tolerance);
            Assert.AreEqual(y, worldY, Tolerance);
        }

        /// 방은 가운데에서 가로로 16, 세로로 9 까지 퍼져 있다. 그 안이면 같은 칸이다.
        [TestCase(0f, 0f, 0, 0)]
        [TestCase(15.9f, 0f, 0, 0)]
        [TestCase(-15.9f, 0f, 0, 0)]
        [TestCase(16.1f, 0f, 1, 0)]
        [TestCase(-16.1f, 0f, -1, 0)]
        [TestCase(0f, 8.9f, 0, 0)]
        [TestCase(0f, 9.1f, 0, 1)]
        [TestCase(40f, 20f, 1, 1)]
        public void 월드_위치가_속한_칸을_찾는다(float x, float y, int cellX, int cellY)
        {
            Assert.AreEqual(new RoomCell(cellX, cellY), RoomSpace.ToCell(x, y, Width, Height));
        }

        /// 두 방의 경계(문 통로 한가운데)는 늘 양의 방향 방으로 간다.
        /// 은행가 반올림(Mathf.RoundToInt)은 0.5 를 0 으로, 1.5 를 2 로 보내 경계가 방마다 다른 쪽으로 붙는다.
        [TestCase(16f, 0f, 1, 0)]
        [TestCase(48f, 0f, 2, 0)]
        [TestCase(-16f, 0f, 0, 0)]
        [TestCase(0f, 9f, 0, 1)]
        [TestCase(0f, 27f, 0, 2)]
        public void 두_방의_경계는_양의_방향_방에_속한다(float x, float y, int cellX, int cellY)
        {
            Assert.AreEqual(new RoomCell(cellX, cellY), RoomSpace.ToCell(x, y, Width, Height));
        }

        [TestCase(0, 0)]
        [TestCase(3, -2)]
        [TestCase(-4, 5)]
        public void 칸을_월드로_바꿨다가_되돌리면_제자리다(int cellX, int cellY)
        {
            var cell = new RoomCell(cellX, cellY);

            RoomSpace.ToWorld(cell, Width, Height, out float x, out float y);

            Assert.AreEqual(cell, RoomSpace.ToCell(x, y, Width, Height));
        }

        /// 여유 거리 3 이면 1번 방(가운데 x 32)의 안쪽은 x 19 ~ 45, y -6 ~ 6 이다.
        /// 칸 (0, 0) 만 쓰면 방 가운데가 원점이라 가운데를 빼지 않아도 통과한다. 그래서 1번 방과 -1번 방으로 시험한다.
        [TestCase(1, 0, 32f, 0f, true)]
        [TestCase(1, 0, 16.5f, 0f, false)]
        [TestCase(1, 0, 18.9f, 0f, false)]
        [TestCase(1, 0, 19f, 0f, true)]
        [TestCase(1, 0, 45f, 0f, true)]
        [TestCase(1, 0, 45.1f, 0f, false)]
        [TestCase(1, 0, 32f, 6f, true)]
        [TestCase(1, 0, 32f, 6.1f, false)]
        [TestCase(1, 0, 0f, 0f, false)]
        [TestCase(-1, 0, -32f, 0f, true)]
        [TestCase(-1, 0, -19f, 0f, true)]
        [TestCase(-1, 0, -18.9f, 0f, false)]
        public void 방_안으로_여유_거리만큼_들어왔는지_본다(int cellX, int cellY, float x, float y, bool expected)
        {
            const float margin = 3f;

            Assert.AreEqual(expected, RoomSpace.IsInside(new RoomCell(cellX, cellY), x, y, Width, Height, margin));
        }
    }
}
