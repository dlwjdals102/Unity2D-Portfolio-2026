using NUnit.Framework;
using JM2D.Logic.Common;

namespace JM2D.Tests.Common
{
    /// 들어가는 값과 나오는 값이 다른 거리 판정.
    /// 숫자는 추적 근접 적의 공격 범위(1.2)와 공격 이탈 범위(1.6)다.
    public class HysteresisTests
    {
        private const float Enter = 1.2f;
        private const float Exit = 1.6f;

        [Test]
        public void 밖에서_들어가는_값_안으로_오면_들어간다()
        {
            Assert.IsTrue(Hysteresis.IsInside(false, 1.0f, Enter, Exit));
        }

        /// 지금 코드가 distance <= 공격 범위 로 들어가므로 같으면 들어간다.
        [Test]
        public void 밖에서_들어가는_값과_같으면_들어간다()
        {
            Assert.IsTrue(Hysteresis.IsInside(false, Enter, Enter, Exit));
        }

        [Test]
        public void 밖에서_사이_구간이면_밖에_머문다()
        {
            Assert.IsFalse(Hysteresis.IsInside(false, 1.4f, Enter, Exit));
        }

        [Test]
        public void 안에서_사이_구간이면_안에_머문다()
        {
            Assert.IsTrue(Hysteresis.IsInside(true, 1.4f, Enter, Exit));
        }

        /// 지금 코드가 distance > 공격 이탈 범위 로 나오므로 같으면 아직 안이다.
        [Test]
        public void 안에서_나오는_값과_같으면_안에_머문다()
        {
            Assert.IsTrue(Hysteresis.IsInside(true, Exit, Enter, Exit));
        }

        [Test]
        public void 안에서_나오는_값_밖으로_가면_나온다()
        {
            Assert.IsFalse(Hysteresis.IsInside(true, 1.8f, Enter, Exit));
        }

        /// 이 함수가 있는 이유다. 사이 구간을 오가는 동안 상태가 한 번도 뒤집히지 않아야 한다.
        /// enter 와 exit 을 뒤바꿔 쓰면 여기서 매 스텝 뒤집힌다.
        [Test]
        public void 사이_구간을_오가도_뒤집히지_않는다()
        {
            float[] distances = { 1.3f, 1.5f, 1.3f, 1.5f, 1.4f };

            bool inside = true;
            foreach (float d in distances)
            {
                inside = Hysteresis.IsInside(inside, d, Enter, Exit);
                Assert.IsTrue(inside, $"안에서 시작해 {d} 에서 나갔다");
            }

            inside = false;
            foreach (float d in distances)
            {
                inside = Hysteresis.IsInside(inside, d, Enter, Exit);
                Assert.IsFalse(inside, $"밖에서 시작해 {d} 에서 들어왔다");
            }
        }
    }
}
