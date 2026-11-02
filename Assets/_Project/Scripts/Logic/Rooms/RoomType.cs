namespace JM2D.Logic.Rooms
{
    /// 방의 종류. 저장하지 않고 RoomLayout.GetRoomType 이 순번으로 계산한다.
    /// None 이 첫 값이어야 한다. C# 이 새 배열을 0 으로 채우므로, 다른 값이 오면 빈 칸이 그 방이 된다.
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
