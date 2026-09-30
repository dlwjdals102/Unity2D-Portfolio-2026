using System.Collections.Generic;
using JM2D.Combat;
using JM2D.Core;
using JM2D.Data;
using JM2D.Logic.Common;
using UnityEngine;

namespace JM2D.Enemy
{
    /// 보스. 방에 들어서면 곧 교전으로 들어가 거기서 나오지 않고, 그 안에서 패턴을 돌린다.
    /// 패턴이 넷이어도 단계는 예고, 발동, 회복 셋이다. 발동에서 무엇을 하는지만 패턴마다 다르다.
    public class Boss : EnemyBase, IProjectileShooter
    {
        /// 쓸 수 있는 패턴.
        private enum Pattern { Sweep, Dash, Fan, Summon }

        /// 패턴 하나가 지나가는 단계. 어느 패턴이든 이 셋을 지난다.
        private enum Step { Windup, Act, Recover }

        /// 순환 순서. 계단 3과 5에서 여기에 패턴을 더한다.
        private static readonly Pattern[] Order = { Pattern.Sweep, Pattern.Dash, Pattern.Fan, Pattern.Summon };

        [SerializeField] private BossData _data;

        private Pattern _pattern;
        private Step _step;
        private float _stepTimeLeft;

        /// 순환에서 지금 어디까지 왔나.
        private int _orderIndex;

        /// 지금 패턴의 단계 시간과 색. 패턴이 정해질 때 한 번 꺼내 둔다.
        private PatternSteps _steps;

        /// 돌진이 예고를 시작할 때 정한 방향. 달리는 동안 바뀌지 않는다.
        private Vector2 _dashDirection;

        /// 이번 돌진이 이미 맞혔는가. 한 번만 때린다.
        private bool _hasHitThisDash;

        /// 지금 페이즈가 쓰는 패턴 묶음. 페이즈가 바뀌면 통째로 갈아 끼운다.
        private PhasePatterns _current;

        /// 2페이즈로 이미 넘어갔는가. 한 방향이라 되돌아가지 않는다.
        private bool _isPhase2;

        /// 페이즈를 넘어가는 데 남은 시간. 0보다 크면 패턴이 돌지 않는다.
        private float _changeTimeLeft;

        /// 씬의 적 투사체 풀. 만드는 쪽이 넣어 준다.
        private ProjectilePool _projectiles;

        /// 이 보스가 불러낸 것들. 죽은 것은 가짜 null 로 남아 셀 때 걸러낸다.
        private readonly List<Health> _summoned = new List<Health>();

        protected override EnemyData Data => _data;

        /// 같은 방에 있으면 그것으로 교전이다. 감지 범위를 그대로 써서 두 값이 어긋날 수 없다.
        protected override float EngageRange => _data.DetectRange;

        /// 체력 바가 경계선을 그을 자리를 묻는다. 같은 값을 두 곳에 적지 않으려고 연다.
        public float PhaseChangeAt => _data.PhaseChangeAt;

        /// EnemyBase 의 Awake 가 체력을 채운 뒤에 구독한다.
        protected override void Awake()
        {
            base.Awake();

            _current = _data.Phase1;

            // EnemyBase 의 OnEnable 이 private 이라, 여기서 같은 이름을 선언하면 부모 것이 가려질 수 있다.
            // 가려지면 부모가 죽음을 구독하지 못해 보스가 죽지 않는다.
            // 그 답에 기대는 대신 이름이 겹치지 않는 자리를 쓴다.
            Health.OnHealthChanged += OnHealthChanged;
            Health.OnDied += OnBossDied;
        }

        private void OnDestroy()
        {
            Health.OnHealthChanged -= OnHealthChanged;
            Health.OnDied -= OnBossDied;
        }

        /// 판 도중에 태어나므로 프리팹이 씬의 풀을 가리킬 수 없다. 만드는 쪽이 넣어 준다.
        /// 넣지 않으면 쏘는 순간 터진다. 조용히 안 쏘는 것보다 낫다.
        public void SetProjectilePool(ProjectilePool pool)
        {
            _projectiles = pool;
        }

        protected override void OnEngage()
        {
            SetColor(_current.BodyColor);

            // 첫 박자는 쫓기로 시작한다. _steps 가 아직 없지만 회복은 그것을 쓰지 않는다.
            _step = Step.Recover;
            _stepTimeLeft = _data.EntryDelay;
            _orderIndex = Order.Length - 1;
        }

