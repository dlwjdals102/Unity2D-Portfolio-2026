using System;

namespace JM2D.Logic.Rooms
{
    /// 방 배치 격자의 칸 하나.
    ///
    /// 북쪽은 y+1 이다. 월드 좌표와 같은 방향이라 칸 좌표에 방 크기를 곱하면 곧 방의 위치가 된다.
    /// 가방 격자(BagGrid)는 UI 라 y 가 아래로 커진다. 둘은 방향이 반대다.
    ///
    /// Logic 은 Vector2Int 를 쓸 수 있지만 지금까지 UnityEngine 없이 쓰였다. 그 모양을 지키고
    /// 이웃 칸 구하기를 칸 타입에 두려고 따로 만든다. 월드 좌표로 바꾸는 곳은 4-B 에 한 곳만 둔다.
    public readonly struct RoomCell : IEquatable<RoomCell>
    {
        public readonly int X;
        public readonly int Y;

        public RoomCell(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// 그 변 너머의 칸.
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

        // 목록에서 칸을 찾을 때(List.IndexOf) 이 비교가 쓰인다.
        // 구조체의 기본 Equals 는 필드를 리플렉션으로 비교하고 박싱이 생긴다.
        public bool Equals(RoomCell other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is RoomCell other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(RoomCell a, RoomCell b) => a.Equals(b);
        public static bool operator !=(RoomCell a, RoomCell b) => !a.Equals(b);

        public override string ToString() => $"({X}, {Y})";
    }
}
