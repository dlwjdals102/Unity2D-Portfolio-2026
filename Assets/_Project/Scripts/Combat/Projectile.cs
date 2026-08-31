using UnityEngine;

namespace JM2D.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _lifetime = 3f;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        /// 발사한 쪽이 방향을 정해서 알려준다.
        public void Launch(Vector2 direction)
        {
            _rb.linearVelocity = direction * _speed;
            Destroy(gameObject, _lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Destroy(gameObject);
        }
    }
}
