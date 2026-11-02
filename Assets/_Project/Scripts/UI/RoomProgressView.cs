using JM2D.Rooms;
using TMPro;
using UnityEngine;

namespace JM2D.UI
{
    /// 지금 몇 번째 방인지 보여준다. 방이 바뀔 때만 갱신한다.
    /// 방 번호는 배치의 순번이다. 한 줄 걷기라 걸은 순서가 곧 진행 순서이고, 가지형이면 이 숫자에 뜻이 없다.
    public class RoomProgressView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        [SerializeField] private RoomTracker _tracker;
        [SerializeField] private LevelBuilder _level;

        /// OnEnable 은 모든 Start 보다 먼저 돈다. RoomTracker 가 Start 에서 알리므로 첫 방부터 받는다.
        private void OnEnable()
        {
            _tracker.OnRoomEntered += Refresh;
        }

        private void OnDisable()
        {
            _tracker.OnRoomEntered -= Refresh;
        }

        private void Refresh(Room room)
        {
            int index = _level.Layout.IndexOf(room.Cell) + 1;

            _text.text = $"방 {index} / {_level.Layout.Rooms.Count}";
        }
    }
}
