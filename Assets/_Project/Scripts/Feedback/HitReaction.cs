using Unity.Cinemachine;
using UnityEngine;
using JM2D.Combat;

namespace JM2D.Feedback
{
    /// 맞았을 때 일어나는 일을 모은다. 맞는 대상에 붙인다.
    /// 세기를 여기서 들고 있으므로 플레이어와 적이 서로 다른 값을 가진다.
    /// 색은 이 컴포넌트가 혼자 소유한다. 번쩍임과 상태 색이 서로를 덮지 않게 하려는 것이다.
    [RequireComponent(typeof(Health), typeof(CinemachineImpulseSource))]
    public class HitReaction : MonoBehaviour
    {
        [Header("세기")]
        [SerializeField] private float _hitStopTime = 0.05f;

        [Tooltip("카메라를 흔드는 힘. 1 이 Cinemachine 의 보통 세기다")]
        [SerializeField] private float _shakeForce = 0.1f;

        [Header("번쩍임")]
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField] private float _flashTime = 0.1f;

        [SerializeField] private HitStop _hitStop;

        private Health _health;
        private CinemachineImpulseSource _impulse;
        private SpriteRenderer _renderer;

        /// 번쩍임이 끝날 때까지 남은 시간.
        private float _flashLeft;

        /// 번쩍임이 끝나면 돌아갈 색. 적은 상태가 바뀔 때마다 갱신된다.
        private Color _baseColor;

        /// 칠할 곳. 같은 오브젝트의 Awake 순서는 보장되지 않아,
        /// EnemyBase 의 Awake 가 이쪽보다 먼저 색을 넣는 경우가 있다. 그때 여기서 잡는다.
        private SpriteRenderer Renderer
        {
            get
            {
                if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
                return _renderer;
            }
        }

        private void Awake()
        {
            _health = GetComponent<Health>();
            _impulse = GetComponent<CinemachineImpulseSource>();
            _baseColor = Renderer.color;
        }

        private void OnEnable()
        {
            _health.OnDamaged += OnDamaged;
        }

        private void Update()
        {
            if (_flashLeft <= 0f) return;

            // 셰이크와 반대로 멈추는 시간을 쓴다. 번쩍임은 멈춘 동안 유지돼야 보인다.
            _flashLeft -= Time.deltaTime;

            if (_flashLeft <= 0f)
                Renderer.color = _baseColor;
        }

        private void OnDisable()
        {
            _health.OnDamaged -= OnDamaged;
        }

        private void OnDamaged()
        {
            _hitStop.Stop(_hitStopTime);
            _impulse.GenerateImpulseWithForce(_shakeForce);

            _flashLeft = _flashTime;
            Renderer.color = _flashColor;
        }

        /// 번쩍임이 끝나면 돌아갈 색을 알려준다. 적은 상태가 바뀔 때마다 부른다.
        /// 번쩍이는 중이면 기억만 바꾼다. 끝날 때 새 색으로 돌아간다.
        public void SetBaseColor(Color color)
        {
            _baseColor = color;

            if (_flashLeft > 0f) return;
            Renderer.color = color;
        }

        /// 판 도중에 태어나는 적은 프리팹이 씬의 HitStop 을 가리킬 수 없다. 만드는 쪽이 넣어 준다.
        /// 적은 EnemyBase 가 받아서 넘겨 주고, 플레이어는 씬에 있어 인스펙터로 잇는다.
        public void SetHitStop(HitStop hitStop)
        {
            _hitStop = hitStop;
        }
    }
}