        protected override void TickEngaged(float distance)
        {
            // 넘어가는 중에는 패턴이 돌지 않는다. 하던 패턴도 그 자리에서 끊긴다.
            if (_changeTimeLeft > 0f) { TickPhaseChange(); return; }

            switch (_step)
            {
                case Step.Windup:
                    Stop();
                    if (TimeUp()) ChangeStep(Step.Act);
                    break;

                case Step.Act:
                    TickAct();
                    if (TimeUp()) ChangeStep(Step.Recover);
                    break;

                case Step.Recover:
                    MoveTowardTarget(_data.MoveSpeed);
                    if (TimeUp()) StartPattern(NextPattern(distance));
                    break;
            }
        }

        /// 차례를 한 칸 넘긴다. 지금 거리에서 쓸 수 없는 패턴은 건너뛴다.
        /// 한 바퀴를 다 돌아도 쓸 것이 없으면 마지막으로 본 것을 그냥 쓴다.
        private Pattern NextPattern(float distance)
        {
            for (int i = 0; i < Order.Length; i++)
            {
                _orderIndex = (_orderIndex + 1) % Order.Length;

                if (CanUse(Order[_orderIndex], distance)) break;
            }

            return Order[_orderIndex];
        }

        /// 지금 거리에서 쓸 만한 패턴인가.
        private bool CanUse(Pattern pattern, float distance)
        {
            switch (pattern)
            {
                case Pattern.Sweep: return distance <= _current.Sweep.Range;
                case Pattern.Dash: return distance >= _current.Dash.MinRange && distance <= _current.Dash.MaxRange;
                case Pattern.Fan: return distance >= _current.Fan.MinRange;
                case Pattern.Summon: return _current.Summon.Prefab != null && CountAliveSummons() < _current.Summon.MaxAlive;
                default: return true;
            }
        }

        /// 패턴을 정하고 그 패턴의 값을 꺼내 둔다. 패턴을 더할 때 고치는 곳은 여기다.
        private void StartPattern(Pattern pattern)
        {
            _pattern = pattern;

            switch (pattern)
            {
                case Pattern.Sweep: _steps = _current.Sweep.Steps; break;
                case Pattern.Dash: _steps = _current.Dash.Steps; break;
                case Pattern.Fan: _steps = _current.Fan.Steps; break;
                case Pattern.Summon: _steps = _current.Summon.Steps; break;
            }

            ChangeStep(Step.Windup);
        }

        /// 단계가 바뀔 때 한 번만 할 일. 타이머를 채우고 색을 바꾼다.
        private void ChangeStep(Step next)
        {
            _step = next;

            switch (next)
            {
                case Step.Windup:
                    _stepTimeLeft = _steps.WindupTime;
                    SetColor(_steps.WindupColor);
                    BeginWindup();
                    break;

                case Step.Act:
                    _stepTimeLeft = _steps.ActTime;
                    SetColor(_steps.ActColor);
                    BeginAct();
                    break;

                case Step.Recover:
                    _stepTimeLeft = _steps.RecoverTime;
                    SetColor(_current.BodyColor);
                    break;
            }
        }

        /// 예고에 들어가는 순간 한 번.
        private void BeginWindup()
        {
            // 돌진은 여기서 방향을 정한다. 예고 동안 플레이어가 빠져나갈 수 있어야 한다.
            if (_pattern == Pattern.Dash) _dashDirection = DirectionToTarget();
        }

        /// 발동에 들어가는 순간 한 번.
        private void BeginAct()
        {
            switch (_pattern)
            {
                case Pattern.Sweep:
                    HitAround(_current.Sweep.HitRadius, _current.Sweep.Damage);
                    break;

                case Pattern.Dash:
                    _hasHitThisDash = false;
                    break;

                case Pattern.Fan:
                    ShootFan();
                    break;

                case Pattern.Summon:
                    SpawnSummons();
                    break;
            }
        }

