using System;

namespace JM2D.Logic.Progress
{
    /// 판과 판 사이에 남는 값. 저장 파일에 그대로 들어간다.
    /// ScriptableObject 가 아니다. 인스펙터에서 만드는 데이터 에셋은 Scripts/Data 에 있다.
    /// 이 클래스는 파일도 씬도 모른다. 읽고 쓰는 것은 Runtime 의 SaveFile 이 맡는다.
    [Serializable]
    public class SaveData
    {
        /// 저장 포맷 번호. 다르면 기본값으로 시작한다.
        public int Version;

        /// 보스를 잡은 횟수. 판이 끝날 때마다 쌓인다.
        public int BossKills;

        /// 한 판에서 도달한 가장 깊은 방 번호. 더 좋은 값만 남는다.
        public int BestRoomsReached;

        /// 마을에서 고른 무기 번호.
        public int SelectedWeapon;
    }
}
