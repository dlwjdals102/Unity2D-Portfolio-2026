using System;
using JM2D.Logic.Rooms;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

namespace JM2D.Rooms
{
    /// 방 하나. 네 변의 문을 열고 닫고, 방 종류에 따라 바닥 색과 카메라 크기를 정하고, 방의 카메라를 켜고 끈다.
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

        /// 평소 시야. 보스 방을 깨면 여기로 돌아온다. 인스펙터 값이라 코드가 숫자를 들고 있지 않는다.
        private float _normalCameraSize;

        /// 평소에 카메라가 따라갈 대상. 보스 전투 동안 방 가운데를 보게 바꾼 뒤 되돌리려고 기억한다.
        private Transform _followTarget;

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

            _normalCameraSize = _camera.Lens.OrthographicSize;
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

            // 보스가 태어나는 것과 같은 프레임이다. 둘이 함께 가면 한 연출로 읽힌다.
            //
            // 시야가 방 전체가 되는 동안 카메라는 플레이어를 쫓지 않고 방 가운데를 본다.
            // 이 오브젝트의 자리가 곧 방 가운데다. LevelBuilder 가 거기에 방을 놓는다.
            // 쫓게 두면 Composer 는 플레이어 쪽으로, Confiner 는 가운데로 당겨 둘이 다툰다.
            // 보여 주려는 것이 방 전체이므로 가운데를 보는 것이 연출과도 맞는다.
            if (_type == RoomType.Boss)
            {
                _camera.Follow = transform;
                StartZoom(FitSize());
            }
        }

        /// 방 전체가 화면에 들어오는 시야 크기.
        ///
        /// OrthographicSize 는 세로로 보이는 범위의 반이고, 가로로 보이는 반폭은
        /// 거기에 화면 비율을 곱한 값이다. 그래서 축마다 꼭 맞는 크기가 다르다.
        /// 두 축이 모두 방 안에 들어오는 것은 작은 쪽이다. 큰 쪽을 쓰면
        /// 다른 축이 방 밖으로 넘치고, 방은 서로 붙어 있어 그 자리가 옆방이다.
        ///
        /// 비율은 켜진 카메라가 출력 카메라에서 받아 둔 값이라 창 크기를 따라간다.
        ///
        /// 방에 딱 맞추지 않고 벽 안쪽으로 반 칸씩 당긴다. 딱 맞추면 Confiner 가
        /// 카메라를 세워 둘 자리의 넓이가 0 이 되고, 그러면 수축으로 얻은 모양 대신
        /// 미리 계산해 둔 다른 모양으로 갈아타며 카메라가 한 프레임에 튄다.
        /// 잃는 것은 벽 타일의 바깥 반쪽이라 방 안은 그대로 다 보인다.
        private float FitSize()
        {
            float width = Size.x - WallThickness;
            float height = Size.y - WallThickness;

            float forHeight = height / 2f;
            float forWidth = width / 2f / _camera.Lens.Aspect;

            return Mathf.Min(forHeight, forWidth);
        }

        /// 넓어지던 중에 전투가 끝나면 앞의 것을 멈춘다. 이 클래스의 코루틴은 이것뿐이다.
        private void StartZoom(float to)
        {
            StopAllCoroutines();
            StartCoroutine(ZoomCamera(to));
        }

        /// 시야를 to 로 옮긴다. 보스가 화면 밖에서 쏘던 것이 이것으로 풀린다.
        ///
        /// Brain 의 블렌드는 카메라가 바뀔 때만 돌아, 같은 카메라의 크기를 바꾸는 것은
        /// 블렌드 대상이 아니다. 그래서 여기서 직접 보간한다.
        ///
        /// 크기를 바꿀 때마다 Confiner 에게 알린다. Confiner 는 효율을 위해 구워 둔
        /// 폴리곤을 다시 계산하지 않아, 알리지 않으면 바뀌기 전 크기로 카메라를 가둔다.
        /// 넓힐 때는 가두는 범위가 남아돌아 방 밖이 보이고, 좁힐 때는 모자라
        /// 카메라가 플레이어를 따라가지 못한다.
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

        /// 마지막 웨이브까지 잡으면 부른다. 이어진 변의 문만 다시 연다.
        public void EndCombat()
        {
            _state = RoomState.Cleared;

            for (int i = 0; i < _isConnected.Length; i++)
                SetDoorOpen((DoorSide)i, _isConnected[i]);

            // 보스를 잡으면 시야와 따라갈 대상이 함께 평소로 돌아온다.
            // 넓은 채로 두면 클리어 화면까지 어색하다.
            if (_type == RoomType.Boss)
            {
                _camera.Follow = _followTarget;
                StartZoom(_normalCameraSize);
            }
        }

        /// 이 방의 카메라를 켜고 대상을 따라가게 한다.
        /// 켜진 카메라로 바뀌면 메인 카메라의 CinemachineBrain 이 블렌드한다.
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

        /// 코루틴은 시작한 쪽이 치운다. 넓어지던 중에 씬이 바뀌어도 남지 않는다.
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
