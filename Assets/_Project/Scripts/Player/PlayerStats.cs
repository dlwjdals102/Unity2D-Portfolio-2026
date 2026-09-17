using JM2D.Combat;
using JM2D.Logic.Stats;
using System.Collections.Generic;
using UnityEngine;

namespace JM2D.Player
{
    /// 플레이어의 스탯을 모아 갖는다.
    /// 공격력과 공격 속도는 기본값이 0 이고 무기가 얹는다. 아이템과 버프는 모디파이어로 얹힌다.
    [RequireComponent(typeof(Health))]
    public class PlayerStats : MonoBehaviour
    {
        [Header("기본값")]
        [SerializeField] private float _maxHealth = 3f;
        [SerializeField] private float _moveSpeed = 6f;

        private Health _health;
        private readonly Dictionary<StatType, Stat> _stats = new Dictionary<StatType, Stat>();

        public Stat Get(StatType type) => _stats[type];
        public Stat MaxHealth => _stats[StatType.MaxHealth];
        public Stat MoveSpeed => _stats[StatType.MoveSpeed];
        public Stat AttackDamage => _stats[StatType.AttackDamage];
        public Stat AttackSpeed => _stats[StatType.AttackSpeed];

        private void Awake()
        {
            _health = GetComponent<Health>();

            _stats.Add(StatType.MaxHealth, new Stat(_maxHealth));
            _stats.Add(StatType.MoveSpeed, new Stat(_moveSpeed));
            _stats.Add(StatType.AttackDamage, new Stat(0f));
            _stats.Add(StatType.AttackSpeed, new Stat(0f));

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
