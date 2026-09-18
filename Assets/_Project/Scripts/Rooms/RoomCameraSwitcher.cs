using UnityEngine;

namespace JM2D.Rooms
{
    /// 플레이어가 들어간 방의 카메라를 켜고, 나온 방의 카메라를 끈다.
    /// 어느 카메라가 화면을 맡을지는 Cinemachine 이 켜진 카메라 중에서 고른다.
    public class RoomCameraSwitcher : MonoBehaviour
    {
        [SerializeField] private RoomTracker _tracker;
        [SerializeField] private Transform _target;

        private Room _current;

        private void OnEnable()
        {
            _tracker.OnRoomEntered += Switch;
        }

        private void OnDisable()
        {
            _tracker.OnRoomEntered -= Switch;
        }

        private void Switch(Room room)
        {
            if (_current != null) _current.HideCamera();

            room.ShowCamera(_target);
            _current = room;
        }
    }
}
