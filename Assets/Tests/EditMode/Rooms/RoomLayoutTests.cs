using System.Collections.Generic;
using NUnit.Framework;
using JM2D.Logic.Rooms;

namespace JM2D.Tests.Rooms
{
    /// 손으로 그린 배치에 방 종류와 문을 묻는다. 걷기는 쓰지 않는다.
    ///
    /// 그림의 숫자는 걸은 순서이고 '.' 은 빈 칸이다. 칸은 공백으로 나눈다.
    /// 윗줄이 북쪽이라 윗줄일수록 y 가 크다. 맨 아랫줄이 y = 0, 왼쪽 칸이 x = 0 이다.
    public class RoomLayoutTests
    {
        private static readonly DoorSide[] AllSides =
            { DoorSide.North, DoorSide.East, DoorSide.South, DoorSide.West };

        private static RoomLayout 배치(params string[] rows)
        {
            var found = new Dictionary<int, RoomCell>();

            for (int r = 0; r < rows.Length; r++)
            {
                int y = rows.Length - 1 - r;
                string[] tokens = rows[r].Split(' ');

                for (int x = 0; x < tokens.Length; x++)
                {
                    if (tokens[x] == ".") continue;

                    int order = int.Parse(tokens[x]);
                    Assert.IsFalse(found.ContainsKey(order), $"준비: {order} 번이 그림에 두 번 있다");
                    found.Add(order, new RoomCell(x, y));
                }
            }

            var rooms = new List<RoomCell>();
            for (int i = 0; i < found.Count; i++)
            {
                Assert.IsTrue(found.ContainsKey(i), $"준비: 그림에 {i} 번이 없다");
                rooms.Add(found[i]);
            }

            return new RoomLayout(rooms);
        }

        private static RoomCell 칸(int x, int y) => new RoomCell(x, y);

        // 종류

        [Test]
        public void 순번으로_종류가_정해진다()
        {
            var layout = 배치("0 1 2 3");

            Assert.AreEqual(RoomType.Start, layout.GetRoomType(칸(0, 0)));
            Assert.AreEqual(RoomType.Combat, layout.GetRoomType(칸(1, 0)));
            Assert.AreEqual(RoomType.Combat, layout.GetRoomType(칸(2, 0)));
            Assert.AreEqual(RoomType.Boss, layout.GetRoomType(칸(3, 0)));
        }

        [Test]
        public void 방이_없는_칸은_None_이다()
        {
            var layout = 배치(
                ". 1",
                "0 .");

            Assert.AreEqual(RoomType.None, layout.GetRoomType(칸(0, 1)));
            Assert.AreEqual(RoomType.None, layout.GetRoomType(칸(-1, 0)));
            Assert.AreEqual(RoomType.None, layout.GetRoomType(칸(100, 100)));
        }

        /// 방이 하나면 0번이 곧 마지막이다. 시작인지 먼저 본다.
        /// 게임에서는 일어나지 않지만, 두 검사의 순서를 이 테스트가 정한다.
        [Test]
        public void 방이_하나뿐이면_시작_방이다()
        {
            Assert.AreEqual(RoomType.Start, 배치("0").GetRoomType(칸(0, 0)));
        }

        [Test]
        public void 넣은_순서대로_방이_남는다()
        {
            var layout = 배치(
                "3 2",
                "0 1");

            CollectionAssert.AreEqual(
                new[] { 칸(0, 0), 칸(1, 0), 칸(1, 1), 칸(0, 1) },
                layout.Rooms);
        }

        [Test]
        public void 넣은_목록을_나중에_바꿔도_배치는_그대로다()
        {
            var rooms = new List<RoomCell> { 칸(0, 0), 칸(1, 0) };
            var layout = new RoomLayout(rooms);

            rooms.Clear();

            Assert.AreEqual(2, layout.Rooms.Count);
        }

        // 문

        [Test]
        public void 가로로_이어진_두_방은_양쪽_모두_문이_있다()
        {
            var layout = 배치("0 1");

            Assert.IsTrue(layout.HasDoor(칸(0, 0), DoorSide.East));
            Assert.IsTrue(layout.HasDoor(칸(1, 0), DoorSide.West));
        }

        /// 윗줄이 1 번이다. 북쪽이 y+1 이 아니면 여기서 틀린다.
        [Test]
        public void 세로로_이어진_두_방은_양쪽_모두_문이_있다()
        {
            var layout = 배치(
                "1",
                "0");

            Assert.IsTrue(layout.HasDoor(칸(0, 0), DoorSide.North));
            Assert.IsTrue(layout.HasDoor(칸(0, 1), DoorSide.South));
        }

        [Test]
        public void 방이_없는_쪽_변에는_문이_없다()
        {
            var layout = 배치("0 1");

            Assert.IsFalse(layout.HasDoor(칸(0, 0), DoorSide.North));
            Assert.IsFalse(layout.HasDoor(칸(0, 0), DoorSide.South));
            Assert.IsFalse(layout.HasDoor(칸(0, 0), DoorSide.West));
            Assert.IsFalse(layout.HasDoor(칸(1, 0), DoorSide.East));
        }

        /// 0 번과 3 번은 칸이 붙었지만 순번이 셋 차이라 벽이다.
        [Test]
        public void U자로_붙은_두_방_사이에는_문이_없다()
        {
            var layout = 배치(
                "3 2",
                "0 1");

            Assert.IsFalse(layout.HasDoor(칸(0, 0), DoorSide.North));
            Assert.IsFalse(layout.HasDoor(칸(0, 1), DoorSide.South));
            Assert.IsTrue(layout.HasDoor(칸(0, 1), DoorSide.East), "3 번과 2 번은 이어져 있다");
        }

        /// 방이 없는 칸의 순번 -1 과 시작 방 0 은 하나 차이다.
        /// 차이만 보면 빈 칸과 시작 방 사이에 문이 생긴다.
        [Test]
        public void 빈_칸과_시작_방_사이에는_문이_없다()
        {
            var layout = 배치("0 1");

            Assert.IsFalse(layout.HasDoor(칸(-1, 0), DoorSide.East));
            Assert.IsFalse(layout.HasDoor(칸(0, -1), DoorSide.North));
        }

        [Test]
        public void 방이_없는_칸끼리는_문이_없다()
        {
            var layout = 배치("0 1");

            Assert.IsFalse(layout.HasDoor(칸(5, 5), DoorSide.East));
        }

        /// 방 8개를 한 줄로 이으면 문으로 이어진 짝이 7 개다.
        /// 모든 방의 네 변을 세면 한 짝을 양쪽에서 한 번씩, 14 번 센다.
        [Test]
        public void 방_여덟_개의_배치에서_문은_일곱_짝이다()
        {
            var layout = 배치(
                "4 5 6",
                "3 2 7",
                "0 1 .");

            int doors = 0;
            foreach (RoomCell room in layout.Rooms)
                foreach (DoorSide side in AllSides)
                    if (layout.HasDoor(room, side)) doors++;

            Assert.AreEqual(14, doors);
        }

        [Test]
        public void 시작_방과_보스_방은_문이_하나이고_전투_방은_둘이다()
        {
            var layout = 배치(
                "4 5 6",
                "3 2 7",
                "0 1 .");

            foreach (RoomCell room in layout.Rooms)
            {
                int doors = 0;
                foreach (DoorSide side in AllSides)
                    if (layout.HasDoor(room, side)) doors++;

                int expected = layout.GetRoomType(room) == RoomType.Combat ? 2 : 1;
                Assert.AreEqual(expected, doors, $"{room} 의 문 수");
            }
        }
    }
}
