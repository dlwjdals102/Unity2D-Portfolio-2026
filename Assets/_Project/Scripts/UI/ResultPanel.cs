using JM2D.Combat;
using JM2D.Core;
using JM2D.Logic.Rooms;
using JM2D.Rooms;
using TMPro;
using UnityEngine;

namespace JM2D.UI
{
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] private Health _playerHealth;
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _message;
        [SerializeField] private EnemyCounter _enemyCounter;
        [SerializeField] private RoomTracker _roomTracker;

        private void OnEnable()
        {
            _playerHealth.OnDied += ShowGameOver;
            _enemyCounter.OnAllEnemiesDead += ShowClear;
            _roomTracker.OnRoomEntered += ShowClearIfBossRoom;
        }

        private void OnDisable()
        {
            _playerHealth.OnDied -= ShowGameOver;
            _enemyCounter.OnAllEnemiesDead -= ShowClear;
            _roomTracker.OnRoomEntered -= ShowClearIfBossRoom;
        }

        /// 임시 클리어 조건. 4-C 에서 웨이브와 보스 전투가 들어오면 지운다.
        private void ShowClearIfBossRoom(Room room)
        {
            if (room.Type == RoomType.Boss) ShowClear();
        }

        private void ShowGameOver()
        {
            Show("게임 오버");
        }

        private void ShowClear()
        {
            Show("클리어");
        }

        private void Show(string message)
        {
            _message.text = message;
            _root.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
