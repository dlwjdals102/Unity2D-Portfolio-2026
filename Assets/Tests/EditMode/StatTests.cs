using System.Collections.Generic;
using NUnit.Framework;
using JM2D.Logic;

namespace JM2D.Tests
{
    public class StatTests
    {
        /// 실수 비교에는 허용 오차가 필요하다. 아래 주석 참고.
        private const float Tolerance = 0.0001f;

        [Test]
        public void 모디파이어가_없으면_기본값이_나온다()
        {
            var stat = new Stat(10f);

            Assert.AreEqual(10f, stat.Value, Tolerance);
        }

        [Test]
        public void 가산_모디파이어가_더해진다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f));

            Assert.AreEqual(15f, stat.Value, Tolerance);
        }

        [Test]
        public void 가산_모디파이어_둘이_모두_더해진다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f));
            stat.AddModifier(new StatModifier(ModifierType.Flat, 3f));

            Assert.AreEqual(18f, stat.Value, Tolerance);
        }

        [Test]
        public void 음수_모디파이어는_빼진다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, -3f));

            Assert.AreEqual(7f, stat.Value, Tolerance);
        }

        /// RemoveAll 이 조건부 삭제였을 때 음수만 살아남던 버그를 잡는다.
        [Test]
        public void 전부_제거하면_음수까지_사라진다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f));
            stat.AddModifier(new StatModifier(ModifierType.Flat, -3f));
            stat.RemoveAll();

            Assert.AreEqual(10f, stat.Value, Tolerance);
        }

        [Test]
        public void 승산_가산은_합쳐서_한_번_곱해진다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.2f));
            stat.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.2f));

            Assert.AreEqual(14f, stat.Value, Tolerance);
        }

        [Test]
        public void 승산_승산은_각각_곱해진다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.PercentMult, 0.2f));
            stat.AddModifier(new StatModifier(ModifierType.PercentMult, 0.2f));

            Assert.AreEqual(14.4f, stat.Value, Tolerance);
        }

        [Test]
        public void 세_종류가_섞이면_가산_승산가산_승산승산_순서로_적용된다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f));
            stat.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.2f));
            stat.AddModifier(new StatModifier(ModifierType.PercentMult, 0.5f));

            Assert.AreEqual(27f, stat.Value, Tolerance);
        }

        /// 눈으로는 확인할 수 없는 항목이다. 테스트가 유일한 검증 수단이다.
        [Test]
        public void 추가한_순서를_바꿔도_결과가_같다()
        {
            var forward = new Stat(10f);
            forward.AddModifier(new StatModifier(ModifierType.Flat, 5f));
            forward.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.2f));
            forward.AddModifier(new StatModifier(ModifierType.PercentMult, 0.5f));

            var backward = new Stat(10f);
            backward.AddModifier(new StatModifier(ModifierType.PercentMult, 0.5f));
            backward.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.2f));
            backward.AddModifier(new StatModifier(ModifierType.Flat, 5f));

            Assert.AreEqual(forward.Value, backward.Value, Tolerance);
        }

        /// 눈으로는 확인할 수 없는 항목이다. 테스트가 유일한 검증 수단이다.
        [Test]
        public void 한_출처만_제거되고_다른_출처는_남는다()
        {
            var 검 = new object();
            var 반지 = new object();

            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f, 검));
            stat.AddModifier(new StatModifier(ModifierType.Flat, 3f, 반지));

            stat.RemoveAllFrom(검);

            Assert.AreEqual(13f, stat.Value, Tolerance);
        }

        /// 값이 똑같아도 출처가 다르면 구분되어야 한다.
        /// 값으로 찾는 구현이었다면 여기서 무너진다.
        [Test]
        public void 값이_같아도_출처가_다르면_구분된다()
        {
            var 검A = new object();
            var 검B = new object();

            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 10f, 검A));
            stat.AddModifier(new StatModifier(ModifierType.Flat, 10f, 검B));

            stat.RemoveAllFrom(검A);

            Assert.AreEqual(20f, stat.Value, Tolerance);
        }

        /// 아이템 하나가 같은 스탯에 여러 모디파이어를 붙이는 경우.
        [Test]
        public void 한_출처가_붙인_모디파이어는_전부_제거된다()
        {
            var 장갑 = new object();

            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f, 장갑));
            stat.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.5f, 장갑));

            stat.RemoveAllFrom(장갑);

            Assert.AreEqual(10f, stat.Value, Tolerance);
        }

        // ── 아래 셋은 캐싱이 들어온 뒤 캐시 무효화를 검사한다 ──
        // 먼저 한 번 읽어 캐시를 채운 다음, 바꾸고 다시 읽는다.

        [Test]
        public void 추가하면_캐시가_갱신된다()
        {
            var stat = new Stat(10f);
            Assert.AreEqual(10f, stat.Value, Tolerance);

            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f));

            Assert.AreEqual(15f, stat.Value, Tolerance);
        }

        [Test]
        public void 전부_제거하면_캐시가_갱신된다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f));
            Assert.AreEqual(15f, stat.Value, Tolerance);

            stat.RemoveAll();

            Assert.AreEqual(10f, stat.Value, Tolerance);
        }

        [Test]
        public void 출처별로_제거해도_캐시가_갱신된다()
        {
            var 검 = new object();

            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f, 검));
            Assert.AreEqual(15f, stat.Value, Tolerance);

            stat.RemoveAllFrom(검);

            Assert.AreEqual(10f, stat.Value, Tolerance);
        }

        // ── 아래 셋은 정수 스탯의 반올림 규칙을 못 박는다 ──

        [Test]
        public void 정수값은_반올림된다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.15f));

            Assert.AreEqual(12, stat.IntValue);
        }

        /// C# 기본 반올림은 은행가 반올림이라 2.5 를 2 로 만든다.
        /// MidpointRounding.AwayFromZero 를 빠뜨리면 이 테스트가 잡는다.
        [Test]
        public void 정수값은_점오에서_올라간다()
        {
            var stat = new Stat(2.5f);

            Assert.AreEqual(3, stat.IntValue);
        }

        [Test]
        public void 정수값은_점오_미만이면_내려간다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.14f));

            Assert.AreEqual(11, stat.IntValue);
        }

        // ── 아래 넷은 변경 알림을 검사한다 ──

        [Test]
        public void 추가하면_변경이_알려진다()
        {
            var stat = new Stat(10f);
            int 알림횟수 = 0;
            stat.OnChanged += () => 알림횟수++;

            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f));

            Assert.AreEqual(1, 알림횟수);
        }

        [Test]
        public void 전부_제거해도_변경이_알려진다()
        {
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f));

            int 알림횟수 = 0;
            stat.OnChanged += () => 알림횟수++;
            stat.RemoveAll();

            Assert.AreEqual(1, 알림횟수);
        }

        [Test]
        public void 출처별로_제거해도_변경이_알려진다()
        {
            var 검 = new object();
            var stat = new Stat(10f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 5f, 검));

            int 알림횟수 = 0;
            stat.OnChanged += () => 알림횟수++;
            stat.RemoveAllFrom(검);

            Assert.AreEqual(1, 알림횟수);
        }

        /// 적의 Stat 은 아무도 구독하지 않는다. ?. 을 빠뜨리면 여기서 터진다.
        [Test]
        public void 구독자가_없어도_예외가_나지_않는다()
        {
            var stat = new Stat(10f);

            Assert.DoesNotThrow(() => stat.AddModifier(new StatModifier(ModifierType.Flat, 5f)));
        }

        // ── 아래 둘은 아이템을 반복 장착·해제할 때의 버그 제보를 재현한다 ──

        /// 부츠를 다섯 번 끼고 다섯 번 뺀 뒤 다시 끼면 정상이어야 한다.
        [Test]
        public void 반복_장착과_해제_뒤에도_값이_정확하다()
        {
            var stat = new Stat(6f);
            var 인스턴스 = new List<object>();

            for (int i = 0; i < 5; i++)
            {
                var 부츠 = new object();
                인스턴스.Add(부츠);
                stat.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.2f, 부츠));
            }

            Assert.AreEqual(12f, stat.Value, Tolerance, "다섯 개를 낀 상태");

            for (int i = 인스턴스.Count - 1; i >= 0; i--)
                stat.RemoveAllFrom(인스턴스[i]);

            Assert.AreEqual(6f, stat.Value, Tolerance, "전부 뺀 상태");

            var 새부츠 = new object();
            stat.AddModifier(new StatModifier(ModifierType.PercentAdd, 0.2f, 새부츠));

            Assert.AreEqual(7.2f, stat.Value, Tolerance, "다시 하나를 낀 상태");
        }

        /// 뺀 순서가 넣은 순서와 달라도 남는 것이 정확해야 한다.
        [Test]
        public void 중간_것을_빼도_나머지가_남는다()
        {
            var 첫째 = new object();
            var 둘째 = new object();
            var 셋째 = new object();

            var stat = new Stat(1f);
            stat.AddModifier(new StatModifier(ModifierType.Flat, 2f, 첫째));
            stat.AddModifier(new StatModifier(ModifierType.Flat, 2f, 둘째));
            stat.AddModifier(new StatModifier(ModifierType.Flat, 2f, 셋째));

            stat.RemoveAllFrom(둘째);

            Assert.AreEqual(5f, stat.Value, Tolerance);
        }
    }
}
