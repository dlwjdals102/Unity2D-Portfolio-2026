using System;

namespace JM2D.Logic.Progress
{
    /// 저장된 값을 보고 무엇이 열렸는지 답한다.
    /// 해금 목록을 저장하지 않는 이유가 여기 있다. 진실을 기록 하나로 두면
    /// 조건을 바꿨을 때 이미 저장된 기록에도 곧바로 적용된다.
    public static class UnlockRules
    {
        /// 무기 하나의 해금 조건. 조건이 두 종류뿐이라 둘을 한 자리에 담는다.
        /// 조건이 더 늘어나면 그때 종류별로 나눈다.
        private readonly struct Rule
        {
            public readonly int BossKills;
            public readonly int RoomsReached;
            public readonly string Text;

            public Rule(int bossKills, int roomsReached, string text)
            {
                BossKills = bossKills;
                RoomsReached = roomsReached;
                Text = text;
            }
        }

        /// 무기 번호가 곧 자리 번호다. PlayerAttack 의 무기 배열과 순서를 맞춘다.
        /// 조건 숫자는 여기 한 번만 적는다. 판정과 안내 문구가 같은 줄을 본다.
        private static readonly Rule[] Rules =
        {
            new Rule(0, 0, ""),
            new Rule(1, 0, "보스를 한 번 잡으면 열립니다"),
            new Rule(0, 6, "한 판에서 여섯 번째 방까지 가면 열립니다"),
        };

        /// 해금 규칙이 있는 무기 수. 마을의 선택대가 몇 칸을 그릴지 여기서 읽는다.
        public static int WeaponCount => Rules.Length;

        /// 처음부터 들고 있는 무기. 조건이 없는 Rules[0] 과 같은 번호여야 한다.
        /// 저장된 무기를 쓸 수 없을 때 돌아오는 자리이기도 하다.
        public const int StarterWeapon = 0;

        public static bool IsUnlocked(int weaponIndex, SaveData save)
        {
            if (weaponIndex < 0 || weaponIndex >= Rules.Length) return false;

            Rule rule = Rules[weaponIndex];

            return save.BossKills >= rule.BossKills
                && save.BestRoomsReached >= rule.RoomsReached;
        }

        /// 잠긴 무기 옆에 보일 글. 열려 있는 무기는 빈 글자다.
        public static string ConditionText(int weaponIndex)
        {
            if (weaponIndex < 0 || weaponIndex >= Rules.Length) return "";

            return Rules[weaponIndex].Text;
        }

        /// 판이 끝났을 때의 저장값을 만든다. 받은 것을 고치지 않고 새로 돌려준다.
        /// 고치는 쪽으로 두면 테스트에서 전과 후를 나란히 둘 수 없고,
        /// 부르는 쪽이 모르는 사이에 원본이 바뀐다.
        ///
        /// 두 기록의 성격이 다르다. 처치 수는 쌓이고, 도달한 방은 최고 기록이다.
        /// 둘을 같은 방식으로 쓰면 세 번째 방에서 죽은 판이 여덟 번째 기록을 지운다.
        public static SaveData After(SaveData save, bool bossKilled, int roomsReached)
        {
            return new SaveData
            {
                Version = save.Version,
                SelectedWeapon = save.SelectedWeapon,

                BossKills = save.BossKills + (bossKilled ? 1 : 0),
                BestRoomsReached = Math.Max(save.BestRoomsReached, roomsReached),
            };
        }

        /// 이번 판으로 새로 열린 무기 수.
        ///
        /// 전과 후를 모두 받는 이유는 열린 목록을 저장하지 않기 때문이다.
        /// 지금 열려 있는지는 기록에서 계산하므로, '새로' 열렸는지는 두 기록을 견줘야 안다.
        /// 전에 잠겨 있던 것만 센다. 지금 열린 것을 모두 세면 두 번째 클리어에서도
        /// 열렸다고 알리게 된다.
        public static int NewlyUnlockedCount(SaveData before, SaveData after)
        {
            int count = 0;

            for (int i = 0; i < Rules.Length; i++)
            {
                if (!IsUnlocked(i, before) && IsUnlocked(i, after)) count++;
            }

            return count;
        }
    }
}
