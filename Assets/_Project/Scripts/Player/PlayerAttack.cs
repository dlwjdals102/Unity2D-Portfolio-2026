using JM2D.Logic.Stats;
using JM2D.View;
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
        private SpriteFacing _facing;
        private CharacterAnimator _characterAnimator;
        private float _fireCooldownLeft;

        /// 공격을 시작할 때 묶어 둔다. 판정이 칼 나가는 프레임까지 밀리므로,
        /// 그 사이에 무기를 바꾸거나 마우스를 돌려도 시작할 때의 것으로 나간다.
        private Weapon _pendingWeapon;
        private Vector2 _pendingDirection;
        private int _pendingDamage;

        private bool CanFire => _fireCooldownLeft <= 0f && !_motor.IsDashing;

        /// 구독이 OnEnable 에 있어 참조를 Awake 에서 잡는다. OnEnable 은 Start 보다 먼저 돈다.
        private void Awake()
        {
            _facing = GetComponent<SpriteFacing>();
            _characterAnimator = GetComponent<CharacterAnimator>();
        }

        private void OnEnable()
        {
            _characterAnimator.AttackFrameReached += FirePendingAttack;
        }

        /// PlayerStats 가 Awake 에서 스탯을 만든 뒤라야 모디파이어를 붙일 수 있어 Start 에서 든다.
        private void Start()
        {
            Equip(0);
        }

        private void Update()
        {
            FacePlayer();

            int slot = _input.WeaponSlotPressed;
            if (slot >= 0 && slot < _weapons.Length)
                Equip(slot);

            if (_fireCooldownLeft > 0f)
                _fireCooldownLeft -= Time.deltaTime;

            if (_input.FireHeld && CanFire)
            {
                _pendingWeapon = _current;
                _pendingDirection = _input.AimDirection;
                _pendingDamage = _stats.AttackDamage.IntValue;

                _fireCooldownLeft = 1f / _stats.AttackSpeed.Value;

                // 간격을 계산할 때와 같은 값을 넘긴다. 둘이 다른 곳에서 오면 아이템이 붙을 때 갈라진다.
                _characterAnimator.PlayAttack(_stats.AttackSpeed.Value);
            }
        }

        private void OnDisable()
        {
            _characterAnimator.AttackFrameReached -= FirePendingAttack;
        }

        /// 칼이 뻗는 프레임에 불린다. 판정과 범위 표시가 여기서 함께 일어난다.
        /// 위치만 지금 것을 쓴다. 휘두르는 동안 걸어갔으면 그림도 거기 있다.
        private void FirePendingAttack()
        {
            _pendingWeapon.Attack(transform.position, _pendingDirection, _pendingDamage);
        }

        /// 쏘는 동안에는 조준한 쪽, 아니면 가는 쪽을 본다.
        /// 조준 방향만 쓰면 쏘지 않을 때도 마우스를 흔들 때마다 몸이 깜빡인다.
        /// 이동 방향만 쓰면 쏘는 쪽이 읽히지 않는데, 조준은 RangeView 와 투사체가 이미 알린다.
        /// 공격하는 순간만 돌리는 안은 버렸다. 공격이 한 프레임이라 눈에 보이지 않는다.
        private void FacePlayer()
        {
            float x = _input.FireHeld ? _input.AimDirection.x : _input.MoveInput.x;
            _facing.Face(x);
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
