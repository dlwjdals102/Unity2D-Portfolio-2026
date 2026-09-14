using JM2D.Core;
using UnityEngine;

namespace JM2D.Combat
{
    /// 투사체 풀을 씬에 둔다. 프리팹 하나당 오브젝트 하나다.
    /// 쓰는 쪽이 죽어도 풀은 남아야 할 때 쓴다. 적이 그렇다.
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private Projectile _prefab;

        private Pool<Projectile> _pool;

        private void Awake()
        {
            _pool = new Pool<Projectile>(_prefab, transform);
        }

        /// 한 발 쏜다. 쏘는 쪽은 풀이 있는지 모른다.
        public void Shoot(Vector2 position, Vector2 direction, int damage)
        {
            Projectile p = _pool.Get();
            p.transform.position = position;
            p.Bind(_pool);
            p.Launch(direction, damage);
        }
    }
}
