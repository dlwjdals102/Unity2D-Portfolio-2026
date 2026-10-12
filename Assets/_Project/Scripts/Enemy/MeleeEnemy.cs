using JM2D.Data;
using JM2D.Logic.Common;
using UnityEngine;

namespace JM2D.Enemy
{
    /// 추적 근접 적. 붙으면 멈춰서 쿨다운마다 때린다.
    public class MeleeEnemy : EnemyBase
    {
        [SerializeField] private MeleeEnemyData _data;

        private float _attackCooldownLeft;

        protected override EnemyData Data => _data;
        protected override float EngageRange => _data.AttackStartRange;

        /// 교전에 들어갈 때 할 일이 없다. 붙어서 때리는 것은 TickEngaged 가 한다.
        protected override void OnEngage()
        {
        }

        protected override void TickEngaged(float distance)
        {
            Stop();

            if (_attackCooldownLeft > 0f)
                _attackCooldownLeft -= Time.fixedDeltaTime;
            else
            {
                HitAround(_data.AttackStopRange, _data.AttackDamage);
                _attackCooldownLeft = _data.AttackCooldown;
            }

            if (!Hysteresis.IsInside(true, distance, _data.AttackStartRange, _data.AttackStopRange))
                ReturnToChase();
        }
    }
}
