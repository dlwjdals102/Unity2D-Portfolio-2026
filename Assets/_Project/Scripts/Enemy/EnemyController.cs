using JM2D.Combat;
using JM2D.Data;
using UnityEngine;

namespace JM2D.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        private enum State { Idle, Chase, Attack }

        [SerializeField] private EnemyData _data;

        [SerializeField] private Transform _target;
        private Rigidbody2D _rb;
        private SpriteRenderer _renderer;
        private Health _health;
        private IDamageable _targetDamageable;

        private float _moveSpeed;
        private float _detectRange;
        private float _giveUpRange;
        private float _attackRange;
        private float _attackExitRange;
        private float _attackCooldown;
        private int _attackDamage;

        private State _state = State.Idle;
        private float _attackCooldownLeft;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _health = GetComponent<Health>();
            _targetDamageable = _target.GetComponent<IDamageable>();

            _health.SetMaxHealth(_data.MaxHealth);

            _moveSpeed = _data.MoveSpeed;
            _detectRange = _data.DetectRange;
            _giveUpRange = _data.GiveUpRange;
            _attackRange = _data.AttackRange;
            _attackExitRange = _data.AttackExitRange;
            _attackCooldown = _data.AttackCooldown;
            _attackDamage = _data.AttackDamage;

            ChangeState(State.Idle);
        }

        private void OnEnable()
        {   
            _health.OnDied += OnDied;
        }

        private void OnDisable()
        {
            _health.OnDied -= OnDied;
        }

        private void FixedUpdate()
        {
            float distance = Vector2.Distance(transform.position, _target.position);

            switch (_state)
            {
                case State.Idle:
                    _rb.linearVelocity = Vector2.zero;
                    if (distance <= _detectRange)
                        ChangeState(State.Chase);
                    break;

                case State.Chase:
                    {
                        Vector2 dir = ((Vector2)_target.position - (Vector2)transform.position).normalized;
                        _rb.linearVelocity = dir * _moveSpeed;

                        if (distance <= _attackRange)
                            ChangeState(State.Attack);
                        else if (distance > _giveUpRange)
                            ChangeState(State.Idle);
                        break;
                    }

                case State.Attack:
                    _rb.linearVelocity = Vector2.zero;

                    if (_attackCooldownLeft > 0f)
                        _attackCooldownLeft -= Time.fixedDeltaTime;
                    else
                    {
                        _targetDamageable.TakeDamage(_attackDamage);
                        _attackCooldownLeft = _attackCooldown;
                    }

                    if (distance > _attackExitRange)
                        ChangeState(State.Chase);
                    break;

            }
        }

        private void ChangeState(State next)
        {
            _state = next;
            _renderer.color = next switch
            {
                State.Idle => Color.red,
                State.Chase => Color.blue,
                State.Attack => Color.green,
                _ => Color.white
            };
        }

        private void OnDied()
        {
            Destroy(gameObject);
        }
    }
}
