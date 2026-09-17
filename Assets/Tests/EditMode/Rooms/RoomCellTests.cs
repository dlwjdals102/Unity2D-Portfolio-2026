using NUnit.Framework;
using JM2D.Logic.Rooms;

namespace JM2D.Tests.Rooms
{
    /// 방 배치의 칸. 북쪽이 y+1 이라는 약속을 지킨다.
    public class RoomCellTests
    {
        [TestCase(DoorSide.North, 2, 4)]
        [TestCase(DoorSide.East, 3, 3)]
        [TestCase(DoorSide.South, 2, 2)]
        [TestCase(DoorSide.West, 1, 3)]
        public void 변_너머의_칸을_구한다(DoorSide side, int x, int y)
        {
            Assert.AreEqual(new RoomCell(x, y), new RoomCell(2, 3).Neighbor(side));
        }

        [Test]
        public void 네_변_너머로_갔다가_반대로_오면_제자리다()
        {
            var cell = new RoomCell(-1, 5);

            Assert.AreEqual(cell, cell.Neighbor(DoorSide.North).Neighbor(DoorSide.South));
            Assert.AreEqual(cell, cell.Neighbor(DoorSide.East).Neighbor(DoorSide.West));
        }

        [Test]
        public void 좌표가_같으면_같은_칸이다()
        {
            Assert.IsTrue(new RoomCell(1, 2) == new RoomCell(1, 2));
            Assert.IsFalse(new RoomCell(1, 2) == new RoomCell(2, 1));
        }
    }
}
