using System.Collections.Generic;
using JM2D.Data;
using JM2D.Items;
using JM2D.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JM2D.Debugging
{
    /// <summary>
    /// 숫자 키로 아이템을 장착하고 백스페이스로 마지막 것을 뗀다.
    /// 가방 그리드가 없는 동안 아이템 효과를 손으로 확인하기 위한 도구이며,
    /// 그리드가 생기면 이 컴포넌트째로 지운다.
    ///
    /// 같은 아이템을 여러 번 장착할 수 있다. 검을 둘 끼고 하나만 빼는 것이
    /// 이 단계의 핵심 검증이라 토글이 아니라 쌓는 방식으로 만들었다.
    /// </summary>
    public class StatDebugKeys : MonoBehaviour
    {
        [SerializeField] private ItemInventory _inventory;
        [SerializeField] private PlayerStats _stats;

        [Header("아이템 (1~4 키 순서)")]
        [SerializeField] private ItemData _boots;
        [SerializeField] private ItemData _sword;
        [SerializeField] private ItemData _gloves;
        [SerializeField] private ItemData _armor;

        /// 장착한 순서대로 쌓는다. 백스페이스는 맨 뒤부터 뺀다.
        private readonly List<ItemInstance> _equipped = new List<ItemInstance>();

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame) Equip(_boots);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) Equip(_sword);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) Equip(_gloves);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) Equip(_armor);

            if (Keyboard.current.backspaceKey.wasPressedThisFrame) UnequipLast();
            if (Keyboard.current.digit0Key.wasPressedThisFrame) UnequipAll();
        }

        private void Equip(ItemData data)
        {
            _equipped.Add(_inventory.Equip(data));
            Log($"{data.DisplayName} 장착");
        }

        private void UnequipLast()
        {
            if (_equipped.Count == 0) return;

            int last = _equipped.Count - 1;
            ItemInstance instance = _equipped[last];
            _equipped.RemoveAt(last);

            _inventory.Unequip(instance);
            Log($"{instance.Data.DisplayName} 해제");
        }

        private void UnequipAll()
        {
            while (_equipped.Count > 0)
                UnequipLast();
        }

        private void Log(string what)
        {
            Debug.Log($"[아이템] {what} · 착용 {_equipped.Count}개 → " +
                      $"공격력 {_stats.AttackDamage.IntValue} / " +
                      $"이동 {_stats.MoveSpeed.Value:F2} / " +
                      $"연사 {_stats.AttackSpeed.Value:F2} / " +
                      $"최대체력 {_stats.MaxHealth.IntValue}");
        }
    }
}
