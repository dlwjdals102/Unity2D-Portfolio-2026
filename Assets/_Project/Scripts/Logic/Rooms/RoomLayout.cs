using System;
using System.Collections.Generic;

namespace JM2D.Logic.Rooms
{
    /// 한 판의 방 배치. 걸은 순서대로 쌓인 칸 목록 하나가 전부다.
    /// 종류와 문은 저장하지 않고 순번으로 계산한다. 기록이 하나라 어긋날 곳이 없다.
    public class RoomLayout
    {
        private readonly List<RoomCell> _rooms;

        /// 걸은 순서대로 쌓인 칸. 0번이 시작, 마지막이 보스다.
        public IReadOnlyList<RoomCell> Rooms => _rooms;

        /// 받은 목록을 복사해 둔다. 만든 쪽이 나중에 목록을 바꿔도 배치는 그대로다.
        public RoomLayout(IReadOnlyList<RoomCell> rooms)
        {
            _rooms = new List<RoomCell>(rooms);
        }

        /// 그 칸의 순번. 방이 없으면 -1 이다.
        public int IndexOf(RoomCell cell) => _rooms.IndexOf(cell);

        public RoomType GetRoomType(RoomCell cell)
        {
            int index = IndexOf(cell);

            if (index < 0) return RoomType.None;
            // 방이 하나면 0번이 곧 마지막이다. 시작을 먼저 본다.
            if (index == 0) return RoomType.Start;
            if (index == _rooms.Count - 1) return RoomType.Boss;
            return RoomType.Combat;
        }

        /// 그 변에 문이 있는가. 변 너머의 방과 순번이 하나 차이 날 때만 있다.
        /// 칸이 붙어 있어도 순번이 멀면 벽이다.
        public bool HasDoor(RoomCell cell, DoorSide side)
        {
            int index = IndexOf(cell);
            int neighborIndex = IndexOf(cell.Neighbor(side));

            // 빈 칸의 -1 과 시작 방의 0 도 하나 차이라, 양쪽에 방이 있는지 먼저 본다.
            if (index < 0 || neighborIndex < 0) return false;
            return Math.Abs(index - neighborIndex) == 1;
        }
    }
}
