using JM2D.Combat;
using JM2D.Core;
using JM2D.Data;
using JM2D.Logic.Common;
using UnityEngine;

namespace JM2D.Enemy
{
    /// 원거리 적. 가까워지면 멈춰서 조준하고, 조준이 끝나는 순간의 플레이어 쪽으로 한 발 쏜다.
    /// 플레이어가 너무 가까우면 물러나면서 쏜다.
    public class RangedEnemy : EnemyBase, IProjectileShooter
    {
        private enum State { Aim, Cooldown }

        [SerializeField] private RangedEnemyData _data;

        [SerializeField] private ProjectilePool _projectiles;

        private State _state;
        private float _stateTimeLeft;

        protected override EnemyData Data => _data;
        protected override float EngageRange => _data.ShootStartRange;

        /// EnemyBase 의 OnEnable 이 private 이라 같은 이름을 쓰면 부모 것이 가려진다.
        /// Boss 가 체력 이벤트를 다루는 것과 같게 Awake 와 OnDestroy 로 짝을 맞춘다.
        protected override void Awake()
        {
            base.Awake();
            CharacterAnimator.AttackFrameReached += FireShot;
        }

        private void OnDestroy()
        {
            CharacterAnimator.AttackFrameReached -= FireShot;
        }

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

            // 조준 중에는 타이머를 보지 않는다. 쏘는 시점은 공격 동작이 정한다.
            if (_state == State.Cooldown && TimeUp())
                ChangeState(State.Aim);

            // 사격 멈춤. 이것도 상태와 상관없이 매 스텝 본다.
            if (!Hysteresis.IsInside(true, distance, _data.ShootStartRange, _data.ShootStopRange))
                ReturnToChase();
        }

        /// 폭탄이 손을 떠나는 프레임에 불린다. 이름을 OnAttackFrame 으로 두면
        /// CharacterAnimator 의 같은 이름과 겹쳐 애니메이션 이벤트가 둘 다 부른다.
        private void FireShot()
        {
            _projectiles.Shoot(transform.position, DirectionToTarget(), _data.ShotDamage);
            ChangeState(State.Cooldown);
        }

        /// 타이머를 한 스텝만큼 줄이고, 다 됐는지 알려 준다.
        private bool TimeUp()
        {
            _stateTimeLeft -= Time.fixedDeltaTime;
            return _stateTimeLeft <= 0f;
        }

        /// 상태가 바뀔 때 한 번만 할 일. 타이머를 채우고 예고 표식을 켜고 끈다.
        private void ChangeState(State next)
        {
            _state = next;

            switch (next)
            {
                case State.Aim:
                    ShowWarning(true);

                    // 동작 길이를 조준 시간에 맞춘다. 클립이 1초라 역수가 곧 배속이다.
                    CharacterAnimator.PlayAttack(1f / _data.AimTime);
                    break;

                case State.Cooldown:
                    _stateTimeLeft = _data.ShootCooldown;
                    ShowWarning(false);
                    break;
            }
        }
    }
}
