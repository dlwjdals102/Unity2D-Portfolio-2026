using JM2D.Combat;
using JM2D.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace JM2D.UI
{
    /// 보스 전용 체력 바. 보스가 나올 때 붙었다가 죽으면 사라진다.
    /// 판이 시작될 때는 보스가 없으므로 만드는 쪽(RoomCombat)이 붙여 준다.
    ///
    /// 조각 셋(바탕, 채움, 경계선)은 씬에 있고 이 클래스는 값만 쓴다.
    /// 자리와 크기와 색은 인스펙터에 있다. 계산에서 나오는 것은 채움 비율과 경계선 자리 둘뿐이다.
    public class BossHealthBar : MonoBehaviour
    {
        [Tooltip("체력만큼 채워지는 칸. Image Type 은 Filled, Fill Method 는 Horizontal 이어야 한다")]
        [SerializeField] private Image _fillImage;

        [Tooltip("2페이즈가 시작되는 자리에 그어지는 선. 가로 앵커만 코드가 옮긴다")]
        [SerializeField] private RectTransform _line;

        [Tooltip("2페이즈에 들어간 뒤의 채움 색")]
        [SerializeField] private Color _fillColorPhase2 = new Color(0.85f, 0.15f, 0.4f, 1f);

        /// 지금 붙어 있는 보스의 체력. 붙은 것이 없으면 null.
        private Health _health;

        /// 1페이즈의 채움 색. 씬에서 칠한 것이고 2페이즈에서 돌아올 자리다.
        private Color _normalColor;

        /// 이 비율 아래로 내려가면 2페이즈다. 보스의 데이터에서 받는다.
        private float _phaseAt = 0.5f;

        private void Awake()
        {
            _normalColor = _fillImage.color;

            // 판이 시작될 때는 보스가 없다. 붙을 때까지 숨어 있는다.
            // 씬에서는 켜 둔 채로 두어야 한다. 꺼 두면 이 Awake 가 돌지 않는다.
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// 보스가 태어날 때 만드는 쪽이 부른다.
        public void Bind(Boss boss)
        {
            Unbind();

            _health = boss.GetComponent<Health>();

            // 절반이라는 값을 여기에 또 적지 않는다. 한쪽만 바뀌면 선이 엉뚱한 곳에 그어진다.
            _phaseAt = boss.PhaseChangeAt;

            PlaceLine();

            _health.OnHealthChanged += Refresh;
            _health.OnDied += OnBossDied;

            gameObject.SetActive(true);

            Refresh(_health.Current, _health.Max);
        }

        private void OnBossDied()
        {
            Unbind();

            gameObject.SetActive(false);
        }

        private void Unbind()
        {
            if (_health == null) return;

            _health.OnHealthChanged -= Refresh;
            _health.OnDied -= OnBossDied;
            _health = null;
        }

        /// 남은 비율만큼 채운다. 스프라이트가 있어 플레이어 체력 바와 같은 방식으로 줄어든다.
        private void Refresh(int current, int max)
        {
            float ratio = max <= 0 ? 0f : (float)current / max;

            _fillImage.fillAmount = ratio;
            _fillImage.color = ratio > _phaseAt ? _normalColor : _fillColorPhase2;
        }

        /// 경계선의 가로 자리는 비율에서 나오는 계산값이라 코드가 정한다.
        /// 세로로 늘어나는 것과 폭은 보기값이라 씬이 정한 대로 둔다. 그래서 x 만 바꾼다.
        /// 앵커는 구조체여서 x 하나만 고쳐 넣을 수 없다. 받아서 고치고 다시 넣는다.
        private void PlaceLine()
        {
            Vector2 min = _line.anchorMin;
            Vector2 max = _line.anchorMax;

            min.x = _phaseAt;
            max.x = _phaseAt;

            _line.anchorMin = min;
            _line.anchorMax = max;
        }
    }
}
