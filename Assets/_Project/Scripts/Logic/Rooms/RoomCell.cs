using System;

namespace JM2D.Logic.Rooms
{
    /// 방 배치 격자의 칸 하나.
    /// 북쪽은 y+1 이고 월드 좌표와 같은 방향이다. 가방 격자는 UI 라 y 가 아래로 커져 방향이 반대다.
    /// Vector2Int 를 쓰지 않는 것은 Logic 을 UnityEngine 없이 두고 이웃 칸 구하기를 칸 타입에 두려는 것이다.
    public readonly struct RoomCell : IEquatable<RoomCell>
    {
        public readonly int X;
        public readonly int Y;

        public RoomCell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public RoomCell Neighbor(DoorSide side)
        {
            switch (side)
            {
                case DoorSide.North: return new RoomCell(X, Y + 1);
                case DoorSide.East: return new RoomCell(X + 1, Y);
                case DoorSide.South: return new RoomCell(X, Y - 1);
                case DoorSide.West: return new RoomCell(X - 1, Y);
                default: throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        // List.IndexOf 가 이 비교를 쓴다. 구조체의 기본 Equals 는 리플렉션으로 비교하고 박싱이 생긴다.
        public bool Equals(RoomCell other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is RoomCell other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(RoomCell a, RoomCell b) => a.Equals(b);
        public static bool operator !=(RoomCell a, RoomCell b) => !a.Equals(b);

        public override string ToString() => $"({X}, {Y})";
    }
}
