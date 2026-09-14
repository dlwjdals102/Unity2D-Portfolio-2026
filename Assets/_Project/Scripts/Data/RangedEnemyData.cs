using UnityEngine;

namespace JM2D.Data
{
    /// 원거리 적. 멈춰서 조준한 뒤 쏘는 순간의 플레이어 쪽으로 한 발 쏜다.
    /// 가까이 오면 물러나면서 쏜다.
    /// 'Fire' 는 화염 피해로 읽혀 'Shoot' 을 쓴다.
    [CreateAssetMenu(fileName = "EnemyData_", menuName = "JM2D/Enemy Data/Ranged")]
    public class RangedEnemyData : EnemyData
    {
        [Header("사격")]
        [Tooltip("추적 중 이 안으로 들어오면 조준을 시작한다. 총알이 닿는 거리가 아니다")]
        [SerializeField] private float _shootStartRange = 6f;

        [Tooltip("사격 중 이 밖으로 나가면 멈추고 다시 쫓는다. 시작 거리보다 넓어야 한다")]
        [SerializeField] private float _shootStopRange = 7f;

        [Tooltip("조준 색이 보이는 시간. 끝나는 순간의 플레이어 쪽으로 쏜다")]
        [SerializeField] private float _aimTime = 0.5f;

        [Tooltip("쏜 뒤 다음 조준까지의 시간")]
        [SerializeField] private float _shootCooldown = 1f;

        [SerializeField] private int _shotDamage = 1;

        [Header("후퇴")]
        [Tooltip("플레이어가 이 안으로 오면 물러난다. 사격 시작 거리보다 좁아야 한다")]
        [SerializeField] private float _retreatRange = 3f;

        [Tooltip("플레이어 이동 속도보다 느려야 쫓아가서 잡을 수 있다")]
        [SerializeField] private float _retreatSpeed = 2f;

        public float ShootStartRange => _shootStartRange;
        public float ShootStopRange => _shootStopRange;
        public float AimTime => _aimTime;
        public float ShootCooldown => _shootCooldown;
        public int ShotDamage => _shotDamage;
        public float RetreatRange => _retreatRange;
        public float RetreatSpeed => _retreatSpeed;

        protected override void OnValidate()
        {
            base.OnValidate();

            if (_shootStopRange < _shootStartRange)
                Debug.LogWarning($"{name}: 사격 멈춤 거리({_shootStopRange})가 사격 시작 거리({_shootStartRange})보다 작다", this);

            // 후퇴가 사격 구간을 넘어가면 물러나기와 쫓기가 거리를 반대로 밀어 추적과 교전을 오간다.
            // 시작과 멈춤 사이여도 깨지지는 않지만 멈춰서 쏘는 구간이 사라져 시작 거리를 기준으로 삼는다.
            if (_retreatRange >= _shootStartRange)
                Debug.LogWarning($"{name}: 후퇴 거리({_retreatRange})가 사격 시작 거리({_shootStartRange})보다 좁지 않다. 멈춰서 쏘는 구간이 없다", this);
        }
    }
}
