using System;

namespace JM2D.Logic.Common
{
    /// 번호 여럿에서 서로 다른 몇 개를 뽑는다. 게임 용어를 모르고 번호만 다룬다.
    /// 뽑은 번호로 무엇을 꺼낼지는 부르는 쪽이 정한다.
    public static class RandomPick
    {
        /// 0 부터 count-1 까지에서 서로 다른 pick 개를 뽑아 뽑은 순서대로 돌려준다.
        /// 뽑을 수가 종류 수보다 많으면 예외를 낸다. 조용히 줄이면 왜 적게 나왔는지 알 수 없다.
        public static int[] Distinct(int count, int pick, Random random)
        {
            if (pick > count)
                throw new ArgumentOutOfRangeException(nameof(pick), pick, $"{count} 종류에서 {pick} 개를 뽑을 수 없다");

            // 주머니. 처음에는 0, 1, 2, ... 가 차례로 들어 있다.
            var pool = new int[count];
            for (int i = 0; i < count; i++)
                pool[i] = i;

            var result = new int[pick];

            for (int i = 0; i < pick; i++)
            {
                // 아직 뽑지 않은 자리 가운데 하나를 고른다.
                int j = random.Next(i, count);

                // 고른 값을 앞으로 보내고, 그 자리에 아직 안 쓴 값을 넣는다.
                (pool[i], pool[j]) = (pool[j], pool[i]);

                result[i] = pool[i];
            }

            return result;
        }
    }
}
