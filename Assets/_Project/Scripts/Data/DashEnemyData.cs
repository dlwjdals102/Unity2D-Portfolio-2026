using UnityEngine;

namespace JM2D.Data
{
    /// 돌진 적. 멈춰서 예고한 뒤, 예고가 시작될 때의 방향으로 곧게 돌진한다.
    [CreateAssetMenu(fileName = "EnemyData_", menuName = "JM2D/Enemy Data/Dash")]
    public class DashEnemyData : EnemyData
    {
        [Header("돌진")]
        [Tooltip("추적 중 이 안으로 들어오면 멈추고 예고한다. 돌진하는 거리가 아니다")]
        [SerializeField] private float _dashStartRange = 4f;

        [Tooltip("예고 시간. 플레이어가 보고 비킬 수 있어야 한다")]
        [SerializeField] private float _windupTime = 0.6f;

        [Tooltip("플레이어보다 빨라야 돌진이다")]
        [SerializeField] private float _dashSpeed = 14f;

        [SerializeField] private float _dashTime = 0.35f;

        [Tooltip("돌진 뒤 멈춰 있는 시간. 이 동안 다시 돌진하지 않는다")]
        [SerializeField] private float _recoverTime = 0.8f;

        [SerializeField] private int _dashDamage = 1;

        [Tooltip("돌진 중 이 거리 안이면 한 번 맞힌다. 1x1 콜라이더 둘이 닿는 거리는 정면 1.0, 대각 1.4 다")]
        [SerializeField] private float _contactRadius = 1f;

        public float DashStartRange => _dashStartRange;
        public float WindupTime => _windupTime;
        public float DashSpeed => _dashSpeed;
        public float DashTime => _dashTime;
        public float RecoverTime => _recoverTime;
        public int DashDamage => _dashDamage;
        public float ContactRadius => _contactRadius;
    }
}
