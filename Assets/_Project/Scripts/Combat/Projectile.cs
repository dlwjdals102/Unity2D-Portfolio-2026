using UnityEngine;
using JM2D.Core;

namespace JM2D.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private int _damage = 1;

        private Rigidbody2D _rb;
        private Pool<Projectile> _pool;

        private float _lifeLeft;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
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

        /// 발사한 쪽이 방향을 정해서 알려준다.
        public void Launch(Vector2 direction)
        {
            _rb.linearVelocity = direction * _speed;
            _lifeLeft = _lifetime;
        }

        /// 어느 풀에서 나왔는지 알려준다. 꺼낼 때마다 불러도 무해하다.
        public void Bind(Pool<Projectile> pool)
        {
            _pool = pool;
        }
    }
}
