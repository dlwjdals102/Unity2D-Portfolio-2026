using JM2D.Core;
using JM2D.Logic.Rooms;
using UnityEngine;

namespace JM2D.Rooms
{
    /// 지금 싸우는 방의 거리표를 들고 있다가, 플레이어가 칸을 옮기면 다시 계산한다.
    /// 한 번에 한 방만 싸우므로 표도 하나면 된다. 적이 몇 마리든 이 표 하나를 같이 읽는다.
    public class RoomFlowField : MonoBehaviour, IPathField
    {
        [SerializeField] private RoomTracker _tracker;
        [SerializeField] private Transform _player;

        /// 지금 방의 장애물과 격자. 격자가 없는 방(시작 방)이면 null.
        private RoomObstacles _room;

        private FlowField _field;
        private RoomCell _playerCell;

        public RoomObstacles Room => _room;
        public FlowField Field => _field;

        private void OnEnable()
        {
            _tracker.OnRoomEntered += OnRoomEntered;
        }

        private void Update()
        {
            if (_room == null) return;

            RoomCell cell = _room.ToGridCell(_player.position);

            // 칸이 그대로면 표도 그대로다. 같은 값을 다시 만들지 않는다.
            if (cell == _playerCell) return;

            Rebuild(cell);
        }

        private void OnDisable()
        {
            _tracker.OnRoomEntered -= OnRoomEntered;
        }

        /// 방이 바뀌면 그 방의 격자로 갈아탄다. 장애물이 없는 방은 표를 두지 않는다.
        private void OnRoomEntered(Room room)
        {
            RoomObstacles obstacles = room.GetComponent<RoomObstacles>();

            if (obstacles == null || obstacles.Grid == null)
            {
                _room = null;
                return;
            }

            _room = obstacles;

            if (_field == null)
                _field = new FlowField(obstacles.Grid.Width, obstacles.Grid.Height);

            // 방이 바뀌었으니 지금 위치로 한 번 계산해 둔다.
            Rebuild(_room.ToGridCell(_player.position));
        }

        private void Rebuild(RoomCell playerCell)
        {
            _playerCell = playerCell;

            _field.Rebuild(_room.Grid, playerCell.X, playerCell.Y);
        }

        /// 그 자리에서 플레이어 쪽으로 한 발 옮길 자리(이웃 칸의 가운데). 거리표가 없으면 거짓이다.
        public bool TryGetNextPoint(Vector2 from, out Vector2 point)
        {
            point = from;

            if (_room == null || _field == null) return false;

            Vector2 toPlayer = (Vector2)_player.position - from;
            float toPlayerDistance = toPlayer.magnitude;

            if (toPlayerDistance < 0.0001f) return false;

            // 거리가 같은 이웃이 둘일 때 직선에 가까운 쪽을 고르는 데만 쓴다.
            Vector2 straight = toPlayer / toPlayerDistance;

            RoomCell cell = _room.ToGridCell(from);
            int ownDistance = _field.GetDistance(cell.X, cell.Y);

            if (ownDistance == FlowField.Unreachable) return false;

            RoomCell bestCell = cell;
            int bestDistance = ownDistance;
            float bestAlignment = float.NegativeInfinity;

            // 거리표를 만들 때와 같은 이웃과 같은 규칙을 쓴다. 둘이 다르면 갈 수 없는 칸을 고른다.
            for (int i = 0; i < FlowField.OffsetX.Length; i++)
            {
                int stepX = FlowField.OffsetX[i];
                int stepY = FlowField.OffsetY[i];

                if (!FlowField.CanStep(_room.Grid, cell.X, cell.Y, stepX, stepY)) continue;

                var next = new RoomCell(cell.X + stepX, cell.Y + stepY);
                int neighborDistance = _field.GetDistance(next.X, next.Y);

                // 나보다 가까운 칸만 후보다.
                if (neighborDistance == FlowField.Unreachable || neighborDistance >= ownDistance) continue;

                Vector2 step = ((Vector2)_room.CellCenterWorld(next) - from).normalized;
                float alignment = Vector2.Dot(step, straight);

                if (neighborDistance > bestDistance) continue;

                // 거리가 같으면 직선에 가까운 쪽을 고른다. 기준이 흔들리면 적이 두 칸 사이에서 떤다.
                if (neighborDistance == bestDistance && alignment <= bestAlignment) continue;

                bestCell = next;
                bestDistance = neighborDistance;
                bestAlignment = alignment;
            }

            // 플레이어와 같은 칸이거나 이웃이 모두 더 멀다. 그때는 부르는 쪽이 직선으로 간다.
            if (bestCell == cell) return false;

            point = _room.CellCenterWorld(bestCell);
            return true;
        }
    }
}
