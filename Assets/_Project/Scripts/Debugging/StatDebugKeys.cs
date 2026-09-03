using JM2D.Logic;
using JM2D.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JM2D.Debugging
{
    /// <summary>
    /// 숫자 키로 플레이어 스탯에 모디파이어를 붙이고 뗀다.
    /// 아이템이 없는 Phase 3에서 스탯 시스템을 손으로 검증하기 위한 도구이며,
    /// 아이템이 생기는 Phase 4에서 지운다.
    ///
    /// 출처를 이 컴포넌트 자신으로 두므로 RemoveAllFrom(this) 하나로 뗄 수 있다.
    /// 스탯이 서로 다른 객체라 출처가 하나여도 섞이지 않는다.
    /// </summary>
    public class StatDebugKeys : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;

        private bool _isSpeedOn;
        private bool _isDamageOn;
        private bool _isAttackSpeedOn;
        private bool _isMaxHealthOn;

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                Toggle(_stats.MoveSpeed, ref _isSpeedOn, ModifierType.PercentAdd, 0.5f, "이동속도 +50%");

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
                Toggle(_stats.AttackDamage, ref _isDamageOn, ModifierType.Flat, 2f, "공격력 +2");

            if (Keyboard.current.digit3Key.wasPressedThisFrame)
                Toggle(_stats.AttackSpeed, ref _isAttackSpeedOn, ModifierType.PercentAdd, 1f, "공격속도 +100%");

            if (Keyboard.current.digit4Key.wasPressedThisFrame)
                Toggle(_stats.MaxHealth, ref _isMaxHealthOn, ModifierType.Flat, 2f, "최대체력 +2");

            if (Keyboard.current.digit0Key.wasPressedThisFrame)
                RemoveEverything();
        }

        private void Toggle(Stat stat, ref bool isOn, ModifierType type, float value, string label)
        {
            if (isOn)
                stat.RemoveAllFrom(this);
            else
                stat.AddModifier(new StatModifier(type, value, this));

            isOn = !isOn;

            Debug.Log($"[스탯] {label} {(isOn ? "켬" : "끔")} → {stat.Value:F2}");
        }

        private void RemoveEverything()
        {
            _stats.MoveSpeed.RemoveAllFrom(this);
            _stats.AttackDamage.RemoveAllFrom(this);
            _stats.AttackSpeed.RemoveAllFrom(this);
            _stats.MaxHealth.RemoveAllFrom(this);

            _isSpeedOn = false;
            _isDamageOn = false;
            _isAttackSpeedOn = false;
            _isMaxHealthOn = false;

            Debug.Log("[스탯] 전부 제거");
        }
    }
}
