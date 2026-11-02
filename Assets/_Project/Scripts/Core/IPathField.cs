using UnityEngine;

namespace JM2D.Core
{
    /// 쫓아가는 쪽에게 '여기서 어느 쪽으로 가면 되나' 를 알려 준다.
    /// 적은 이것만 알면 되고, 방과 격자와 BFS 는 모른다.
    public interface IPathField
    {
        /// 그 자리에서 대상 쪽으로 한 발 옮길 자리(이웃 칸의 가운데). 길을 모르면 거짓이다.
        /// 방향이 아니라 자리를 주는 것은, 쫓아가는 쪽이 닿을 때까지 붙잡고 가게 하려는 것이다.
        /// 매 프레임 방향을 새로 고르면 칸 경계에서 선택이 뒤집혀 제자리에서 떤다.
        /// '대상이 보이면 곧장 간다' 는 두 번 시도했다가 뺐다. 기둥 그늘의 경계에서 적이 맴돌았다.
        bool TryGetNextPoint(Vector2 from, out Vector2 point);

        /// 이 자리에 설 수 있는가. 벽이나 장애물 안이거나 방 밖이면 거짓이다.
        /// 길을 묻는 쪽이 아니라 무언가를 놓는 쪽이 쓴다. 보스의 잡몹 소환이 그렇다.
        bool CanStand(Vector2 point);
    }
}
