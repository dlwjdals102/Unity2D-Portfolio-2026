using System.Collections.Generic;
using JM2D.Data;
using JM2D.Logic.Common;
using JM2D.Logic.Rooms;
using JM2D.Rooms;
using JM2D.UI;
using UnityEngine;

namespace JM2D.Items
{
    /// 방을 깨면 보상 후보를 뽑아 창에 띄우고, 고른 것을 손에 넣는다.
    /// 전투 진행과 창과 가방을 잇기만 한다. 셋은 서로를 모른다.
    public class RewardGiver : MonoBehaviour
    {
        [Tooltip("보상으로 나올 수 있는 아이템 전부")]
        [SerializeField] private ItemData[] _items;

        [Tooltip("한 번에 보여줄 후보 수")]
        [Min(1)]
        [SerializeField] private int _candidateCount = 3;

        [SerializeField] private LevelBuilder _level;
        [SerializeField] private RoomCombat _combat;
        [SerializeField] private RewardPanel _panel;
        [SerializeField] private ItemInventory _inventory;

        [Tooltip("보상을 고르면 곧바로 놓을 수 있게 가방을 연다")]
        [SerializeField] private BagView _bag;

        /// 후보를 뽑는 난수. 조합을 뽑는 난수와 별개이고 시드만 같다.
        private System.Random _random;

        /// 이번에 보여줄 후보. 방마다 다시 채운다.
        private readonly List<ItemData> _candidates = new List<ItemData>();

        private void OnEnable()
        {
            _combat.OnRoomCleared += OnRoomCleared;
            _panel.OnPicked += OnPicked;
        }

        // LevelBuilder 가 Awake 에서 시드를 정한다. 그 뒤라야 읽을 수 있어 Start 에서 한다.
        private void Start()
        {
            _random = new System.Random(_level.CurrentSeed);
        }

        private void OnDisable()
        {
            _combat.OnRoomCleared -= OnRoomCleared;
            _panel.OnPicked -= OnPicked;
        }

        /// 전투 방을 깨면 후보를 뽑아 창을 띄운다. 보스 방은 클리어 화면이 뜬다.
        private void OnRoomCleared(Room room)
        {
            if (room.Type == RoomType.Boss) return;

            _candidates.Clear();

            int[] indices = RandomPick.Distinct(_items.Length, _candidateCount, _random);

            foreach (int index in indices)
                _candidates.Add(_items[index]);

            _panel.Show(_candidates);

            Time.timeScale = 0f;
        }

        /// 창에서 하나를 고르면 손에 넣고 다시 게임이 흐른다.
        private void OnPicked(ItemData data)
        {
            _inventory.PutInHand(data);
            _bag.Open();
            Time.timeScale = 1f;
        }
    }
}
