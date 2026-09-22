using System.Collections.Generic;
using JM2D.Logic.Rooms;
using UnityEngine;

namespace JM2D.Rooms
{
    /// 판이 시작될 때 방 배치를 만들고 방 여덟 개를 씬에 놓는다.
    /// 다른 컴포넌트는 Start 부터 배치와 방을 읽을 수 있다.
    public class LevelBuilder : MonoBehaviour
    {
        [Header("시드")]
        [SerializeField] private bool _useFixedSeed;
        [SerializeField] private int _fixedSeed;
        [Tooltip("이번 판에 쓴 시드. 플레이 중에 확인하려고 둔다.")]
        [SerializeField] private int _currentSeed;

        [SerializeField] private Room _roomPrefab;

        private static readonly DoorSide[] Sides =
            { DoorSide.North, DoorSide.East, DoorSide.South, DoorSide.West };

        private RoomLayout _layout;
        private Vector2Int _roomSize;
        private readonly List<Room> _rooms = new List<Room>();

        public RoomLayout Layout => _layout;
        public Vector2Int RoomSize => _roomSize;
        public int CurrentSeed => _currentSeed;

        private void Awake()
        {
            _currentSeed = _useFixedSeed ? _fixedSeed : Random.Range(0, int.MaxValue);
            _layout = RoomLayoutGenerator.Generate(_currentSeed);

            _roomSize = _roomPrefab.Size;
            Debug.Assert(_roomSize.x % 2 == 0 && _roomSize.y % 2 == 0,
                "방 크기가 짝수가 아니다. Walls 의 Compress Tilemap Bounds 를 확인한다.");

            // 장애물 자리를 뽑는 난수. 배치와 조합을 뽑는 난수와 별개이고 시드만 같다.
            var obstacleRandom = new System.Random(_currentSeed);

            for (int i = 0; i < _layout.Rooms.Count; i++)
            {
                RoomCell cell = _layout.Rooms[i];

                RoomSpace.ToWorld(cell, _roomSize.x, _roomSize.y, out float x, out float y);
                Room room = Instantiate(_roomPrefab, new Vector2(x, y), Quaternion.identity, transform);
                room.name = $"Room {i} {cell}";
                room.Setup(cell, _layout.GetRoomType(cell));

                // 옆방과 이어진 변의 문만 연다. 나머지는 닫힌 채 벽이 된다.
                foreach (DoorSide side in Sides)
                    room.SetConnected(side, _layout.HasDoor(cell, side));

                if (room.Type != RoomType.Start)
                    room.GetComponent<RoomObstacles>().Build(obstacleRandom);

                _rooms.Add(room);
            }
        }

        /// 그 칸에 놓인 방. 방은 배치의 순번대로 들어 있다.
        public Room GetRoom(RoomCell cell) => _rooms[_layout.IndexOf(cell)];
    }
}
