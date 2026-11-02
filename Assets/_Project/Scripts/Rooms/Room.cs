using System;
using JM2D.Logic.Rooms;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

namespace JM2D.Rooms
{
    /// 방 하나. 문과 바닥 색과 방 카메라를 들고 있다.
    /// 방 크기는 벽 Tilemap 이 칠한 범위에서 읽는다. 벽을 다시 칠하면 방 간격과 카메라 경계가 따라 바뀐다.
    public class Room : MonoBehaviour
    {
        [Header("바닥 색")]
        [SerializeField] private Color _startColor = new Color(0.16f, 0.22f, 0.3f);
        [SerializeField] private Color _combatColor = new Color(0.18f, 0.18f, 0.2f);
        [SerializeField] private Color _bossColor = new Color(0.3f, 0.14f, 0.14f);

        [SerializeField] private Tilemap _walls;

        [Tooltip("바닥 타일맵. 모서리와 가장자리와 가운데가 다른 타일이라 한 장으로 늘릴 수 없다")]
        [SerializeField] private Tilemap _floor;

        [Header("문")]
        [SerializeField] private GameObject _doorNorth;
        [SerializeField] private GameObject _doorEast;
        [SerializeField] private GameObject _doorSouth;
        [SerializeField] private GameObject _doorWest;

        [Header("카메라")]
        [SerializeField] private CinemachineCamera _camera;
        [Tooltip("카메라가 비출 수 있는 범위. 모양만 읽히도록 꺼 둔다. 켜면 투사체가 닿아 사라진다.")]
        [SerializeField] private BoxCollider2D _cameraBounds;

        [Tooltip("위 범위로 카메라를 가두는 쪽. 시야 크기를 바꿀 때마다 다시 계산하게 해야 한다")]
        [SerializeField] private CinemachineConfiner2D _confiner;

        [Tooltip("보스 방에서 전투가 시작될 때 시야가 방 전체로 넓어지는 데 걸리는 시간")]
        [Min(0.01f)]
        [SerializeField] private float _bossZoomTime = 0.5f;

        [Header("적 출현 지점")]
        [Tooltip("방 안에 흩어 둔다. 플레이어와 가까운 지점은 건너뛴다")]
        [SerializeField] private Transform[] _spawnPoints;

        private RoomType _type;
        private RoomCell _cell;
        private RoomState _state;

        /// 프리팹에 넣어 둔 평소 시야. 보스 방을 깨면 여기로 돌아온다.
        private float _normalCameraSize;

        /// 보스 전투 동안 방 가운데를 보게 바꾼 뒤 되돌리려고 기억한다.
        private Transform _followTarget;

        /// 옆방과 이어진 변. DoorSide 의 순서(북, 동, 남, 서)로 들어 있다.
        private readonly bool[] _isConnected = new bool[4];

        /// 방의 가로, 세로 칸 수. 벽 Tilemap 의 칠한 범위이고 타일을 지워도 줄지 않는다.
        /// 벽을 다시 칠했으면 Walls 에서 Compress Tilemap Bounds 를 실행한다.
        public Vector2Int Size => (Vector2Int)_walls.cellBounds.size;

        public RoomType Type => _type;
        public RoomCell Cell => _cell;
        public RoomState State => _state;

        /// 벽 두께. 벽을 한 칸 두께로 칠한다는 방 프리팹의 약속에 기댄다.
        public float WallThickness => _walls.layoutGrid.cellSize.x;
        public IReadOnlyList<Transform> SpawnPoints => _spawnPoints;

        private void Awake()
        {
            // 방 크기를 두 곳에 적지 않는다.
            _cameraBounds.size = Size;

            _normalCameraSize = _camera.Lens.OrthographicSize;
        }

        /// 방을 놓은 직후 한 번 부른다.
        public void Setup(RoomCell cell, RoomType type)
        {
            _cell = cell;
            _type = type;
            _floor.color = GetColor(type);
            _state = type == RoomType.Start ? RoomState.Cleared : RoomState.NotStarted;
        }

        public void SetConnected(DoorSide side, bool isConnected)
        {
            _isConnected[(int)side] = isConnected;
            SetDoorOpen(side, isConnected);
        }

        /// 이어지지 않은 변은 이미 닫혀 있어 넷을 다 닫아도 같다.
        public void BeginCombat()
        {
            _state = RoomState.Fighting;

            for (int i = 0; i < _isConnected.Length; i++)
                SetDoorOpen((DoorSide)i, false);

            // 넓히는 동안 플레이어를 쫓게 두면 Composer 와 Confiner 가 서로 당겨 다툰다.
            // 이 오브젝트의 자리가 방 가운데다.
            if (_type == RoomType.Boss)
            {
                _camera.Follow = transform;
                StartZoom(FitSize());
            }
        }

        /// 방 전체가 화면에 들어오는 시야 크기.
        /// OrthographicSize 는 세로 반범위라 축마다 꼭 맞는 값이 다르다. 작은 쪽을 쓴다.
        /// 큰 쪽은 다른 축이 옆방까지 넘친다.
        /// 벽 안쪽으로 반 칸 당기는 것은, 딱 맞추면 Confiner 가 가둘 자리가 0 이 되어
        /// 다른 모양으로 갈아타며 카메라가 한 프레임 튀기 때문이다.
        private float FitSize()
        {
            float width = Size.x - WallThickness;
            float height = Size.y - WallThickness;

            float forHeight = height / 2f;
            float forWidth = width / 2f / _camera.Lens.Aspect;

            return Mathf.Min(forHeight, forWidth);
        }

        /// 이 클래스의 코루틴은 이것뿐이라 StopAllCoroutines 로 앞의 것을 멈춘다.
        private void StartZoom(float to)
        {
            StopAllCoroutines();
            StartCoroutine(ZoomCamera(to));
        }

        /// Brain 의 블렌드는 카메라가 바뀔 때만 돌아 같은 카메라의 크기 변화는 대상이 아니다.
        /// 그래서 직접 보간하고, 바꿀 때마다 Confiner 에 알린다.
        /// 알리지 않으면 구워 둔 폴리곤이 남아 바뀌기 전 크기로 카메라를 가둔다.
        private IEnumerator ZoomCamera(float to)
        {
            float from = _camera.Lens.OrthographicSize;
            float left = _bossZoomTime;

            while (left > 0f)
            {
                left -= Time.deltaTime;
                SetCameraSize(Mathf.Lerp(to, from, Mathf.Clamp01(left / _bossZoomTime)));

                yield return null;
            }

            SetCameraSize(to);
        }

        private void SetCameraSize(float size)
        {
            _camera.Lens.OrthographicSize = size;
            _confiner.InvalidateLensCache();
        }

        public void EndCombat()
        {
            _state = RoomState.Cleared;

            for (int i = 0; i < _isConnected.Length; i++)
                SetDoorOpen((DoorSide)i, _isConnected[i]);

            if (_type == RoomType.Boss)
            {
                _camera.Follow = _followTarget;
                StartZoom(_normalCameraSize);
            }
        }

        /// 켜진 카메라가 바뀌면 메인 카메라의 CinemachineBrain 이 블렌드한다.
        public void ShowCamera(Transform target)
        {
            _followTarget = target;
            _camera.Follow = target;
            _camera.enabled = true;
        }

        /// 꺼진 카메라는 계산하지 않는다. 블렌드는 꺼지기 직전의 모습에서 시작한다.
        public void HideCamera()
        {
            _camera.enabled = false;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
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
