using System;

namespace JM2D.Logic.Common
{
    /// 한 점 둘레에 여러 개를 고르게 나눠 놓을 자리를 계산한다.
    /// UnityEngine 을 모르므로 좌표를 x, y 로 따로 돌려준다.
    public static class CircleSpread
    {
        /// count 개를 반지름 radius 인 원 위에 같은 간격으로 놓을 때, index 번째가 중심에서 떨어진 만큼.
        public static void Offset(int index, int count, float radius, out float x, out float y)
        {
            double angle = 2 * Math.PI * index / count;
            x = (float)(Math.Cos(angle) * radius);
            y = (float)(Math.Sin(angle) * radius);
        }
    }
}
