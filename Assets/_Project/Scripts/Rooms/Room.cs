using System;
using JM2D.Logic.Rooms;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace JM2D.Rooms
{
    /// 방 하나. 네 변의 문을 열고 닫고, 방 종류에 따라 바닥 색을 바꾸고, 방의 카메라를 켜고 끈다.
    /// 제 칸과 전투 상태를 알고, 어느 변이 옆방과 이어졌는지 기억한다.
    /// 문 오브젝트가 켜져 있으면 닫힌 문이고 벽처럼 길을 막는다.
    ///
    /// 방 크기는 벽 Tilemap 이 칠한 범위에서 읽는다. 코드나 인스펙터에 따로 적지 않아
    /// 벽을 다시 칠하면 방 간격과 카메라 경계도 따라 바뀐다.
    public class Room : MonoBehaviour
    {
        [Header("바닥 색")]
        [SerializeField] private Color _startColor = new Color(0.16f, 0.22f, 0.3f);
        [SerializeField] private Color _combatColor = new Color(0.18f, 0.18f, 0.2f);
        [SerializeField] private Color _bossColor = new Color(0.3f, 0.14f, 0.14f);

        [SerializeField] private Tilemap _walls;
        [SerializeField] private SpriteRenderer _floor;

        [Header("문")]
        [SerializeField] private GameObject _doorNorth;
        [SerializeField] private GameObject _doorEast;
        [SerializeField] private GameObject _doorSouth;
        [SerializeField] private GameObject _doorWest;

        [Header("카메라")]
        [SerializeField] private CinemachineCamera _camera;
        [Tooltip("카메라가 비출 수 있는 범위. 모양만 읽히도록 꺼 둔다. 켜면 투사체가 닿아 사라진다.")]
        [SerializeField] private BoxCollider2D _cameraBounds;

        [Header("적 출현 지점")]
        [Tooltip("방 안에 흩어 둔다. 플레이어와 가까운 지점은 건너뛴다")]
        [SerializeField] private Transform[] _spawnPoints;

        private RoomType _type;
        private RoomCell _cell;
        private RoomState _state;

        /// 옆방과 이어진 변. DoorSide 의 순서(북, 동, 남, 서)로 들어 있다.
        private readonly bool[] _isConnected = new bool[4];

        /// 방의 가로, 세로 칸 수. 벽 Tilemap 이 기억하는 칠한 범위다.
        /// 이 범위는 타일을 지워도 줄지 않는다. 벽을 다시 칠했으면
        /// Walls 를 우클릭해 Compress Tilemap Bounds 를 실행한다.
        public Vector2Int Size => (Vector2Int)_walls.cellBounds.size;

        public RoomType Type => _type;
        public RoomCell Cell => _cell;
        public RoomState State => _state;

        /// 벽 두께. 벽은 한 칸 두께로 칠한다는 것이 방 프리팹의 약속이고, 한 칸의 크기는 Grid 에서 읽는다.
        /// 벽을 두 칸으로 칠하면 이 약속을 고친다.
        public float WallThickness => _walls.layoutGrid.cellSize.x;
        public IReadOnlyList<Transform> SpawnPoints => _spawnPoints;

        private void Awake()
        {
            // 카메라 경계를 벽 범위에 맞춘다. 방 크기를 두 곳에 적지 않기 위해서다.
            _cameraBounds.size = Size;
        }

        /// 방을 놓은 직후 한 번 부른다. 칸, 종류, 처음 상태를 정한다.
        public void Setup(RoomCell cell, RoomType type)
        {
            _cell = cell;
            _type = type;
            _floor.color = GetColor(type);
            _state = type == RoomType.Start ? RoomState.Cleared : RoomState.NotStarted;
        }

        /// 방을 놓을 때 부른다. 이어진 변은 기억해 두고 문을 연다.
        public void SetConnected(DoorSide side, bool isConnected)
        {
            _isConnected[(int)side] = isConnected;
            SetDoorOpen(side, isConnected);
        }

        /// 플레이어가 들어서면 부른다. 문을 모두 닫는다.
        /// 이어지지 않은 변은 이미 닫혀 있어 넷을 다 닫아도 같다.
        public void BeginCombat()
        {
            _state = RoomState.Fighting;

            for (int i = 0; i < _isConnected.Length; i++)
                SetDoorOpen((DoorSide)i, false);
        }

        /// 마지막 웨이브까지 잡으면 부른다. 이어진 변의 문만 다시 연다.
        public void EndCombat()
        {
            _state = RoomState.Cleared;

            for (int i = 0; i < _isConnected.Length; i++)
                SetDoorOpen((DoorSide)i, _isConnected[i]);
        }

        /// 이 방의 카메라를 켜고 대상을 따라가게 한다.
        /// 켜진 카메라로 바뀌면 메인 카메라의 CinemachineBrain 이 블렌드한다.
        public void ShowCamera(Transform target)
        {
            _camera.Follow = target;
            _camera.enabled = true;
        }

        /// 꺼진 카메라는 계산하지 않는다. 블렌드는 꺼지기 직전의 모습에서 시작한다.
        public void HideCamera()
        {
            _camera.enabled = false;
        }

        /// 연 문은 오브젝트를 끄고, 닫은 문은 켠다.
        private void SetDoorOpen(DoorSide side, bool isOpen)
        {
            GetDoor(side).SetActive(!isOpen);
        }

        private GameObject GetDoor(DoorSide side)
        {
            switch (side)
            {
                case DoorSide.North: return _doorNorth;
                case DoorSide.East: return _doorEast;
                case DoorSide.South: return _doorSouth;
                case DoorSide.West: return _doorWest;
                default: throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        private Color GetColor(RoomType type)
        {
            switch (type)
            {
                case RoomType.Start: return _startColor;
                case RoomType.Combat: return _combatColor;
                case RoomType.Boss: return _bossColor;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
