namespace JM2D.Logic
{
    /// 들어가는 값과 나오는 값이 다른 거리 판정.
    /// 두 값 사이에서는 지금 상태를 지킨다. 그래서 경계에서 상태가 깜빡이지 않는다.
    /// 근거는 docs/decisions/state-transition-hysteresis.md 에 있다.
    public static class Hysteresis
    {
        /// 지금 '안' 에 있다고 볼 것인가.
        /// wasInside : 바로 전 스텝에 안이었는가
        /// enter     : 밖에 있을 때, 이 거리 안으로 오면 들어간다
        /// exit      : 안에 있을 때, 이 거리 밖으로 가면 나온다. enter 보다 크다
        public static bool IsInside(bool wasInside, float distance, float enter, float exit)
        {
            if (wasInside) return distance <= exit; // 안에 있었다면 어느 값으로 보나
            return distance <= enter;               // 밖에 있었다면 어느 값으로 보나
        }
    }
}
