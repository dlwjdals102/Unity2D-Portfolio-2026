using JM2D.Data;
using UnityEngine;

namespace JM2D.Enemy
{
    /// 돌진 적. 가까워지면 멈춰서 예고하고, 예고가 시작될 때의 방향으로 곧게 돌진한 뒤 잠시 쉰다.
    public class DashEnemy : EnemyBase
    {
        private enum State { Windup, Dash, Recover }

        [SerializeField] private DashEnemyData _data;

        private State _state;
        private float _stateTimeLeft;
        private Vector2 _dashDirection;
        private bool _hasHitThisDash;

        protected override EnemyData Data => _data;
        protected override float EngageRange => _data.DashStartRange;

        protected override void OnEngage()
        {
            ChangeState(State.Windup);
        }

        protected override void TickEngaged(float distance)
        {
            switch (_state)
            {
                case State.Windup:
                    Stop();
                    if (TimeUp()) ChangeState(State.Dash);
                    break;

                case State.Dash:
                    Move(_dashDirection, _data.DashSpeed);

                    if (!_hasHitThisDash && distance <= _data.ContactRadius)
                    {
                        HitTarget(_data.DashDamage);
                        _hasHitThisDash = true;
                    }

                    if (TimeUp()) ChangeState(State.Recover);
                    break;

                case State.Recover:
                    Stop();
                    if (TimeUp()) ReturnToChase();
                    break;
            }
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
                case State.Windup:
                    _stateTimeLeft = _data.WindupTime;
                    _dashDirection = DirectionToTarget();
                    SetColor(Color.yellow);
                    break;

                case State.Dash:
                    _stateTimeLeft = _data.DashTime;
                    _hasHitThisDash = false;
                    SetColor(Color.magenta);
                    break;

                case State.Recover:
                    _stateTimeLeft = _data.RecoverTime;
                    SetColor(Color.gray);
                    break;
            }
        }
    }
}
