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

        /// 시너지가 붙인 모디파이어의 출처.
        /// 아이템이 붙인 것과 구분되어야 시너지만 지울 수 있다.
        private readonly object _synergySource = new object();

        public BagGrid Grid => _grid;

        public bool TryPlace(ItemInstance instance, int x, int y)
        {
            if (!_grid.TryPlace(instance, x, y)) return false;

            foreach (ItemModifier m in instance.Data.Modifiers)
                _stats.Get(m.Target).AddModifier(new StatModifier(m.Type, m.Value, instance));

            RecalculateSynergy();

            return true;
        }

        public void Remove(ItemInstance instance)
        {
            _grid.Remove(instance);

            foreach (ItemModifier m in instance.Data.Modifiers)
                _stats.Get(m.Target).RemoveAllFrom(instance);

            RecalculateSynergy();
        }

        /// 배치가 바뀔 때마다 시너지를 전부 지우고 다시 계산한다.
        private void RecalculateSynergy()
        {
            foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
                _stats.Get(type).RemoveAllFrom(_synergySource);

            foreach (PlacedItem placed in _grid.GetPlacedItems())
            {
                var instance = (ItemInstance)placed.Item;
                int adjacent = _grid.CountAdjacent(instance);

                if (adjacent == 0) continue;

                foreach (ItemModifier m in instance.Data.AdjacencyBonus)
                    _stats.Get(m.Target).AddModifier(
                        new StatModifier(m.Type, m.Value * adjacent, _synergySource));
            }
        }
    }
}
