using JM2D.Data;
using JM2D.Logic;
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

        protected override void OnEngage()
        {
            SetColor(Color.green);
        }

        protected override void TickEngaged(float distance)
        {
            Stop();

            if (_attackCooldownLeft > 0f)
                _attackCooldownLeft -= Time.fixedDeltaTime;
            else
            {
                HitTarget(_data.AttackDamage);
                _attackCooldownLeft = _data.AttackCooldown;
            }

            if (!Hysteresis.IsInside(true, distance, _data.AttackStartRange, _data.AttackStopRange))
                ReturnToChase();
        }
    }
}
