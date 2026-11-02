using System;
using System.Collections.Generic;
using JM2D.Combat;
using JM2D.Core;
using JM2D.Data;
using JM2D.Enemy;
using JM2D.Feedback;
using JM2D.Logic.Rooms;
using JM2D.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace JM2D.Rooms
{
    /// 방의 전투를 진행한다. 들어서면 문을 닫고 웨이브를 부르고, 마지막 웨이브를 잡으면 방을 연다.
    /// 한 번에 한 방만 싸우므로 씬에 하나만 둔다.
    public class RoomCombat : MonoBehaviour
    {
        /// 방의 마지막 웨이브를 깼을 때 알린다. 클리어 화면과 보상이 듣는다.
        public event Action<Room> OnRoomCleared;

        /// 방의 첫 웨이브가 나올 때 알린다. 가방 잠금이 듣는다.
        public event Action<Room> OnCombatStarted;

        [Tooltip("몸이 문 밖으로 다 나온 뒤 몇 칸 더 들어서야 전투가 시작되나")]
        [Min(0f)]
        [SerializeField] private float _extraMargin = 1.5f;

        [Tooltip("플레이어에게서 이보다 가까운 출현 지점은 건너뛴다")]
        [Min(0f)]
        [SerializeField] private float _minSpawnDistance = 5f;

        [Tooltip("전투 방은 이 중에서 시드로 하나 고른다")]
        [SerializeField] private EncounterData[] _encounters;

        [Tooltip("보스 방 전용. 보스가 들어올 때까지 쓰는 강한 조합")]
        [SerializeField] private EncounterData _bossEncounter;

        [Tooltip("판 도중에 태어난 원거리 적에게 넣어 준다. 씬의 EnemyProjectilePool")]
        [SerializeField] private ProjectilePool _enemyProjectiles;

        [SerializeField] private LevelBuilder _level;
        [SerializeField] private RoomTracker _tracker;
        [SerializeField] private Collider2D _player;
        [SerializeField] private EnemyCounter _enemyCounter;

        [Tooltip("적이 장애물을 돌아가게 한다. 씬의 RoomFlowField")]
        [SerializeField] private RoomFlowField _flowField;

        [Tooltip("보스가 나올 때 붙인다. 씬의 BossHealthBar")]
        [SerializeField] private BossHealthBar _bossBar;

        [Tooltip("판 도중에 태어난 적에게 넣어 준다. 씬의 HitFeedback")]
        [FormerlySerializedAs("_hitStop")]
        [SerializeField] private HitFeedback _hitFeedback;

        /// 들어섰지만 아직 전투가 시작되지 않은 방. 없으면 null.
        private Room _waiting;

        private Room _fighting;

        private int _waveIndex;

        /// 방 바깥 경계에서 잰 거리. 이만큼 들어와야 전투가 시작된다.
        private float _margin;

        /// 이번 웨이브에 쓸 출현 지점. 매 웨이브 다시 고른다.
        private readonly List<Transform> _points = new List<Transform>();

        /// 지점을 어디서부터 쓰기 시작할지. 매 웨이브 바뀐다.
        private int _startIndex;

        private EncounterData _current;

        /// 배치를 만든 난수와 별개이고 시드만 같다.
        /// System.Random 이라 UnityEngine.Random 을 쓰는 다른 코드와 섞이지 않는다.
        private System.Random _encounterRandom;

        private void OnEnable()
        {
            _tracker.OnRoomEntered += OnRoomEntered;
            _enemyCounter.OnAllEnemiesDead += OnWaveCleared;
        }

        // LevelBuilder 가 Awake 에서 시드를 정한다. 그 뒤라야 읽을 수 있어 Start 에서 한다.
        private void Start()
        {
            _encounterRandom = new System.Random(_level.CurrentSeed);
        }

        private void Update()
        {
            if (_waiting == null) return;

            Vector3 position = _player.transform.position;
            Vector2Int size = _level.RoomSize;

            if (!RoomSpace.IsInside(_waiting.Cell, position.x, position.y, size.x, size.y, _margin)) return;

            Begin(_waiting);
        }

        private void OnDisable()
        {
            _tracker.OnRoomEntered -= OnRoomEntered;
            _enemyCounter.OnAllEnemiesDead -= OnWaveCleared;
        }

        /// 입장할 때마다 기다릴 방을 새로 정한다. 싸울 방이 아니면 비운다.
        private void OnRoomEntered(Room room)
        {
            _waiting = room.State == RoomState.NotStarted ? room : null;

            // 벽 두께에 몸 반폭을 더한 만큼 들어와야 몸이 문 밖으로 다 나온다. 큰 쪽을 써서 정사각형이 아니어도 걸리지 않는다.
            Vector3 extents = _player.bounds.extents;
            float bodyHalf = Mathf.Max(extents.x, extents.y);
            _margin = room.WallThickness + bodyHalf + _extraMargin;
        }

        private void Begin(Room room)
        {
            _waiting = null;
            _fighting = room;
            _waveIndex = 0;
            _current = Pick(room);

            room.BeginCombat();
            SpawnWave(_waveIndex);
            OnCombatStarted?.Invoke(room);
        }

        /// 보스 방은 전용 조합, 전투 방은 뽑기.
        private EncounterData Pick(Room room)
        {
            if (room.Type == RoomType.Boss) return _bossEncounter;

            return _encounters[_encounterRandom.Next(_encounters.Length)];
        }

        /// 다음 웨이브가 있으면 부르고, 없으면 방을 연다.
        private void OnWaveCleared()
        {
            if (_fighting == null) return;

            _waveIndex++;

            if (_waveIndex >= _current.Waves.Count)
            {
                _fighting.EndCombat();

                // 상태를 먼저 정리하고 알린다. 듣는 쪽이 여기서 다시 건드려도 싸우는 중으로 보이지 않는다.
                Room cleared = _fighting;
                _fighting = null;
                OnRoomCleared?.Invoke(cleared);
                return;
            }

            SpawnWave(_waveIndex);
        }

        private void SpawnWave(int index)
        {
            CollectPoints();

            int spawned = 0;

            foreach (WaveEntry entry in _current.Waves[index].Entries)
            {
                for (int i = 0; i < entry.Count; i++)
                {
                    Vector2 position = SpawnPosition(entry.Prefab, spawned);
                    EnemyBase enemy = Instantiate(entry.Prefab, position, Quaternion.identity);
                    enemy.SetTarget(_player.transform);
                    enemy.SetPathField(_flowField);

                    // 맞는 반응도 씬의 HitFeedback 이 필요하다. 쏘는 적의 풀과 같은 이유다.
                    enemy.SetHitFeedback(_hitFeedback);

                    // 쏘는 적은 씬의 풀이 필요하다. 프리팹이 씬을 가리킬 수 없어 여기서 넣는다.
                    if (enemy is IProjectileShooter shooter)
                        shooter.SetProjectilePool(_enemyProjectiles);

                    // 체력 바도 씬에 있어 보스가 태어날 때 붙여 준다.
                    if (enemy is Boss boss)
                        _bossBar.Bind(boss);

                    spawned++;
                }
            }
        }

        /// 보스는 방 가운데에서 나온다. 구석에서 나오면 등장이 우연처럼 보인다.
        private Vector2 SpawnPosition(EnemyBase prefab, int spawned)
        {
            if (prefab is Boss)
            {
                Vector2Int size = _level.RoomSize;
                RoomSpace.ToWorld(_fighting.Cell, size.x, size.y, out float x, out float y);
                return new Vector2(x, y);
            }

            return _points[(_startIndex + spawned) % _points.Count].position;
        }

        /// 이번 웨이브에 쓸 지점을 고른다. 플레이어와 가까운 곳은 건너뛴다.
        private void CollectPoints()
        {
            _points.Clear();

            Vector2 player = _player.transform.position;
            IReadOnlyList<Transform> all = _fighting.SpawnPoints;

            foreach (Transform point in all)
            {
                if (Vector2.Distance(player, point.position) >= _minSpawnDistance)
                    _points.Add(point);
            }

            // 전부 걸러졌으면 가장 먼 한 곳이라도 쓴다.
            if (_points.Count == 0)
                _points.Add(Farthest(all, player));

            // 자리는 판마다 달라도 되는 값이라 시드 난수를 쓰지 않는다. 같은 파일에 둘이 있어 끝까지 적는다.
            _startIndex = UnityEngine.Random.Range(0, _points.Count);
        }

        private static Transform Farthest(IReadOnlyList<Transform> points, Vector2 from)
        {
            Transform best = points[0];
            float bestDistance = -1f;

            foreach (Transform point in points)
            {
                float distance = Vector2.Distance(from, point.position);

                if (distance <= bestDistance) continue;

                best = point;
                bestDistance = distance;
            }
            return best;
        }
    }
}
