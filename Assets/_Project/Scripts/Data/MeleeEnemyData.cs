using UnityEngine;
using UnityEngine.Serialization;

namespace JM2D.Data
{
    /// 추적 근접 적. 붙어서 쿨다운마다 때린다.
    [CreateAssetMenu(fileName = "EnemyData_", menuName = "JM2D/Enemy Data/Melee")]
    public class MeleeEnemyData : EnemyData
    {
        [Header("공격")]
        [Tooltip("추적 중 이 안으로 들어오면 공격을 시작한다. 공격이 닿는 거리가 아니다")]
        [FormerlySerializedAs("_attackRange")]
        [SerializeField] private float _attackStartRange = 1.2f;

        [Tooltip("공격 중 이 밖으로 나가면 멈추고 다시 쫓는다. 시작 거리보다 넓어야 한다")]
        [FormerlySerializedAs("_attackExitRange")]
        [SerializeField] private float _attackStopRange = 1.6f;

        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private int _attackDamage = 1;

        public float AttackStartRange => _attackStartRange;
        public float AttackStopRange => _attackStopRange;
        public float AttackCooldown => _attackCooldown;
        public int AttackDamage => _attackDamage;

        protected override void OnValidate()
        {
            base.OnValidate();

            if (_attackStopRange < _attackStartRange)
                Debug.LogWarning($"{name}: 공격 멈춤 거리({_attackStopRange})가 공격 시작 거리({_attackStartRange})보다 작다", this);
        }
    }
}
