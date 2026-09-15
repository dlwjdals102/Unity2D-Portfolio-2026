using JM2D.Combat;
using JM2D.Data;
using UnityEngine;

namespace JM2D.Weapons
{
    /// 검과 창. 공격하는 순간 조준 방향 앞 사각형 안의 적을 모두 때린다. 두 무기는 데이터만 다르다.
    public class MeleeWeapon : Weapon
    {
        [SerializeField] private MeleeWeaponData _data;

        [SerializeField] private RangeView _rangeView;

        private int _enemyMask;

        public override WeaponData Data => _data;

        private void Awake()
        {
            _enemyMask = LayerMask.GetMask("Enemy");
        }

        public override void Attack(Vector2 origin, Vector2 direction, int damage)
        {
            Vector2 center = origin + direction * (_data.Length / 2f);
            Vector2 size = new Vector2(_data.Length, _data.Width);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            AreaHit.Box(center, size, angle, _enemyMask, damage);
            _rangeView.Show(center, size, angle, _data.ShowTime);
        }
    }
}
