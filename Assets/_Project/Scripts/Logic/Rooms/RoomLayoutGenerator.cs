using System;
using System.Collections.Generic;

namespace JM2D.Logic.Rooms
{
    /// 시드 하나로 한 판의 방 배치를 만든다.
    /// 시작 칸 (0, 0) 에서 빈 이웃으로 한 칸씩 걷고, 걸은 순서가 곧 진행 순서다.
    /// 격자가 없어 가장자리가 없다. 방 8개는 7걸음이라 스스로 갇히지 않는다.
    public static class RoomLayoutGenerator
    {
        /// 시작 1, 전투 6, 보스 1.
        public const int RoomCount = 8;

        private static readonly DoorSide[] Sides =
            { DoorSide.North, DoorSide.East, DoorSide.South, DoorSide.West };

        public static RoomLayout Generate(int seed)
        {
            var random = new Random(seed);
            var rooms = new List<RoomCell> { new RoomCell(0, 0) };
            var candidates = new List<RoomCell>(Sides.Length);

            while (rooms.Count < RoomCount)
            {
                // 지금 서 있는 칸은 방금 놓은 방이다.
                RoomCell current = rooms[rooms.Count - 1];

                candidates.Clear();
                foreach (DoorSide side in Sides)
                {
                    RoomCell next = current.Neighbor(side);
                    if (!rooms.Contains(next)) candidates.Add(next);
                }

                // Next 는 넘긴 수를 포함하지 않는다. 후보가 넷이면 0 부터 3 까지 나온다.
                rooms.Add(candidates[random.Next(candidates.Count)]);
            }

            return new RoomLayout(rooms);
        }
    }
}
