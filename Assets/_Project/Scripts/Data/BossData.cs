using JM2D.Enemy;
using UnityEngine;

namespace JM2D.Data
{
    /// 패턴 하나가 지나가는 세 단계의 시간과 색.
    /// 보스의 패턴들이 예고, 발동, 회복이라는 같은 모양이라 이 묶음을 함께 쓴다.
    [System.Serializable]
    public class PatternSteps
    {
        [Tooltip("멈춰서 예고하는 시간")]
        public float WindupTime = 0.5f;

        [Tooltip("발동이 이어지는 시간. 한 번에 끝나는 패턴은 색이 보일 만큼만 준다")]
        public float ActTime = 0.15f;

        [Tooltip("발동 뒤 쉬는 시간. 이 동안 플레이어를 쫓는다")]
        public float RecoverTime = 1f;

        [Tooltip("예고 중의 몸 색. 패턴마다 달라야 무엇이 오는지 읽힌다")]
        public Color WindupColor = Color.yellow;

        [Tooltip("발동 중의 몸 색")]
        public Color ActColor = Color.white;
    }

    /// 휩쓸기. 몸에 붙은 플레이어를 벌하는 패턴이다.
    [System.Serializable]
    public class SweepPattern
    {
        public PatternSteps Steps = new PatternSteps();

        [Tooltip("플레이어가 이 거리 안이면 이 패턴을 쓴다")]
        public float Range = 2.5f;

        [Tooltip("둘레 이 반경 안에 든 대상을 친다")]
        public float HitRadius = 2.5f;

        public int Damage = 1;
    }

    /// 돌진. 예고가 시작될 때의 플레이어 방향으로 곧게 달린다.
    [System.Serializable]
    public class DashPattern
    {
        public PatternSteps Steps = new PatternSteps();

        [Tooltip("플레이어가 이 거리 밖이어야 이 패턴을 쓴다. 너무 붙으면 지나쳐 버린다")]
        public float MinRange = 2f;

        [Tooltip("이 거리 안이어야 쓴다. 더 멀면 달려도 닿지 않아 사격에 넘긴다")]
        public float MaxRange = 10f;

        public float Speed = 12f;

        [Tooltip("달리는 동안 이 반경 안에 든 대상을 한 번만 친다")]
        public float ContactRadius = 1.2f;

        public int Damage = 1;
    }

    /// 부채꼴 사격. 플레이어 쪽을 가운데 두고 여러 발을 한 번에 뿌린다.
    [System.Serializable]
    public class FanPattern
    {
        public PatternSteps Steps = new PatternSteps();

        [Tooltip("플레이어가 이 거리 밖이어야 이 패턴을 쓴다. 최대는 두지 않는다")]
        public float MinRange = 6f;

        [Tooltip("한 번에 나가는 탄 수")]
        [Min(1)]
        public int ShotCount = 3;

        [Tooltip("첫 발과 마지막 발 사이의 전체 각도")]
        public float SpreadDegrees = 30f;

        public int Damage = 1;
    }

    /// 잡몹 소환. 보스 둘레에 작은 적을 고르게 놓는다.
    /// Prefab 이 비어 있으면 그 페이즈에서는 쓰지 않는다. 어느 페이즈에 무엇이 있는지를 데이터가 정한다.
    [System.Serializable]
    public class SummonPattern
    {
        public PatternSteps Steps = new PatternSteps();

        [Tooltip("소환할 적. 비워 두면 이 페이즈에서는 소환하지 않는다")]
        public EnemyBase Prefab;

        [Min(1)]
        public int Count = 3;

        [Tooltip("보스 중심에서 이만큼 떨어뜨려 놓는다. 보스 반지름보다 커야 몸 안에서 태어나지 않는다")]
        public float SpreadRadius = 1.5f;

        [Tooltip("살아 있는 소환물이 이만큼 있으면 이 차례를 건너뛴다")]
        [Min(1)]
        public int MaxAlive = 4;
    }

    /// 페이즈 하나가 쓰는 패턴 전부와 몸 색.
    /// 페이즈가 바뀌면 이 묶음을 통째로 갈아 끼운다.
    /// 그래서 보스 코드에 '지금 몇 페이즈인가' 를 묻는 곳이 한 군데도 없다.
    [System.Serializable]
    public class PhasePatterns
    {
        [Tooltip("패턴을 쓰지 않을 때의 몸 색")]
        public Color BodyColor = new Color(0.45f, 0.12f, 0.55f);

        public SweepPattern Sweep = new SweepPattern();
        public DashPattern Dash = new DashPattern();
        public FanPattern Fan = new FanPattern();
        public SummonPattern Summon = new SummonPattern();
    }

    /// 보스의 수치. 공통 값 위에 페이즈 묶음이 둘 붙는다.
    /// EnemyData 가 abstract 라 에셋을 만들려면 자식이 하나 있어야 한다.
    [CreateAssetMenu(fileName = "EnemyData_", menuName = "JM2D/Enemy Data/Boss")]
    public class BossData : EnemyData
    {
        [Tooltip("방에 나온 뒤 첫 패턴까지 쫓아오는 시간. 나오자마자 때리면 읽을 틈이 없다")]
        [Min(0f)]
        [SerializeField] private float _entryDelay = 1f;

        [Tooltip("체력이 이 비율 아래로 내려가면 2페이즈로 넘어간다")]
        [Range(0f, 1f)]
        [SerializeField] private float _phaseChangeAt = 0.5f;

        [Tooltip("넘어가는 동안 그 자리에 멈춰 있는 시간. 이 동안 피해를 받지 않는다")]
        [Min(0f)]
        [SerializeField] private float _phaseChangeTime = 1f;

        [SerializeField] private PhasePatterns _phase1 = new PhasePatterns();
        [SerializeField] private PhasePatterns _phase2 = new PhasePatterns();

        public float EntryDelay => _entryDelay;
        public float PhaseChangeAt => _phaseChangeAt;
        public float PhaseChangeTime => _phaseChangeTime;
        public PhasePatterns Phase1 => _phase1;
        public PhasePatterns Phase2 => _phase2;
    }
}
