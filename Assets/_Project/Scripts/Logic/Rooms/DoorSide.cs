namespace JM2D.Logic.Rooms
{
    /// 방의 네 변. 문은 변의 가운데에 있다.
    /// 북쪽은 y+1 이고, 변 너머의 칸은 RoomCell.Neighbor 가 준다.
    public enum DoorSide
    {
        North,
        East,
        South,
        West
    }
}
