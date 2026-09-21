namespace JM2D.Rooms
{
    /// 방 하나의 전투 진행. 한 방향으로만 넘어간다. 안 싸움, 싸우는 중, 끝남.
    /// 판 도중에만 쓰고 배치 생성은 모르므로 Logic 이 아니라 여기 둔다.
    public enum RoomState
    {
        /// 아직 들어서지 않았다. 들어서면 전투가 시작된다.
        NotStarted,

        /// 문이 닫히고 웨이브가 나오는 중이다.
        Fighting,

        /// 전투가 끝났거나 처음부터 싸울 것이 없다(시작 방). 다시 싸우지 않는다.
        Cleared
    }
}
