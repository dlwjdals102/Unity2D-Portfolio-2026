using JM2D.Data;
using JM2D.Logic.Stats;
using JM2D.Logic.Bag;
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

        /// 아직 가방에 놓지 않은 아이템. 없으면 null.
        /// 방 보상으로 들어오고, 그리드에 놓이면 빈다. 판이 끝날 때까지 남는다.
        private ItemInstance _hand;

        public BagGrid Grid => _grid;
        public ItemInstance Hand => _hand;

        public bool TryPlace(ItemInstance instance, int x, int y)
        {
            if (!_grid.TryPlace(instance, x, y)) return false;

            foreach (ItemModifier m in instance.Data.Modifiers)
                _stats.Get(m.Target).AddModifier(new StatModifier(m.Type, m.Value, instance));

            RecalculateSynergy();

            return true;
        }
        
        /// 보상이나 디버그가 손을 채운다. 이미 들고 있으면 덮어쓴다.
        public void PutInHand(ItemData data)
        {
            _hand = new ItemInstance(data);
        }

        /// 놓기 전에 돌린다. 손이 비어 있으면 아무 일도 하지 않는다.
        public void RotateHand()
        {
            if (_hand == null) return;

            _hand.Rotate();
        }

        /// 손에 든 것을 그리드에 놓는다. 성공하면 손이 빈다.
        public bool TryPlaceHand(int x, int y)
        {
            if (_hand == null) return false;

            if (!TryPlace(_hand, x, y)) return false;

            _hand = null;
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

                foreach (ItemSynergy s in instance.Data.Synergies)
                {
                    int count = _grid.Count(instance, s.Condition);

                    if (count == 0) continue;

                    ItemModifier m = s.Modifier;

                    _stats.Get(m.Target).AddModifier(
                        new StatModifier(m.Type, m.Value * count, _synergySource));
                }
            }
        }
    }
}
