using JM2D.Combat;
using JM2D.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace JM2D.UI
{
    /// 보스 전용 체력 바. 보스가 나올 때 붙었다가 죽으면 사라진다.
    /// 판이 시작될 때는 보스가 없으므로 만드는 쪽(RoomCombat)이 붙여 준다.
    ///
    /// 바탕과 채움과 경계선은 코드로 만든다. 보상 창의 카드와 같은 방식이고,
    /// 자리 계산의 기준인 앵커와 피벗을 사람이 건드려 조용히 어긋나는 것을 막는다.
    public class BossHealthBar : MonoBehaviour
    {
        [Header("모양")]
        [SerializeField] private Vector2 _size = new Vector2(720f, 22f);

        [Tooltip("화면 위쪽 변에서 얼마나 내려 놓을지")]
        [SerializeField] private float _topOffset = 40f;

        [SerializeField] private Color _backgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.9f);

        [Tooltip("1페이즈 동안의 채움 색")]
        [SerializeField] private Color _fillColor = new Color(0.55f, 0.2f, 0.7f, 1f);

        [Tooltip("2페이즈에 들어간 뒤의 채움 색")]
        [SerializeField] private Color _fillColorPhase2 = new Color(0.85f, 0.15f, 0.4f, 1f);

        [Header("페이즈 경계선")]
        [SerializeField] private Color _lineColor = Color.white;
        [SerializeField] private float _lineWidth = 3f;

        private RectTransform _fill;
        private Image _fillImage;
        private RectTransform _line;

        /// 지금 붙어 있는 보스의 체력. 붙은 것이 없으면 null.
        private Health _health;

        /// 이 비율 아래로 내려가면 2페이즈다. 보스의 데이터에서 받는다.
        private float _phaseAt = 0.5f;

        private void Awake()
        {
            var rect = (RectTransform)transform;

            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = _size;
            rect.anchoredPosition = new Vector2(0f, -_topOffset);

            CreateBackground();
            _fill = CreateFill();
            _line = CreateLine();

            // 판이 시작될 때는 보스가 없다. 붙을 때까지 숨어 있는다.
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

        /// 남은 비율만큼 채움의 오른쪽 앵커를 당긴다.
        /// Image 의 fillAmount 는 스프라이트가 있어야 해서, 스프라이트 없이도 도는 앵커로 줄인다.
        private void Refresh(int current, int max)
        {
            float ratio = max <= 0 ? 0f : (float)current / max;

            _fill.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
            _fillImage.color = ratio > _phaseAt ? _fillColor : _fillColorPhase2;
        }

        /// 경계선의 가로 자리는 비율에서 나오는 계산값이라 코드가 정한다.
        private void PlaceLine()
        {
            _line.anchorMin = new Vector2(_phaseAt, 0f);
            _line.anchorMax = new Vector2(_phaseAt, 1f);
        }

        private void CreateBackground()
        {
            RectTransform rect = CreateChild("Background", _backgroundColor);

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private RectTransform CreateFill()
        {
            RectTransform rect = CreateChild("Fill", _fillColor);

            _fillImage = rect.GetComponent<Image>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return rect;
        }

        private RectTransform CreateLine()
        {
            RectTransform rect = CreateChild("PhaseLine", _lineColor);

            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(_lineWidth, 0f);
            rect.anchoredPosition = Vector2.zero;

            return rect;
        }

        private RectTransform CreateChild(string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);

            Image image = go.AddComponent<Image>();
            image.color = color;

            // 바는 누르는 것이 아니다. 아래의 것이 눌리는 것을 막지 않는다.
            image.raycastTarget = false;

            return (RectTransform)go.transform;
        }
    }
}
