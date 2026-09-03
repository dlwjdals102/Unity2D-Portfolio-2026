using System;
using UnityEngine;

namespace JM2D.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Health : MonoBehaviour, IDamageable
    {
        /// 체력이 0 이하가 되면 한 번 발생한다.
        public event Action OnDied;

        /// 체력이 바뀔 때 발생한다. (현재, 최대)
        public event Action<int, int> OnHealthChanged;

        [SerializeField] private int _maxHealth = 3;
        [SerializeField] private float _invulnerableTime = 0.5f;
        [SerializeField] private float _blinkInterval = 0.08f;

        private SpriteRenderer _renderer;

        private float _invulnerableLeft;
        private int _current;

        public bool IsDead => _current <= 0;

        /// 대시처럼 밖에서 거는 무적. Health 는 그 이유를 알지 못한다.
        public bool IsExternallyInvulnerable { get; set; }
        public bool IsInvulnerable => _invulnerableLeft > 0f || IsExternallyInvulnerable;
        public int Current => _current;
        public int Max => _maxHealth;

        private void Awake()
        {
            _current = _maxHealth;
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_invulnerableLeft > 0f)
            {
                _invulnerableLeft -= Time.deltaTime;
                _renderer.enabled = (int)(_invulnerableLeft / _blinkInterval) % 2 == 0;
            }
            else if (!_renderer.enabled)
                _renderer.enabled = true;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            if (IsInvulnerable) return;

            _current -= amount;
            OnHealthChanged?.Invoke(_current, _maxHealth);

            if (IsDead)
                OnDied?.Invoke();
            else
                _invulnerableLeft = _invulnerableTime;
        }

        /// 최대 체력을 밖에서 정한다. 적처럼 데이터에서 값이 오는 경우에 쓴다.
        /// 이 메서드를 부르면 인스펙터의 Max Health 는 무시된다.
        /// 초기화 전용이다. 게임 도중에 부르면 체력이 가득 차고 UI 갱신도 되지 않는다
        public void SetMaxHealth(int max)
        {
            _maxHealth = max;
            _current = _maxHealth;
        }
    }
}
