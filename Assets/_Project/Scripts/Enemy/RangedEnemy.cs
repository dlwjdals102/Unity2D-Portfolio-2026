using JM2D.Combat;
using JM2D.Data;
using JM2D.Logic.Common;
using UnityEngine;

namespace JM2D.Enemy
{
    /// 원거리 적. 가까워지면 멈춰서 조준하고, 조준이 끝나는 순간의 플레이어 쪽으로 한 발 쏜다.
    /// 플레이어가 너무 가까우면 물러나면서 쏜다.
    public class RangedEnemy : EnemyBase
    {
        private enum State { Aim, Cooldown }

        [SerializeField] private RangedEnemyData _data;

        [SerializeField] private ProjectilePool _projectiles;

        private State _state;
        private float _stateTimeLeft;

        protected override EnemyData Data => _data;
        protected override float EngageRange => _data.ShootStartRange;

        /// 판 도중에 태어난 적에게 투사체 풀을 넣는다. 프리팹은 씬의 풀을 가리킬 수 없다.
        /// 넣지 않으면 쏘는 순간 터진다. 조용히 안 쏘는 것보다 낫다.
        public void SetProjectilePool(ProjectilePool pool)
        {
            _projectiles = pool;
        }

        protected override void OnEngage()
        {
            ChangeState(State.Aim);
        }

        protected override void TickEngaged(float distance)
        {
            // 거리 유지. 조준 중이든 재장전 중이든 매 스텝 한다.
            if (distance < _data.RetreatRange)
                Move(-DirectionToTarget(), _data.RetreatSpeed);
            else
                Stop();

            switch (_state)
            {
                case State.Aim:
                    if (TimeUp())
                    {
                        _projectiles.Shoot(transform.position, DirectionToTarget(), _data.ShotDamage);
                        ChangeState(State.Cooldown);
                    }
                    break;

                case State.Cooldown:
                    if (TimeUp()) ChangeState(State.Aim);
                    break;
            }

            // 사격 멈춤. 이것도 상태와 상관없이 매 스텝 본다.
            if (!Hysteresis.IsInside(true, distance, _data.ShootStartRange, _data.ShootStopRange))
                ReturnToChase();
        }

        /// 타이머를 한 스텝만큼 줄이고, 다 됐는지 알려 준다.
        private bool TimeUp()
        {
            _stateTimeLeft -= Time.fixedDeltaTime;
            return _stateTimeLeft <= 0f;
        }

        /// 상태가 바뀔 때 한 번만 할 일. 타이머를 채우고 색을 바꾼다.
        private void ChangeState(State next)
        {
            _state = next;

            switch (next)
            {
                case State.Aim:
                    _stateTimeLeft = _data.AimTime;
                    SetColor(Color.cyan);
                    break;

                case State.Cooldown:
                    _stateTimeLeft = _data.ShootCooldown;
                    SetColor(Color.gray);
                    break;
            }
        }
    }
}
