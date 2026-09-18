using System;
using JM2D.Logic.Rooms;
using UnityEngine;

namespace JM2D.Rooms
{
    /// 플레이어가 어느 방에 있는지 매 프레임 확인하고, 방이 바뀌면 알린다.
    /// 판이 시작될 때 시작 방에 들어간 것도 알린다.
    public class RoomTracker : MonoBehaviour
    {
        [SerializeField] private LevelBuilder _level;
        [SerializeField] private Transform _player;

        private RoomCell _currentCell;

        public event Action<Room> OnRoomEntered;

        // LevelBuilder 가 Awake 에서 방을 놓는다. 그 뒤라야 배치를 읽을 수 있어 Start 에서 한다.
        private void Start()
        {
            Enter(PlayerCell());
        }

        private void Update()
        {
            RoomCell cell = PlayerCell();

            if (cell == _currentCell) return;

            Enter(cell);
        }

        private RoomCell PlayerCell()
        {
            Vector3 position = _player.position;
            return RoomSpace.ToCell(position.x, position.y, _level.RoomSize.x, _level.RoomSize.y);
        }

        private void Enter(RoomCell cell)
        {
            _currentCell = cell;
            OnRoomEntered?.Invoke(_level.GetRoom(cell));
        }
    }
}
