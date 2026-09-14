using JM2D.Combat;
using JM2D.Logic;
using UnityEngine;

namespace JM2D.Enemy
{
    /// 죽으면 그 자리에서 작은 적 여럿으로 갈라진다.
    /// 적의 행동과 따로 붙는 부품이다. 살아 있는 동안에는 아무것도 하지 않는다.
    [RequireComponent(typeof(Health), typeof(EnemyBase))]
    public class SplitOnDeath : MonoBehaviour
    {
        [Header("갈라지기")]
        [Tooltip("갈라져 나올 적. 이 적에는 SplitOnDeath 를 붙이지 않는다")]
        [SerializeField] private EnemyBase _childPrefab;

        [Min(1)]
        [SerializeField] private int _count = 3;

        [Tooltip("큰 적의 반폭보다 작아야 작은 적의 중심이 벽을 넘지 않는다")]
        [SerializeField] private float _spreadRadius = 0.5f;

        private Health _health;
        private EnemyBase _self;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _self = GetComponent<EnemyBase>();
        }

        private void OnEnable()
        {
            _health.OnDied += Split;
        }

        private void OnDisable()
        {
            _health.OnDied -= Split;
        }

        private void Split()
        {
            for (int i = 0; i < _count; i++)
            {
                CircleSpread.Offset(i, _count, _spreadRadius, out float x, out float y);
                Vector2 position = (Vector2)transform.position + new Vector2(x, y);

                EnemyBase child = Instantiate(_childPrefab, position, Quaternion.identity);
                child.SetTarget(_self.Target);
            }
        }
    }
}
