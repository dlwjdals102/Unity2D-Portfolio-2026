using JM2D.Logic;
using JM2D.Weapons;
using UnityEngine;

namespace JM2D.Player
{
    /// 지금 든 무기로 공격한다. 입력, 공격 간격, 대시 금지, 스탯을 맡는다.
    /// 무기마다 다른 '한 번 공격하는 방식' 은 Weapon 이 맡는다.
    public class PlayerAttack : MonoBehaviour
    {
        [Tooltip("0 번이 시작할 때 드는 무기다")]
        [SerializeField] private Weapon[] _weapons;

        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private PlayerStats _stats;

        private Weapon _current;
        private float _fireCooldownLeft;

        private bool CanFire => _fireCooldownLeft <= 0f && !_motor.IsDashing;

        /// PlayerStats 가 Awake 에서 스탯을 만든 뒤라야 모디파이어를 붙일 수 있어 Start 에서 든다.
        private void Start()
        {
            Equip(0);
        }

        private void Update()
        {
            int slot = _input.WeaponSlotPressed;
            if (slot >= 0 && slot < _weapons.Length)
                Equip(slot);

            if (_fireCooldownLeft > 0f)
                _fireCooldownLeft -= Time.deltaTime;

            if (_input.FireHeld && CanFire)
            {
                _current.Attack(transform.position, _input.AimDirection, _stats.AttackDamage.IntValue);
                _fireCooldownLeft = 1f / _stats.AttackSpeed.Value;
            }
        }

        /// 무기를 든다. 옛 무기가 얹은 값만 떼고 새 무기의 값을 얹는다. 아이템이 얹은 값은 건드리지 않는다.
        private void Equip(int index)
        {
            if (_current != null)
            {
                _stats.AttackDamage.RemoveAllFrom(_current);
                _stats.AttackSpeed.RemoveAllFrom(_current);
            }

            _current = _weapons[index];

            _stats.AttackDamage.AddModifier(new StatModifier(ModifierType.Flat, _current.Data.AttackDamage, _current));
            _stats.AttackSpeed.AddModifier(new StatModifier(ModifierType.Flat, _current.Data.AttacksPerSecond, _current));
        }
    }
}
