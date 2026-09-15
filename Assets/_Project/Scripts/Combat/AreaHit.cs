using UnityEngine;

namespace JM2D.Combat
{
    /// 공격하는 순간 범위 안에 몸이 걸린 상대 모두에게 피해를 준다.
    /// 플레이어의 검과 창, 적의 근접 공격이 함께 쓴다. 누구를 맞힐지는 레이어로만 가른다.
    public static class AreaHit
    {
        /// 한 번에 맞힐 수 있는 수. 넘으면 먼저 찾은 것만 맞는다.
        private const int MaxHits = 16;

        /// 결과를 담을 자리. 공격마다 새로 만들지 않고 다시 쓴다.
        private static readonly Collider2D[] _hits = new Collider2D[MaxHits];

        /// center 를 중심으로 size 크기이고 angleDegrees 만큼 돌린 사각형 안의 상대에게 피해를 준다. 피해를 준 수를 돌려준다.
        public static int Box(Vector2 center, Vector2 size, float angleDegrees, int layerMask, int damage)
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(layerMask);

            int count = Physics2D.OverlapBox(center, size, angleDegrees, filter, _hits);
            return DamageHits(count, damage);
        }

        /// center 를 중심으로 반경 radius 인 원 안의 상대에게 피해를 준다. 피해를 준 수를 돌려준다.
        public static int Circle(Vector2 center, float radius, int layerMask, int damage)
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(layerMask);

            int count = Physics2D.OverlapCircle(center, radius, filter, _hits);
            return DamageHits(count, damage);
        }

        /// _hits 의 앞 count 칸 중 피해를 받을 수 있는 것에 피해를 준다. 준 수를 돌려준다.
        private static int DamageHits(int count, int damage)
        {
            int damaged = 0;

            for (int i = 0; i < count; i++)
            {
                if (_hits[i].TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage);
                    damaged++;
                }
            }

            return damaged;
        }
    }
}
