using JM2D.Combat;
using JM2D.Core;
using JM2D.Data;
using UnityEngine;

namespace JM2D.Weapons
{
    /// 활. 조준 방향으로 투사체 하나를 쏜다.
    public class BowWeapon : Weapon
    {
        [SerializeField] private BowWeaponData _data;

        [Header("발사")]
        [SerializeField] private Transform _poolParent;

        private Pool<Projectile> _pool;

        public override WeaponData Data => _data;

        private void Awake()
        {
            _pool = new Pool<Projectile>(_data.ProjectilePrefab, _poolParent);
        }

        public override void Attack(Vector2 origin, Vector2 direction, int damage)
        {
            Projectile p = _pool.Get();
            p.transform.position = origin;
            p.Bind(_pool);
            p.Launch(direction, damage);
        }
    }
}
