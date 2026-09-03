using JM2D.Combat;
using JM2D.Core;
using UnityEngine;

namespace JM2D.Debugging
{
    /// <summary>
    /// 오브젝트 풀링 전후를 비교하기 위한 부하 테스트 도구.
    /// 실제 게임에서는 쓰지 않으며 측정 전용 씬에만 배치한다.
    /// 초당 일정한 수를 사방으로 발사한다.
    /// 게임의 발사 경로와 같은 방식을 써야 비교가 성립하므로 풀을 거친다.
    /// </summary>
    public class ProjectileStressSpawner : MonoBehaviour
    {
        [SerializeField] private Projectile _prefab;
        [SerializeField] private Transform _poolParent;

        [Tooltip("초당 생성할 투사체 수. 측정할 때는 이 값을 고정한다.")]
        [SerializeField] private int _perSecond = 600;

        private Pool<Projectile> _pool;

        /// 초당 개수가 프레임당 개수로 나누어떨어지지 않으므로 소수부를 이월한다.
        private float _carry;

        private void Awake()
        {
            _pool = new Pool<Projectile>(_prefab, _poolParent);
        }

        private void Update()
        {
            _carry += _perSecond * Time.deltaTime;

            int count = Mathf.FloorToInt(_carry);
            _carry -= count;

            for (int i = 0; i < count; i++)
                SpawnOne();
        }

        private void SpawnOne()
        {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            Projectile projectile = _pool.Get();
            projectile.transform.position = transform.position;
            projectile.Bind(_pool);
            // 측정 씬에는 맞을 대상이 없다. 피해량은 결과에 영향을 주지 않는다.
            projectile.Launch(direction, 1);
        }
    }
}
