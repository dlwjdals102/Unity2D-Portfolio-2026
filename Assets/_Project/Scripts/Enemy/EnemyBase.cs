using JM2D.Combat;
using JM2D.Data;
using JM2D.Logic;
using UnityEngine;

namespace JM2D.Enemy
{
    /// 모든 적이 함께 갖는 것. 대기, 추적, 체력 연결, 죽음 처리.
    /// 가까워지면 교전으로 넘기고, 교전 중에 할 일은 자식이 정한다.
    /// 왜 이 모양인지는 docs/specs/enemy-dash.md 의 계단 1 결과에 있다.
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        private enum Phase { Idle, Chase, Engaged }

        [SerializeField] private Transform _target;

        private Rigidbody2D _rb;
        private SpriteRenderer _renderer;
        private Health _health;
        private IDamageable _targetDamageable;

        private Phase _phase = Phase.Idle;

        /// 자식은 자기 데이터를 제 타입으로 갖고, 부모에게는 공통 칸만 보여 준다.
        protected abstract EnemyData Data { get; }

        /// 추적 중 이 거리 안으로 들어오면 교전을 시작한다.
        protected abstract float EngageRange { get; }

        /// 자식이 Awake 를 쓰려면 override 하고 base.Awake() 를 먼저 부른다.
        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _health = GetComponent<Health>();
            _targetDamageable = _target.GetComponent<IDamageable>();

            _health.InitializeMaxHealth(Data.MaxHealth);

            ChangePhase(Phase.Idle);
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

            switch (_phase)
            {
                case Phase.Idle:
                    Stop();

                    if (Hysteresis.IsInside(false, distance, Data.DetectRange, Data.GiveUpRange))
                        ChangePhase(Phase.Chase);
                    break;

                case Phase.Chase:
                    MoveTowardTarget(Data.MoveSpeed);

                    if (distance <= EngageRange)
                    {
                        ChangePhase(Phase.Engaged);
                        OnEngage();
                    }
                    else if (!Hysteresis.IsInside(true, distance, Data.DetectRange, Data.GiveUpRange))
                        ChangePhase(Phase.Idle);
                    break;

                case Phase.Engaged:
                    TickEngaged(distance);
                    break;
            }
        }

        /// 교전이 시작될 때 한 번 불린다.
        protected abstract void OnEngage();

        /// 교전 중 물리 스텝마다 불린다. 끝나면 ReturnToChase 를 부른다.
        protected abstract void TickEngaged(float distance);

        // 자식이 쓰는 도구. 부모의 필드를 열어 주지 않고 할 일만 연다.

        protected void ReturnToChase()
        {
            ChangePhase(Phase.Chase);
        }

        protected void Stop()
        {
            _rb.linearVelocity = Vector2.zero;
        }

        protected void MoveTowardTarget(float speed)
        {
            Move(DirectionToTarget(), speed);
        }

        /// 지금 플레이어가 있는 쪽. 크기는 1.
        protected Vector2 DirectionToTarget()
        {
            return ((Vector2)_target.position - (Vector2)transform.position).normalized;
        }

        /// 정해 둔 방향으로 움직인다. 돌진처럼 도중에 방향을 바꾸지 않을 때 쓴다.
        protected void Move(Vector2 direction, float speed)
        {
            _rb.linearVelocity = direction * speed;
        }

        protected void HitTarget(int damage)
        {
            _targetDamageable.TakeDamage(damage);
        }

        protected void SetColor(Color color)
        {
            _renderer.color = color;
        }

        private void ChangePhase(Phase next)
        {
            _phase = next;

            if (next == Phase.Idle) SetColor(Color.red);
            else if (next == Phase.Chase) SetColor(Color.blue);
        }

        private void OnDied()
        {
            Destroy(gameObject);
        }
    }
}
