using System;
using JM2D.Combat;
using JM2D.Data;
using JM2D.Logic.Common;
using UnityEngine;
using JM2D.Core;

namespace JM2D.Enemy
{
    /// 모든 적이 함께 갖는 것. 대기, 추적, 체력 연결, 죽음 처리.
    /// 가까워지면 교전으로 넘기고, 교전 중에 할 일은 자식이 정한다.
    /// 왜 이 모양인지는 docs/specs/enemy-dash.md 의 계단 1 결과에 있다.
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        private enum Phase { Idle, Chase, Engaged }

        /// 적이 태어나면 알린다. 적 수를 세는 쪽이 듣는다. 적은 누가 듣는지 모른다.
        public static event Action<Health> Spawned;

        [SerializeField] private Transform _target;

        private Rigidbody2D _rb;
        private SpriteRenderer _renderer;
        private Health _health;

        private Phase _phase = Phase.Idle;

        /// 다음 자리에 이만큼 가까워지면 닿은 것으로 본다.
        private const float WaypointReach = 0.2f;

        /// 이 시간이 지나면 다음 자리를 다시 고른다. 플레이어가 움직이면 길이 바뀐다.
        private const float WaypointLifetime = 0.4f;

        /// 주변 적을 담을 자리. 매 스텝 새로 만들지 않고 다시 쓴다.
        private readonly Collider2D[] _neighbors = new Collider2D[8];
        private ContactFilter2D _neighborFilter;

        /// 길 안내. 없으면 플레이어 쪽으로 곧장 간다.
        private IPathField _pathField;

        /// 지금 향하고 있는 자리. 닿을 때까지 바꾸지 않는다.
        private Vector2 _waypoint;
        private bool _hasWaypoint;

        /// 이 자리를 붙잡은 지 얼마나 됐나. 플레이어가 움직이면 길도 바뀌므로 오래 붙잡지 않는다.
        private float _waypointAge;

        /// 자식은 자기 데이터를 제 타입으로 갖고, 부모에게는 공통 칸만 보여 준다.
        protected abstract EnemyData Data { get; }

        /// 추적 중 이 거리 안으로 들어오면 교전을 시작한다.
        protected abstract float EngageRange { get; }

        /// 갈라지는 부품이 작은 적에게 대상을 물려줄 때 읽는다.
        public Transform Target => _target;
        public IPathField PathField => _pathField;

        /// 자식이 Awake 를 쓰려면 override 하고 base.Awake() 를 먼저 부른다.
        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _health = GetComponent<Health>();

            _health.InitializeMaxHealth(Data.MaxHealth);

            _neighborFilter = new ContactFilter2D();
            _neighborFilter.SetLayerMask(1 << gameObject.layer);

            ChangePhase(Phase.Idle);

            Spawned?.Invoke(_health);
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

        /// 런타임에 태어난 적에게 추적 대상을 넣는다.
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        /// 판 도중에 태어난 적에게 길 안내를 넣는다.
        public void SetPathField(IPathField field)
        {
            _pathField = field;
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
            Vector2 here = transform.position;
            Vector2 toTarget = DirectionToTarget();

            // 길 안내가 있으면 그쪽을 따른다. 없거나 길을 모르면 직선 그대로다.
            if (_pathField != null && TryGetPathDirection(here, out Vector2 path))
                toTarget = path;

            Vector2 direction = toTarget + SeparationPush() * Data.SeparationStrength;

            Move(direction.normalized, speed);
        }

        /// 길 안내가 준 자리로 갈 방향.
        /// 플레이어가 보이면 그 자리로 곧장 가고, 아니면 다음 칸을 붙잡고 간다.
        /// 칸을 붙잡는 이유는, 매 프레임 고르면 칸 경계에서 선택이 뒤집혀 제자리에서 떨기 때문이다.
        private bool TryGetPathDirection(Vector2 here, out Vector2 direction)
        {
            direction = Vector2.zero;

            _waypointAge += Time.fixedDeltaTime;

            bool arrived = _hasWaypoint && Vector2.Distance(here, _waypoint) < WaypointReach;

            if (!_hasWaypoint || arrived || _waypointAge > WaypointLifetime)
            {
                _hasWaypoint = _pathField.TryGetNextPoint(here, out _waypoint);
                _waypointAge = 0f;
            }

            if (!_hasWaypoint) return false;

            Vector2 toWaypoint = _waypoint - here;

            if (toWaypoint.sqrMagnitude < 0.0001f) return false;

            direction = toWaypoint.normalized;
            return true;
        }

        /// 가까운 적에게서 멀어지는 방향. 가까울수록 길고, 이웃이 없으면 0 이다.
        private Vector2 SeparationPush()
        {
            Vector2 self = transform.position;
            float radius = Data.SeparationRadius;
            int count = Physics2D.OverlapCircle(self, radius, _neighborFilter, _neighbors);

            Vector2 push = Vector2.zero;
            for (int i = 0; i < count; i++)
            {
                Collider2D other = _neighbors[i];
                if (other.attachedRigidbody == _rb) continue;

                Vector2 away = self - (Vector2)other.transform.position;
                float distance = away.magnitude;

                if (distance < 0.0001f) continue;
                if (distance >= radius) continue;

                push += away.normalized * (1f - distance / radius);
            }
            return push;
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

        /// 자기 둘레 반경 안에 몸이 걸린 대상에게 피해를 준다. 피해를 준 수를 돌려준다.
        /// 대상이 있는 레이어를 찾으므로 적은 서로를 때리지 않는다.
        protected int HitAround(float radius, int damage)
        {
            return AreaHit.Circle(transform.position, radius, 1 << _target.gameObject.layer, damage);
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
