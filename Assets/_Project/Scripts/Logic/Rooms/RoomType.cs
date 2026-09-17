namespace JM2D.Logic.Rooms
{
    /// 방 배치의 칸 하나에 놓인 방의 종류.
    /// 저장하지 않고 순번으로 계산한다. RoomLayout.GetRoomType
    ///
    /// None 이 첫 값이어야 한다. C# 은 새 배열과 필드를 0 으로 채우므로,
    /// 다른 값이 첫 자리에 오면 아무것도 넣지 않은 칸이 그 방이 된다.
    public enum RoomType
    {
        /// 방이 없는 칸.
        None,

        /// 0번 방. 판이 시작되는 곳.
        Start,

        /// 시작과 보스 사이의 방.
        Combat,

        /// 마지막 방.
        Boss
    }
}
