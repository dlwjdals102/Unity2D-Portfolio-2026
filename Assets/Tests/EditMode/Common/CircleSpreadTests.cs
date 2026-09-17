using System;
using NUnit.Framework;
using JM2D.Logic.Common;

namespace JM2D.Tests.Common
{
    /// 한 점 둘레에 고르게 나눠 놓는 자리.
    /// 분열 적이 갈라질 때 작은 적을 놓는 데 쓴다. 숫자는 그 값(마릿수 3, 반지름 0.5)을 따른다.
    public class CircleSpreadTests
    {
        private const float Radius = 0.5f;
        private const float Tolerance = 0.0001f;

        private static (float x, float y)[] 자리들(int count, float radius)
        {
            var result = new (float x, float y)[count];
            for (int i = 0; i < count; i++)
            {
                CircleSpread.Offset(i, count, radius, out float x, out float y);
                result[i] = (x, y);
            }
            return result;
        }

        private static float 거리((float x, float y) a, (float x, float y) b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        public void 모든_자리가_중심에서_반지름만큼_떨어져_있다(int count)
        {
            foreach (var p in 자리들(count, Radius))
                Assert.AreEqual(Radius, 거리(p, (0f, 0f)), Tolerance);
        }

        /// 원이라 마지막 자리의 이웃은 첫 자리다.
        /// count - 1 로 나누면 마지막이 첫 자리와 같은 곳에 놓여 그 사이가 0 이 된다.
        /// 도를 라디안 자리에 넣으면 간격이 제각각이 된다.
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void 이웃한_자리끼리의_거리가_모두_같다(int count)
        {
            var points = 자리들(count, Radius);
            float first = 거리(points[0], points[1]);

            for (int i = 0; i < count; i++)
            {
                var next = points[(i + 1) % count];
                Assert.AreEqual(first, 거리(points[i], next), Tolerance, $"{i} 번과 그 다음 자리 사이");
            }
        }

        /// index / count 를 정수로 나누면 모든 각도가 0 이 되어 한 자리에 겹친다.
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void 어느_두_자리도_겹치지_않는다(int count)
        {
            var points = 자리들(count, Radius);

            for (int i = 0; i < count; i++)
                for (int j = i + 1; j < count; j++)
                    Assert.Greater(거리(points[i], points[j]), 0.01f, $"{i} 번과 {j} 번이 겹친다");
        }

        /// 이 값이 작은 적의 몸(0.6)보다 커서 셋이 겹치지 않는다. 명세의 계산을 그대로 지킨다.
        [Test]
        public void 셋을_반지름_0_5_로_놓으면_이웃_거리는_작은_적의_몸보다_크다()
        {
            var points = 자리들(3, Radius);

            Assert.AreEqual(0.866f, 거리(points[0], points[1]), 0.001f);
            Assert.Greater(거리(points[0], points[1]), 0.6f);
        }

        /// 0 번 자리가 늘 같은 곳이라 갈라지는 모양을 예측할 수 있다.
        [Test]
        public void 첫_자리는_중심의_오른쪽이다()
        {
            CircleSpread.Offset(0, 3, Radius, out float x, out float y);

            Assert.AreEqual(Radius, x, Tolerance);
            Assert.AreEqual(0f, y, Tolerance);
        }
    }
}
