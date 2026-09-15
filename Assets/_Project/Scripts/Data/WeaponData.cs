using UnityEngine;

namespace JM2D.Data
{
    /// 모든 무기가 주는 공격력과 공격 속도. 무기마다 자식 데이터가 자기 것을 더한다.
    /// 공격하는 방식은 여기 없다. 무기 컴포넌트가 이 값을 읽어 공격한다.
    public abstract class WeaponData : ScriptableObject
    {
        [Header("공격")]
        [SerializeField] private float _attackDamage = 1f;

        [Min(0.1f)]
        [Tooltip("초당 공격 수. 공격 간격은 이 값의 역수다")]
        [SerializeField] private float _attacksPerSecond = 6.67f;

        public float AttackDamage => _attackDamage;
        public float AttacksPerSecond => _attacksPerSecond;
    }
}
