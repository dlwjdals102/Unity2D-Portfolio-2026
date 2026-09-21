using System;
using System.Linq;
using NUnit.Framework;
using JM2D.Logic.Common;

namespace JM2D.Tests.Common
{
    /// 번호 여럿에서 서로 다른 몇 개를 뽑는 계산.
    /// 보상 후보를 고르는 데 쓴다. 숫자는 그 값(아이템 15종에서 3개)을 따른다.
    public class RandomPickTests
    {
        private const int ItemCount = 15;
        private const int RewardCount = 3;

        [TestCase(ItemCount, RewardCount)]
        [TestCase(ItemCount, 1)]
        [TestCase(5, 5)]
        public void 요청한_개수만큼_뽑는다(int count, int pick)
        {
            int[] picked = RandomPick.Distinct(count, pick, new Random(7));

            Assert.AreEqual(pick, picked.Length);
        }

        [Test]
        public void 뽑은_번호는_서로_다르다()
        {
            int[] picked = RandomPick.Distinct(ItemCount, RewardCount, new Random(7));

            Assert.AreEqual(picked.Length, picked.Distinct().Count(), $"중복이 있다: {string.Join(", ", picked)}");
        }

        [Test]
        public void 뽑은_번호는_0_이상_종류_수_미만이다()
        {
            int[] picked = RandomPick.Distinct(ItemCount, RewardCount, new Random(7));

            foreach (int index in picked)
                Assert.That(index, Is.InRange(0, ItemCount - 1), $"범위 밖: {index}");
        }

        /// 시드 하나만 보면 앞자리 안에서만 섞는 실수가 통과한다.
        /// 뽑히는 번호가 0, 1, 2 에만 몰리는지도 함께 본다.
        [Test]
        public void 시드_1000개에서_모두_서로_다르고_범위_안이다()
        {
            var seen = new bool[ItemCount];

            for (int seed = 0; seed < 1000; seed++)
            {
                int[] picked = RandomPick.Distinct(ItemCount, RewardCount, new Random(seed));

                Assert.AreEqual(RewardCount, picked.Distinct().Count(), $"시드 {seed} 에서 중복: {string.Join(", ", picked)}");

                foreach (int index in picked)
                {
                    Assert.That(index, Is.InRange(0, ItemCount - 1), $"시드 {seed} 에서 범위 밖: {index}");
                    seen[index] = true;
                }
            }

            Assert.IsTrue(seen.All(x => x), "1000번을 뽑는 동안 한 번도 나오지 않은 번호가 있다");
        }

        [TestCase(1)]
        [TestCase(17)]
        [TestCase(12345)]
        public void 같은_시드면_같은_셋이_나온다(int seed)
        {
            int[] first = RandomPick.Distinct(ItemCount, RewardCount, new Random(seed));
            int[] second = RandomPick.Distinct(ItemCount, RewardCount, new Random(seed));

            CollectionAssert.AreEqual(first, second);
        }

        [TestCase(3, 4)]
        [TestCase(0, 1)]
        public void 종류보다_많이_뽑으려_하면_예외다(int count, int pick)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => RandomPick.Distinct(count, pick, new Random(7)));
        }
    }
}
