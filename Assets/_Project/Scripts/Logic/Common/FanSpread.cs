using System;

namespace JM2D.Logic.Common
{
    /// 한 방향을 가운데 두고 여러 개를 부챗살처럼 벌릴 각도를 계산한다.
    /// UnityEngine 을 모르므로 방향이 아니라 '가운데에서 몇 도 틀어졌는지' 만 돌려준다.
    /// 돌리는 것은 이 값을 받는 쪽이 한다.
    public static class FanSpread
    {
        /// count 개를 전체 폭 spreadDegrees 안에 고르게 벌릴 때,
        /// index 번째가 가운데에서 틀어진 각도. 한쪽이 음수, 반대쪽이 양수다.
        public static float AngleOffset(int index, int count, float spreadDegrees)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "발 수는 1 이상이어야 한다");

            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException(nameof(index), "번호가 발 수 밖이다");

            // 하나뿐이면 나눌 것이 없다. 아래 식의 count - 1 이 0 이 되는 것도 여기서 막힌다.
            if (count == 1) return 0f;

            // 전체 폭을 칸 수로 나눈 한 칸 크기. 칸 수는 발 수보다 하나 적다.
            float step = spreadDegrees / (count - 1);

            return -spreadDegrees * 0.5f + step * index;
        }
    }
}
