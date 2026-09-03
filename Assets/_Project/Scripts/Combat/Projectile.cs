using UnityEngine;
using JM2D.Core;
using JM2D.Data;

namespace JM2D.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private ProjectileData _data;

        private Rigidbody2D _rb;
        private Pool<Projectile> _pool;

        private float _speed;
        private float _lifetime;
        private int _damage;
        private float _lifeLeft;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _speed = _data.Speed;
            _lifetime = _data.Lifetime;
        }

        private void Update()
        {
            if (_lifeLeft <= 0f) return;

            _lifeLeft -= Time.deltaTime;

            if (_lifeLeft <= 0f)
                _pool.Release(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(_damage);

            _pool.Release(this);
        }

        /// 발사한 쪽이 방향과 피해량을 정해서 알려준다.
        public void Launch(Vector2 direction, int damage)
        {
            _rb.linearVelocity = direction * _speed;
            _lifeLeft = _lifetime;
            _damage = damage;
        }

        /// 어느 풀에서 나왔는지 알려준다. 꺼낼 때마다 불러도 무해하다.
        public void Bind(Pool<Projectile> pool)
        {
            _pool = pool;
        }
    }
}
