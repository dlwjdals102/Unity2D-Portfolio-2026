using JM2D.Core;
using JM2D.Logic.Progress;
using JM2D.Logic.Stats;
using JM2D.View;
using JM2D.Weapons;
using System.Collections.Generic;
using UnityEngine;

namespace JM2D.Player
{
    /// 지금 든 무기로 공격한다. 입력, 공격 간격, 대시 금지, 스탯을 맡는다.
    /// 무기마다 다른 '한 번 공격하는 방식' 은 Weapon 이 맡는다.
    ///
    /// 휘두르는 소리는 씬의 HitFeedback 에 두지 않고 여기서 낸다.
    /// 그쪽은 '맞았을 때' 자리이고 이것은 '휘둘렀을 때' 자리다.
    /// 타격음이 씬에 있어야 하는 이유는 맞은 적이 그 프레임에 파괴되는 것인데,
    /// 플레이어는 파괴되지 않아 그 제약이 없다. 모으면 제약 없는 쪽까지 씬에 묶인다.
    [RequireComponent(typeof(AudioSource))]
    public class PlayerAttack : MonoBehaviour
    {
        [Tooltip("저장된 무기가 없거나 잠겨 있으면 0 번을 든다")]
        [SerializeField] private Weapon[] _weapons;

        [Tooltip("공격을 시작할 때 한 번 난다. 맞지 않아도 난다")]
        [SerializeField] private AudioClip _swingClip;

        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private PlayerStats _stats;

        /// 시작할 때 저장된 무기를 읽는 데 쓴다.
        private readonly SaveFile _file = new SaveFile();

        private Weapon _current;
        private SpriteFacing _facing;
        private CharacterAnimator _characterAnimator;
        private AudioSource _audio;
        private float _fireCooldownLeft;

        /// 공격을 시작할 때 묶어 둔다. 판정이 칼 나가는 프레임까지 밀리므로,
        /// 그 사이에 무기를 바꾸거나 마우스를 돌려도 시작할 때의 것으로 나간다.
        private Weapon _pendingWeapon;
        private Vector2 _pendingDirection;
        private int _pendingDamage;

        /// 휘두르는 중인가. 공격이 시작될 때 켜고 애니메이션이 끝날 때 끈다.
        /// 그동안 몸을 돌리지 않아, 칼이 왼쪽으로 가다 오른쪽에서 이어지는 일이 없다.
        private bool _isSwinging;

        private bool CanFire => _fireCooldownLeft <= 0f && !_motor.IsDashing;
        /// 마을의 선택대가 이름과 수치를 읽는다. 번호 순서가 해금 규칙의 순서와 같아야 한다.
        public IReadOnlyList<Weapon> Weapons => _weapons;

        /// 구독이 OnEnable 에 있어 참조를 Awake 에서 잡는다. OnEnable 은 Start 보다 먼저 돈다.
        private void Awake()
        {
            _facing = GetComponent<SpriteFacing>();
            _characterAnimator = GetComponent<CharacterAnimator>();
            _audio = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            _characterAnimator.AttackFrameReached += FirePendingAttack;
            _characterAnimator.AttackEnded += OnSwingEnded;
        }

        /// 저장된 무기를 들고 시작한다. 던전과 마을 둘 다 이 자리를 지난다.
        ///
        /// Start 인 이유는 PlayerStats 가 Awake 에서 스탯을 만들기 때문이다.
        /// 그보다 먼저 들면 무기의 공격력과 공격 속도를 얹을 자리가 아직 없다.
        private void Start()
        {
            SaveData save = _file.Load();

            // 저장된 번호를 그대로 믿지 않는다. 해금 조건을 올리면 옛 선택이 잠긴 무기가 되고,
            // 저장 파일을 손으로 고치면 없는 번호가 들어올 수도 있다.
            // IsUnlocked 가 범위 밖 번호도 잠긴 것으로 답해서 둘을 한 번에 거른다.
            int weapon = UnlockRules.IsUnlocked(save.SelectedWeapon, save)
                ? save.SelectedWeapon
                : UnlockRules.StarterWeapon;

            Equip(weapon);
        }

        private void Update()
        {
            FacePlayer();

#if UNITY_EDITOR
            // 확인용 뒷문. 무기를 고르는 정식 경로는 마을의 무기대다.
            int slot = _input.WeaponSlotPressed;
            if (slot >= 0 && slot < _weapons.Length)
                Equip(slot);
#endif

            if (_fireCooldownLeft > 0f)
                _fireCooldownLeft -= Time.deltaTime;

            if (_input.FireHeld && CanFire)
            {
                _pendingWeapon = _current;
                _pendingDirection = _input.AimDirection;
                _pendingDamage = _stats.AttackDamage.IntValue;

                // 몸이 이 공격의 방향을 보고 그대로 멈춘다. 판정과 같은 값을 쓰므로
                // 휘두르는 그림과 맞는 쪽이 맞는다.
                _isSwinging = true;
                _facing.Face(_pendingDirection.x);

                _fireCooldownLeft = 1f / _stats.AttackSpeed.Value;

                // 간격을 계산할 때와 같은 값을 넘긴다. 둘이 다른 곳에서 오면 아이템이 붙을 때 갈라진다.
                _characterAnimator.PlayAttack(_stats.AttackSpeed.Value);

                // 판정이 아니라 입력에 붙는다. 빗나간 공격도 소리가 나야 입력이 들린다.
                // 한 프레임에 두 번 들어올 수 없어 겹침을 막는 장치가 없다. CanFire 가 쿨다운을 본다.
                _audio.PlayOneShot(_swingClip);
            }
        }

        private void OnDisable()
        {
            _characterAnimator.AttackFrameReached -= FirePendingAttack;
            _characterAnimator.AttackEnded -= OnSwingEnded;
        }

        /// 칼이 뻗는 프레임에 불린다. 판정과 범위 표시가 여기서 함께 일어난다.
        /// 위치만 지금 것을 쓴다. 휘두르는 동안 걸어갔으면 그림도 거기 있다.
        private void FirePendingAttack()
        {
            _pendingWeapon.Attack(transform.position, _pendingDirection, _pendingDamage);
        }

        /// 휘두르는 동작이 끝나면 다시 몸을 돌릴 수 있다.
        /// 애니메이션이 Has Exit Time 으로만 나가는 것에 기댄다. 조건으로 중간에 끊기면
        /// 이 알림이 오지 않아 방향이 고정된 채로 남는다.
        private void OnSwingEnded()
        {
            _isSwinging = false;
        }

        /// 가는 쪽을 본다. 조준 쪽으로 돌리는 것은 공격이 시작될 때 한 번뿐이다.
        ///
        /// 전에는 쏘는 동안 조준을 따라가게 했는데, 그것이 휘두르는 중에 몸이 뒤집히는
        /// 원인이었다. 공격마다 한 번 돌리는 지금 방식이 '마우스를 흔들면 깜빡인다' 와
        /// '쏘는 쪽이 읽히지 않는다' 를 함께 푼다.
        private void FacePlayer()
        {
            if (_isSwinging) return;

            _facing.Face(_input.MoveInput.x);
        }

        /// 무기를 든다. 옛 무기가 얹은 값만 떼고 새 무기의 값을 얹는다. 아이템이 얹은 값은 건드리지 않는다.
        public void Equip(int index)
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
