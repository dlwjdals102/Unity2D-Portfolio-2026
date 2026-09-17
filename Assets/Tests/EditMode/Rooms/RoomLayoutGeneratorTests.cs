using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using JM2D.Logic.Rooms;

namespace JM2D.Tests.Rooms
{
    /// 시드로 만든 배치가 늘 지키는 것. 시드 하나가 아니라 여러 시드로 본다.
    /// 실패하면 그 시드와 배치 그림을 메시지에 찍는다.
    public class RoomLayoutGeneratorTests
    {
        private const int SeedCount = 1000;

        private static readonly DoorSide[] AllSides =
            { DoorSide.North, DoorSide.East, DoorSide.South, DoorSide.West };

        /// 배치를 숫자 그림으로 바꾼다. 윗줄이 북쪽이다. RoomLayoutTests 의 그림과 같은 모양이다.
        private static string 그림(RoomLayout layout)
        {
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach (RoomCell room in layout.Rooms)
            {
                minX = Math.Min(minX, room.X);
                maxX = Math.Max(maxX, room.X);
                minY = Math.Min(minY, room.Y);
                maxY = Math.Max(maxY, room.Y);
            }

            var text = new StringBuilder();
            for (int y = maxY; y >= minY; y--)
            {
                text.AppendLine();
                for (int x = minX; x <= maxX; x++)
                {
                    int index = layout.IndexOf(new RoomCell(x, y));
                    text.Append(index < 0 ? "." : index.ToString());
                    text.Append(' ');
                }
            }
            return text.ToString();
        }

        private static string 설명(int seed, RoomLayout layout) => $"시드 {seed}{그림(layout)}";

        [Test]
        public void 천_개의_시드에서_한_번도_막히지_않는다()
        {
            for (int seed = 0; seed < SeedCount; seed++)
            {
                int s = seed;
                Assert.DoesNotThrow(() => RoomLayoutGenerator.Generate(s), $"시드 {s}");
            }
        }

        [Test]
        public void 방이_정확히_여덟_개다()
        {
            Assert.AreEqual(8, RoomLayoutGenerator.RoomCount, "준비: 명세의 방 수");

            for (int seed = 0; seed < SeedCount; seed++)
            {
                RoomLayout layout = RoomLayoutGenerator.Generate(seed);
                Assert.AreEqual(RoomLayoutGenerator.RoomCount, layout.Rooms.Count, 설명(seed, layout));
            }
        }

        [Test]
        public void 시작_방은_원점이다()
        {
            for (int seed = 0; seed < SeedCount; seed++)
            {
                RoomLayout layout = RoomLayoutGenerator.Generate(seed);
                Assert.AreEqual(new RoomCell(0, 0), layout.Rooms[0], 설명(seed, layout));
            }
        }

        [Test]
        public void 두_방이_같은_칸에_없다()
        {
            for (int seed = 0; seed < SeedCount; seed++)
            {
                RoomLayout layout = RoomLayoutGenerator.Generate(seed);
                var seen = new HashSet<RoomCell>();

                foreach (RoomCell room in layout.Rooms)
                    Assert.IsTrue(seen.Add(room), $"{room} 에 방이 둘이다. {설명(seed, layout)}");
            }
        }

        /// 한 걸음은 상하좌우 한 칸이다. 대각선이나 두 칸 건너뛰기가 없다.
        [Test]
        public void 순번이_이어진_두_방은_상하좌우로_붙어_있다()
        {
            for (int seed = 0; seed < SeedCount; seed++)
            {
                RoomLayout layout = RoomLayoutGenerator.Generate(seed);

                for (int i = 1; i < layout.Rooms.Count; i++)
                {
                    RoomCell a = layout.Rooms[i - 1];
                    RoomCell b = layout.Rooms[i];
                    int distance = Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

                    Assert.AreEqual(1, distance, $"{i - 1} 번과 {i} 번. {설명(seed, layout)}");
                }
            }
        }

        /// 모든 방의 네 변을 세면 한 짝을 양쪽에서 한 번씩 센다. 일곱 짝이면 14 번이다.
        [Test]
        public void 문으로_이어진_짝이_일곱이다()
        {
            for (int seed = 0; seed < SeedCount; seed++)
            {
                RoomLayout layout = RoomLayoutGenerator.Generate(seed);

                int doors = 0;
                foreach (RoomCell room in layout.Rooms)
                    foreach (DoorSide side in AllSides)
                        if (layout.HasDoor(room, side)) doors++;

                Assert.AreEqual(14, doors, 설명(seed, layout));
            }
        }

        [Test]
        public void 같은_시드는_같은_배치를_낸다()
        {
            foreach (int seed in new[] { 0, 1, 42, 777, -5, int.MaxValue })
            {
                RoomLayout first = RoomLayoutGenerator.Generate(seed);
                RoomLayout second = RoomLayoutGenerator.Generate(seed);

                CollectionAssert.AreEqual(first.Rooms, second.Rooms, $"시드 {seed}");
            }
        }

        /// 가능한 배치가 2,172 가지라 시드 100 개면 거의 모두 다르게 나온다.
        /// 시드를 무시하고 늘 같은 배치를 내면 1 이 된다.
        [Test]
        public void 여러_시드에서_서로_다른_배치가_나온다()
        {
            var shapes = new HashSet<string>();
            for (int seed = 0; seed < 100; seed++)
                shapes.Add(그림(RoomLayoutGenerator.Generate(seed)));

            Assert.Greater(shapes.Count, 50);
        }

        /// 첫 걸음은 네 방향 모두 비어 있어 넷 다 나와야 한다.
        /// 후보를 고를 때 마지막이나 첫 후보가 빠지면 한 방향이 나오지 않는다.
        [Test]
        public void 첫_걸음은_네_방향이_모두_나온다()
        {
            var firstSteps = new HashSet<RoomCell>();
            for (int seed = 0; seed < SeedCount; seed++)
                firstSteps.Add(RoomLayoutGenerator.Generate(seed).Rooms[1]);

            Assert.AreEqual(4, firstSteps.Count);
        }
    }
}
