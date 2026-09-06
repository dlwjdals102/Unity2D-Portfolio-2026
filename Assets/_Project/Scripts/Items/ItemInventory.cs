using JM2D.Data;
using JM2D.Logic;
using JM2D.Player;
using UnityEngine;

namespace JM2D.Items
{
    /// 가방 그리드에 아이템을 놓고 빼면서 스탯에 모디파이어를 붙이고 뗀다.
    /// 아이템 목록은 그리드가 갖는다. 이 클래스는 그리드와 스탯을 잇는다.
    public class ItemInventory : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;

        private readonly BagGrid _grid = new BagGrid(5, 5);

        public BagGrid Grid => _grid;

        public bool TryPlace(ItemInstance instance, int x, int y)
        {
            if (!_grid.TryPlace(instance, x, y)) return false;

            foreach (ItemModifier m in instance.Data.Modifiers)
                _stats.Get(m.Target).AddModifier(new StatModifier(m.Type, m.Value, instance));

            return true;
        }

        public void Remove(ItemInstance instance)
        {
            _grid.Remove(instance);

            foreach (ItemModifier m in instance.Data.Modifiers)
                _stats.Get(m.Target).RemoveAllFrom(instance);
        }
    }
}
