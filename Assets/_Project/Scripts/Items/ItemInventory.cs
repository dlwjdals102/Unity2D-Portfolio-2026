using System.Collections.Generic;
using JM2D.Data;
using JM2D.Logic;
using JM2D.Player;
using UnityEngine;

namespace JM2D.Items
{
    /// 장착한 아이템 목록을 갖고, 스탯에 모디파이어를 붙이고 뗀다.
    /// 가방 그리드가 생기면 이 클래스가 그 역할을 넘겨받는다.
    public class ItemInventory : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;

        private readonly List<ItemInstance> _items = new List<ItemInstance>();

        public ItemInstance Equip(ItemData data)
        {
            var instance = new ItemInstance(data);

            foreach (ItemModifier m in data.Modifiers)
                _stats.Get(m.Target).AddModifier(new StatModifier(m.Type, m.Value, instance));

            _items.Add(instance);
            return instance;
        }

        public void Unequip(ItemInstance instance)
        {
            foreach (ItemModifier m in instance.Data.Modifiers)
                _stats.Get(m.Target).RemoveAllFrom(instance);

            _items.Remove(instance);
        }
    }
}
