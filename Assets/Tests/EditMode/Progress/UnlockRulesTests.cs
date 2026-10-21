using NUnit.Framework;
using JM2D.Logic.Progress;

namespace JM2D.Tests.Progress
{
    public class UnlockRulesTests
    {
        /// 조건에 쓰이는 두 값만 채운다. 나머지는 이 테스트와 상관이 없다.
        private static SaveData 기록(int bossKills, int rooms)
        {
            return new SaveData { BossKills = bossKills, BestRoomsReached = rooms };
        }

        [Test]
        public void 처음에는_시작_무기만_열려_있다()
        {
            SaveData save = 기록(0, 0);

            Assert.IsTrue(UnlockRules.IsUnlocked(0, save), "0번은 처음부터 열려 있어야 한다");
            Assert.IsFalse(UnlockRules.IsUnlocked(1, save));
            Assert.IsFalse(UnlockRules.IsUnlocked(2, save));
        }

        [Test]
        public void 보스를_잡기_전에는_검이_잠겨_있다()
        {
            Assert.IsFalse(UnlockRules.IsUnlocked(1, 기록(0, 8)));
        }

        [Test]
        public void 보스를_한_번_잡으면_검이_열린다()
        {
            Assert.IsTrue(UnlockRules.IsUnlocked(1, 기록(1, 0)));
        }

        [Test]
        public void 다섯째_방까지는_창이_잠겨_있다()
        {
            Assert.IsFalse(UnlockRules.IsUnlocked(2, 기록(5, 5)));
        }

        [Test]
        public void 여섯째_방에_닿으면_창이_열린다()
        {
            Assert.IsTrue(UnlockRules.IsUnlocked(2, 기록(0, 6)));
        }

        [Test]
        public void 없는_무기_번호는_잠긴_것으로_답한다()
        {
            Assert.IsFalse(UnlockRules.IsUnlocked(-1, 기록(9, 9)));
            Assert.IsFalse(UnlockRules.IsUnlocked(99, 기록(9, 9)));
        }

        [Test]
        public void 열린_무기는_조건_글이_비어_있다()
        {
            Assert.AreEqual("", UnlockRules.ConditionText(0));
            Assert.IsNotEmpty(UnlockRules.ConditionText(1));
        }

        [Test]
        public void 보스를_잡은_판은_처치_수가_하나_는다()
        {
            SaveData after = UnlockRules.After(기록(2, 8), bossKilled: true, roomsReached: 8);

            Assert.AreEqual(3, after.BossKills);
        }

        [Test]
        public void 죽은_판은_처치_수를_늘리지_않는다()
        {
            SaveData after = UnlockRules.After(기록(2, 8), bossKilled: false, roomsReached: 3);

            Assert.AreEqual(2, after.BossKills);
        }

        [Test]
        public void 더_나쁜_판은_최고_기록을_깎지_않는다()
        {
            SaveData after = UnlockRules.After(기록(2, 8), bossKilled: false, roomsReached: 3);

            Assert.AreEqual(8, after.BestRoomsReached);
        }

        [Test]
        public void 더_좋은_판은_최고_기록을_올린다()
        {
            SaveData after = UnlockRules.After(기록(2, 3), bossKilled: false, roomsReached: 8);

            Assert.AreEqual(8, after.BestRoomsReached);
        }

        [Test]
        public void 갱신해도_원본은_바뀌지_않는다()
        {
            SaveData before = 기록(2, 3);

            UnlockRules.After(before, bossKilled: true, roomsReached: 8);

            Assert.AreEqual(2, before.BossKills, "After 는 새 값을 돌려주고 받은 것을 고치지 않는다");
            Assert.AreEqual(3, before.BestRoomsReached);
        }

        [Test]
        public void 고른_무기와_포맷_번호는_판이_끝나도_그대로다()
        {
            var before = new SaveData { SelectedWeapon = 2, Version = 1 };

            SaveData after = UnlockRules.After(before, bossKilled: true, roomsReached: 8);

            Assert.AreEqual(2, after.SelectedWeapon);
            Assert.AreEqual(1, after.Version);
        }
    }
}
