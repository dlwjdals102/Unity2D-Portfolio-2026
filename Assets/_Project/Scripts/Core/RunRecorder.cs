using System;
using UnityEngine;
using JM2D.Combat;
using JM2D.Player;
using JM2D.Rooms;
using JM2D.Logic.Progress;
using JM2D.Logic.Rooms;

namespace JM2D.Core
{
    /// 이번 판이 어디까지 갔는지 세고, 끝날 때 저장한다.
    /// 저장 파일을 읽고 쓰는 것은 SaveFile 이 하고, 무엇을 남길지는 UnlockRules 가 정한다.
    /// 이 클래스는 둘을 잇고 '언제' 를 안다.
    public class RunRecorder : MonoBehaviour
    {
        [SerializeField] private RoomCombat _combat;
        [SerializeField] private LevelBuilder _level;
        [SerializeField] private Health _playerHealth;
        [SerializeField] private PlayerAttack _playerAttack;

        /// 판이 끝나고 기록을 저장한 뒤에 알린다. 인자는 차례로
        /// 보스를 잡았는가, 저장된 기록, 이번 판으로 새로 열린 무기 수다.
        ///
        /// 저장이 끝난 뒤에 알리는 것이 요점이다. 듣는 쪽이 죽음이나 클리어를 직접 들으면
        /// 저장보다 먼저 글을 쓸 수 있고, 그러면 기록이 한 판 뒤처져 보인다.
        public event Action<bool, SaveData, int> OnRunFinished;

        private readonly SaveFile _file = new SaveFile();

        /// 이번 판에서 가장 깊이 들어간 방 번호. 1부터 센다.
        private int _roomsReached = 1;

        /// 판이 끝났는가. 두 번 저장하지 않으려고 둔다.
        private bool _isRunOver;

        private void OnEnable()
        {
            _combat.OnCombatStarted += OnCombatStarted;
            _combat.OnRoomCleared += OnRoomCleared;
            _playerHealth.OnDied += OnPlayerDied;
        }

        /// PlayerStats 가 Awake 에서 스탯을 만든 뒤라야 무기의 값을 얹을 수 있어 Start 에서 든다.
        /// PlayerAttack 이 스스로 0번을 들던 자리를 여기로 옮겼다. 둘 다 들면 한 프레임 동안 활이 들린다.
        private void Start()
        {
            SaveData save = _file.Load();

            // 저장된 번호를 그대로 믿지 않는다. 해금 조건을 올리면 옛 선택이 잠긴 무기가 되고,
            // 저장 파일을 손으로 고치면 없는 번호가 들어올 수도 있다.
            // IsUnlocked 가 범위 밖 번호도 잠긴 것으로 답해서 둘을 한 번에 거른다.
            int weapon = UnlockRules.IsUnlocked(save.SelectedWeapon, save)
                ? save.SelectedWeapon
                : UnlockRules.StarterWeapon;

            _playerAttack.Equip(weapon);
        }

        private void OnDisable()
        {
            _combat.OnCombatStarted -= OnCombatStarted;
            _combat.OnRoomCleared -= OnRoomCleared;
            _playerHealth.OnDied -= OnPlayerDied;
        }

        /// 방에 들어서서 전투가 시작되면 그 방까지 간 것이다.
        /// 걸은 순서가 곧 방 번호라 배치에 물어본다. 시작 방이 0번이므로 하나를 더한다.
        private void OnCombatStarted(Room room)
        {
            int number = _level.Layout.IndexOf(room.Cell) + 1;

            _roomsReached = Mathf.Max(_roomsReached, number);
        }

        private void OnRoomCleared(Room room)
        {
            if (room.Type != RoomType.Boss) return;

            FinishRun(bossKilled: true);
        }

        private void OnPlayerDied()
        {
            FinishRun(bossKilled: false);
        }

        /// 기록을 남긴다. 죽어도 잡아도 한 번만 돈다.
        private void FinishRun(bool bossKilled)
        {
            if (_isRunOver) return;
            _isRunOver = true;

            // 전과 후를 각자 들고 있는다. 새로 열린 것을 세려면 둘을 견줘야 한다.
            SaveData before = _file.Load();
            SaveData after = UnlockRules.After(before, bossKilled, _roomsReached);

            _file.Save(after);

            // 저장을 마친 뒤에 알린다. 값을 함께 넘겨 듣는 쪽이 파일을 다시 읽지 않게 한다.
            OnRunFinished?.Invoke(bossKilled, after, UnlockRules.NewlyUnlockedCount(before, after));
        }
    }
}
