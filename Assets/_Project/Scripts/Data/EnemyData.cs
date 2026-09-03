using UnityEngine;

namespace JM2D.Data
{
    /// 적 한 종류의 수치를 담는다.
    /// 행동은 여기 없다. EnemyController 가 이 값을 읽어 움직인다.
    [CreateAssetMenu(fileName = "EnemyData_", menuName = "JM2D/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("체력")]
        [SerializeField] private int _maxHealth = 3;

        [Header("이동")]
        [SerializeField] private float _moveSpeed = 3f;

        [Header("감지")]
        [SerializeField] private float _detectRange = 6f;
        [SerializeField] private float _giveUpRange = 8f;

        [Header("공격")]
        [SerializeField] private float _attackRange = 1.2f;
        [SerializeField] private float _attackExitRange = 1.6f;
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private int _attackDamage = 1;

        public int MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float DetectRange => _detectRange;
        public float GiveUpRange => _giveUpRange;
        public float AttackRange => _attackRange;
        public float AttackExitRange => _attackExitRange;
        public float AttackCooldown => _attackCooldown;
        public int AttackDamage => _attackDamage;
    }
}
