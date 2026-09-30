using System;
using NUnit.Framework;
using JM2D.Logic.Common;

namespace JM2D.Tests.Common
{
    /// 한 방향을 가운데 두고 부챗살처럼 벌리는 각도.
    /// 보스의 사격에 쓴다. 숫자는 그 값(1페이즈 3발 30도, 2페이즈 5발)을 따른다.
    public class FanSpreadTests
    {
        private const float Spread = 30f;
        private const float Tolerance = 0.0001f;

        private static float[] 각도들(int count, float spreadDegrees)
        {
            var result = new float[count];
            for (int i = 0; i < count; i++)
                result[i] = FanSpread.AngleOffset(i, count, spreadDegrees);
            return result;
        }

        [TestCase(0f)]
        [TestCase(30f)]
        public void 한_발이면_퍼짐과_상관없이_가운데다(float spreadDegrees)
        {
            Assert.AreEqual(0f, FanSpread.AngleOffset(0, 1, spreadDegrees), Tolerance);
        }

        [Test]
        public void 세_발이면_가운데와_양끝이다()
        {
            float[] angles = 각도들(3, Spread);

            Assert.AreEqual(-15f, angles[0], Tolerance);
            Assert.AreEqual(0f, angles[1], Tolerance, "가운데 발은 틀어지지 않는다");
            Assert.AreEqual(15f, angles[2], Tolerance);
        }

        [Test]
        public void 네_발이면_가운데가_비고_고르게_벌어진다()
        {
            float[] angles = 각도들(4, Spread);

            Assert.AreEqual(-15f, angles[0], Tolerance);
            Assert.AreEqual(-5f, angles[1], Tolerance);
            Assert.AreEqual(5f, angles[2], Tolerance);
            Assert.AreEqual(15f, angles[3], Tolerance);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void 가운데를_기준으로_좌우_대칭이다(int count)
        {
            float[] angles = 각도들(count, Spread);

            for (int i = 0; i < count; i++)
                Assert.AreEqual(-angles[count - 1 - i], angles[i], Tolerance, $"{i} 번째와 그 짝");
        }

        [TestCase(3)]
        [TestCase(5)]
        public void 이웃한_두_발의_간격이_모두_같다(int count)
        {
            float[] angles = 각도들(count, Spread);
            float step = angles[1] - angles[0];

            Assert.Greater(step, 0f, "준비: 번호가 늘면 각도도 늘어야 한다");

            for (int i = 1; i < count - 1; i++)
                Assert.AreEqual(step, angles[i + 1] - angles[i], Tolerance, $"{i} 번째와 다음 사이");
        }

        [TestCase(2)]
        [TestCase(5)]
        public void 양_끝이_전체_폭만큼_벌어진다(int count)
        {
            float[] angles = 각도들(count, Spread);

            Assert.AreEqual(Spread, angles[count - 1] - angles[0], Tolerance);
        }

        [Test]
        public void 퍼짐이_0이면_전부_가운데다()
        {
            foreach (float angle in 각도들(5, 0f))
                Assert.AreEqual(0f, angle, Tolerance);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void 발_수가_1보다_작으면_예외(int count)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FanSpread.AngleOffset(0, count, Spread));
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void 번호가_발_수_밖이면_예외(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FanSpread.AngleOffset(index, 3, Spread));
        }
    }
}
