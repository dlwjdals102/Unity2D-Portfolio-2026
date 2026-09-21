using JM2D.Combat;
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
        [SerializeField] private RoomCombat _roomCombat;

        private void OnEnable()
        {
            _playerHealth.OnDied += ShowGameOver;
            _roomCombat.OnRoomCleared += ShowClearIfBossRoom;
        }

        private void OnDisable()
        {
            _playerHealth.OnDied -= ShowGameOver;
            _roomCombat.OnRoomCleared -= ShowClearIfBossRoom;
        }

        /// 보스 방을 깨면 판이 끝난다. 보스는 Phase 5 라 지금은 보스 방의 마지막 웨이브가 그 자리다.
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
