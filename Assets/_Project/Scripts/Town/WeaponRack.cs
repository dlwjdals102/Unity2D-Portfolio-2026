using TMPro;
using UnityEngine;
using JM2D.Core;
using JM2D.Data;
using JM2D.Logic.Progress;
using JM2D.Player;

namespace JM2D.Town
{
    /// 무기 하나가 놓인 자리. 고른 것은 저장되고 던전이 그 값을 읽어 시작한다.
    /// 무기마다 하나씩 놓고, 창을 띄우지 않으므로 이동이나 공격을 막을 일도 없다.
    public class WeaponRack : Interactable
    {
        [Tooltip("이 자리의 무기 번호. 해금 규칙과 PlayerAttack 의 무기 순서를 함께 따른다")]
        [SerializeField] private int _weaponIndex;

        [Tooltip("이름과 수치를 여기서 읽고, 고른 무기를 여기에 들린다")]
        [SerializeField] private PlayerAttack _playerAttack;

        [Tooltip("다가갔을 때 보일 글. Interactable 의 Prompt 안에 있는 글자")]
        [SerializeField] private TMP_Text _promptText;

        [Tooltip("잠겨 있으면 어둡게 둘 그림. 비워 두면 아무것도 하지 않는다")]
        [SerializeField] private SpriteRenderer _icon;

        [SerializeField] private Color _lockedTint = new Color(0.35f, 0.35f, 0.4f, 1f);

        private readonly SaveFile _file = new SaveFile();

        /// 다가올 때 읽어 둔 저장값. 누를 때 잠겼는지 다시 본다.
        private SaveData _save;

        private void Start()
        {
            // 잠긴 자리는 멀리서도 어두워 보인다. 다가가 보기 전에 알 수 있다.
            if (_icon != null && !UnlockRules.IsUnlocked(_weaponIndex, _file.Load()))
                _icon.color = _lockedTint;
        }

        /// 다가온 순간 파일을 읽는다. 무기대끼리 서로를 모르면서도 표시가 어긋나지 않는다.
        protected override void OnPlayerNear()
        {
            _save = _file.Load();

            WritePrompt();
        }

        protected override void Interact()
        {
            if (!UnlockRules.IsUnlocked(_weaponIndex, _save)) return;

            _save.SelectedWeapon = _weaponIndex;

            _file.Save(_save);

            _playerAttack.Equip(_weaponIndex);

            WritePrompt();
        }

        private void WritePrompt()
        {
            if (_promptText == null) return;

            if (!UnlockRules.IsUnlocked(_weaponIndex, _save))
            {
                _promptText.text = UnlockRules.ConditionText(_weaponIndex);
                return;
            }

            WeaponData data = _playerAttack.Weapons[_weaponIndex].Data;

            if (_save.SelectedWeapon == _weaponIndex)
            {
                _promptText.text = $"{data.DisplayName}\n지금 들고 있다";
                return;
            }

            _promptText.text = $"E - {data.DisplayName}\n공격력 {data.AttackDamage}  연사 {data.AttacksPerSecond:F1}";
        }
    }
}
