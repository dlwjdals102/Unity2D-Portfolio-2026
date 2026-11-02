using System;
using UnityEngine;
using JM2D.Combat;
using JM2D.Rooms;
using JM2D.Logic.Progress;
using JM2D.Logic.Rooms;

namespace JM2D.Core
{
    /// 이번 판이 어디까지 갔는지 세고, 끝날 때 저장한다.
    /// 읽고 쓰는 것은 SaveFile, 무엇을 남길지는 UnlockRules 가 정한다. 이 클래스는 '언제' 를 안다.
    public class RunRecorder : MonoBehaviour
    {
        [SerializeField] private RoomCombat _combat;
        [SerializeField] private LevelBuilder _level;
        [SerializeField] private Health _playerHealth;

        /// 인자는 차례로 보스를 잡았는가, 저장된 기록, 새로 열린 무기 수다.
        /// 저장이 끝난 뒤에 알린다. 듣는 쪽이 죽음을 직접 들으면 저장보다 먼저 글을 써 기록이 한 판 뒤처진다.
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

        private void OnDisable()
        {
            _combat.OnCombatStarted -= OnCombatStarted;
            _combat.OnRoomCleared -= OnRoomCleared;
            _playerHealth.OnDied -= OnPlayerDied;
        }

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
