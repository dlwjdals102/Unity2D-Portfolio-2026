using UnityEngine;

namespace JM2D.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        private enum State { Idle, Chase, Attack }

        [Header("이동")]
        [SerializeField] private float _moveSpeed = 3f;

        [Header("감지")]
        [SerializeField] private float _detectRange = 6f;
        [SerializeField] private float _giveUpRange = 8f;

        [Header("공격")]
        [SerializeField] private float _attackRange = 1.2f;
        [SerializeField] private float _attackExitRange = 1.6f;
        [SerializeField] private float _attackCooldown = 1f;

        [SerializeField] private Transform _target;
        private Rigidbody2D _rb;
        private SpriteRenderer _renderer;
        private State _state = State.Idle;

        private float _attackCooldownLeft;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();

            ChangeState(State.Idle);
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
                        Debug.Log("적 공격"); // 지금은 로그만
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
    }
}