        /// 플레이어 쪽을 가운데 두고 여러 발을 한 번에 뿌린다.
        /// 몇 도씩 벌릴지는 Logic 이 세고, 그 각도만큼 방향을 돌리는 것은 여기서 한다.
        private void ShootFan()
        {
            FanPattern fan = _current.Fan;
            Vector2 center = DirectionToTarget();

            for (int i = 0; i < fan.ShotCount; i++)
            {
                float angle = FanSpread.AngleOffset(i, fan.ShotCount, fan.SpreadDegrees);
                Vector2 direction = Quaternion.Euler(0f, 0f, angle) * center;

                _projectiles.Shoot(transform.position, direction, fan.Damage);
            }
        }

        /// 발동이 여러 스텝에 걸치는 패턴은 돌진뿐이다. 나머지는 들어가는 순간 끝났다.
        private void TickAct()
        {
            if (_pattern != Pattern.Dash)
            {
                Stop();
                return;
            }

            Move(_dashDirection, _current.Dash.Speed);

            if (!_hasHitThisDash && HitAround(_current.Dash.ContactRadius, _current.Dash.Damage) > 0)
                _hasHitThisDash = true;
        }

        /// 체력이 바뀔 때마다 불린다. 절반 아래로 처음 내려가는 순간 2페이즈로 넘어간다.
        private void OnHealthChanged(int current, int max)
        {
            if (_isPhase2) return;

            // 나눗셈을 곱셈으로 뒤집었다. current / max 는 둘 다 int 라 정수 나눗셈이 된다.
            if (current > max * _data.PhaseChangeAt) return;

            _isPhase2 = true;
            _current = _data.Phase2;
            _changeTimeLeft = _data.PhaseChangeTime;

            Stop();
            SetColor(_current.BodyColor);
            Health.IsExternallyInvulnerable = true;
        }

        /// 페이즈를 넘어가는 동안. 그 자리에 멈춰 있는다.
        private void TickPhaseChange()
        {
            Stop();

            _changeTimeLeft -= Time.fixedDeltaTime;

            if (_changeTimeLeft > 0f) return;

            Health.IsExternallyInvulnerable = false;

            // 다음 스텝에 바로 다음 패턴을 고르도록 회복으로 들어간다.
            _step = Step.Recover;
            _stepTimeLeft = 0f;
        }

        /// 보스 둘레에 잡몹을 고르게 놓는다. 분열 적이 갈라질 때와 같은 계산을 쓴다.
        private void SpawnSummons()
        {
            SummonPattern summon = _current.Summon;

            for (int i = 0; i < summon.Count; i++)
            {
                CircleSpread.Offset(i, summon.Count, summon.SpreadRadius, out float x, out float y);
                Vector2 position = SummonSpot(new Vector2(x, y));

                EnemyBase child = Instantiate(summon.Prefab, position, Quaternion.identity);
                child.SetTarget(Target);
                child.SetPathField(PathField);

                _summoned.Add(child.GetComponent<Health>());
            }
        }

        /// 놓을 자리. 막혔으면 반대쪽을 보고, 그것도 막혔으면 보스 자리에 놓는다.
        /// 보스가 벽에 붙어 있다는 것은 반대쪽이 비어 있다는 뜻이라 한 번 뒤집는 것으로 거의 해결된다.
        /// 보스가 서 있는 칸은 지나갈 수 있는 칸이라 마지막 자리로 안전하다.
        private Vector2 SummonSpot(Vector2 offset)
        {
            Vector2 center = transform.position;

            if (PathField == null) return center + offset;
            if (PathField.CanStand(center + offset)) return center + offset;
            if (PathField.CanStand(center - offset)) return center - offset;

            return center;
        }

        /// 살아 있는 소환물 수.
        private int CountAliveSummons()
        {
            int alive = 0;

            foreach (Health health in _summoned)
                if (health != null) alive++;

            return alive;
        }

        /// 보스가 죽으면 불러낸 것도 함께 죽는다.
        /// 그러지 않으면 적 수가 0이 되지 않아 방이 열리지 않는다.
        private void OnBossDied()
        {
            foreach (Health health in _summoned)
            {
                if (health == null) continue;
                health.Kill();
            }

            _summoned.Clear();
        }

        /// 타이머를 한 스텝만큼 줄이고, 다 됐는지 알려 준다.
        private bool TimeUp()
        {
            _stepTimeLeft -= Time.fixedDeltaTime;
            return _stepTimeLeft <= 0f;
        }
    }
}
