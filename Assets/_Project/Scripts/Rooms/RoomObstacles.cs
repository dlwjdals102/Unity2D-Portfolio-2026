using System.Collections.Generic;
using JM2D.Logic.Rooms;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JM2D.Rooms
{
    /// 방 하나의 장애물. 판이 시작될 때 시드로 자리를 정해 타일을 칠한다.
    /// 어디에 놓을지는 Logic 의 ObstaclePlacer 가 정한다. 여기서는 격자를 만들어 넘기고, 돌려받은 칸을 칠한다.
    ///
    /// 격자 칸은 0 부터 세고(0 ~ 31, 0 ~ 17) 타일 칸은 방 가운데가 0 이다(-16 ~ 15, -9 ~ 8).
    /// 둘 사이를 옮기는 곳은 ToTilemapCell 과 ToGridCell 두 곳뿐이다.
    [RequireComponent(typeof(Room))]
    public class RoomObstacles : MonoBehaviour
    {
        [SerializeField] private Tilemap _walls;
        [SerializeField] private Tilemap _obstacles;

        [Tooltip("장애물로 칠할 타일. 벽과 같은 것을 쓴다")]
        [SerializeField] private TileBase _tile;

        [Tooltip("문 구멍에서 안쪽으로 이만큼 비워 둔다")]
        [Min(0)]
        [SerializeField] private int _doorClearance = 3;

        [Tooltip("출현 지점 둘레로 이만큼 비워 둔다")]
        [Min(0)]
        [SerializeField] private int _spawnClearance = 1;

        private Room _room;
        private RoomGrid _grid;

        /// 이 방의 칸 격자. 벽과 장애물이 막힌 칸이다. 흐름장(계단 4)이 읽는다.
        public RoomGrid Grid => _grid;

        private void Awake()
        {
            _room = GetComponent<Room>();
        }

        /// 방을 놓은 뒤 한 번 부른다. 자리를 정하고 타일을 칠한다.
        public void Build(System.Random random)
        {
            _grid = BuildWallGrid();

            List<RoomCell> doors = FindDoorCells();
            List<RoomCell> spawns = SpawnCells();

            var mustReach = new List<RoomCell>(doors);
            mustReach.AddRange(spawns);

            List<RoomCell> cells = ObstaclePlacer.Place(_grid, KeepClear(doors, spawns), mustReach, random);

            foreach (RoomCell cell in cells)
            {
                _grid.Block(cell.X, cell.Y);
                _obstacles.SetTile(ToTilemapCell(cell), _tile);
            }
        }

        /// 벽 Tilemap 을 훑어 격자를 만든다. 타일이 있는 칸이 막힌 칸이다.
        private RoomGrid BuildWallGrid()
        {
            BoundsInt bounds = _walls.cellBounds;
            var grid = new RoomGrid(bounds.size.x, bounds.size.y);

            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                {
                    var cell = new RoomCell(x, y);

                    if (_walls.HasTile(ToTilemapCell(cell))) grid.Block(x, y);
                }

            return grid;
        }

        /// 가장자리인데 벽 타일이 없는 칸이 문 구멍이다.
        /// 문 폭이나 가운데 좌표를 코드에 적지 않아, 프리팹에서 문을 넓히면 따라온다.
        private List<RoomCell> FindDoorCells()
        {
            var cells = new List<RoomCell>();

            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                {
                    bool isEdge = x == 0 || y == 0 || x == _grid.Width - 1 || y == _grid.Height - 1;

                    if (isEdge && _grid.IsPassable(x, y)) cells.Add(new RoomCell(x, y));
                }

            return cells;
        }

        /// 방 프리팹의 출현 지점을 칸 좌표로 바꾼다.
        private List<RoomCell> SpawnCells()
        {
            var cells = new List<RoomCell>();

            foreach (Transform point in _room.SpawnPoints)
                cells.Add(ToGridCell(_walls.WorldToCell(point.position)));

            return cells;
        }

        /// 비워 둘 칸. 문에서 안쪽으로 몇 칸, 출현 지점 둘레 몇 칸이다.
        private List<RoomCell> KeepClear(List<RoomCell> doors, List<RoomCell> spawns)
        {
            var cells = new List<RoomCell>();

            foreach (RoomCell door in doors)
            {
                // 문이 어느 변에 있는지 보고 방 안쪽 방향을 정한다.
                int stepX = door.X == 0 ? 1 : door.X == _grid.Width - 1 ? -1 : 0;
                int stepY = door.Y == 0 ? 1 : door.Y == _grid.Height - 1 ? -1 : 0;

                for (int step = 0; step <= _doorClearance; step++)
                    cells.Add(new RoomCell(door.X + stepX * step, door.Y + stepY * step));
            }

            foreach (RoomCell spawn in spawns)
                for (int dy = -_spawnClearance; dy <= _spawnClearance; dy++)
                    for (int dx = -_spawnClearance; dx <= _spawnClearance; dx++)
                        cells.Add(new RoomCell(spawn.X + dx, spawn.Y + dy));

            return cells;
        }

        /// 격자 칸(0 부터 셈) 을 타일 칸(가운데가 0) 으로.
        private Vector3Int ToTilemapCell(RoomCell cell)
        {
            BoundsInt bounds = _walls.cellBounds;

            return new Vector3Int(cell.X + bounds.xMin, cell.Y + bounds.yMin, 0);
        }

        /// 타일 칸을 격자 칸으로. 위의 반대다.
        private RoomCell ToGridCell(Vector3Int tilemapCell)
        {
            BoundsInt bounds = _walls.cellBounds;

            return new RoomCell(tilemapCell.x - bounds.xMin, tilemapCell.y - bounds.yMin);
        }

        /// 월드 위치가 이 방의 어느 칸인가. 좌표를 옮기는 곳은 이 클래스 안에만 둔다.
        public RoomCell ToGridCell(Vector3 worldPosition)
        {
            return ToGridCell(_walls.WorldToCell(worldPosition));
        }

        /// 칸 가운데의 월드 위치. 디버그 표시와 적이 걸어갈 자리에 쓴다.
        public Vector3 CellCenterWorld(RoomCell cell)
        {
            return _walls.GetCellCenterWorld(ToTilemapCell(cell));
        }
    }
}
