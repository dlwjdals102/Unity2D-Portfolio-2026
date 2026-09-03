using JM2D.Combat;
using JM2D.Logic;
using UnityEngine;

namespace JM2D.Player
{
    /// 플레이어의 스탯을 모아 갖는다.
    /// 기본값은 인스펙터에 있고, 아이템과 버프는 모디파이어로 얹힌다.
    [RequireComponent(typeof(Health))]
    public class PlayerStats : MonoBehaviour
    {
        [Header("기본값")]
        [SerializeField] private float _maxHealth = 3f;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _attackDamage = 1f;

        [Min(0.1f)]
        [Tooltip("초당 발사 수. 연사 간격은 이 값의 역수다.")]
        [SerializeField] private float _attackSpeed = 6.67f;

        private Health _health;

        public Stat MaxHealth { get; private set; }
        public Stat MoveSpeed { get; private set; }
        public Stat AttackDamage { get; private set; }
        public Stat AttackSpeed { get; private set; }

        private void Awake()
        {
            _health = GetComponent<Health>();

            MaxHealth = new Stat(_maxHealth);
            MoveSpeed = new Stat(_moveSpeed);
            AttackDamage = new Stat(_attackDamage);
            AttackSpeed = new Stat(_attackSpeed);

            _health.InitializeMaxHealth(MaxHealth.IntValue);
        }

        private void OnEnable()
        {
            MaxHealth.OnChanged += OnMaxHealthChanged;
        }

        private void OnDisable()
        {
            MaxHealth.OnChanged -= OnMaxHealthChanged;
        }

        private void OnMaxHealthChanged()
        {
            _health.ChangeMaxHealth(MaxHealth.IntValue);
        }
    }
}
