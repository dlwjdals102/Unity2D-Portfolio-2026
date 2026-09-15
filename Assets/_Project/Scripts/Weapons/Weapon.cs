using JM2D.Data;
using UnityEngine;

namespace JM2D.Weapons
{
    /// 무기 하나. 한 번 공격하는 방식만 정한다.
    /// 입력, 공격 간격, 대시 금지, 스탯은 PlayerAttack 이 맡는다.
    public abstract class Weapon : MonoBehaviour
    {
        /// 이 무기가 스탯에 얹을 공격력과 공격 속도.
        public abstract WeaponData Data { get; }

        /// 한 번 공격한다. 간격, 대시, 입력은 부르는 쪽이 이미 확인했다.
        public abstract void Attack(Vector2 origin, Vector2 direction, int damage);
    }
}
