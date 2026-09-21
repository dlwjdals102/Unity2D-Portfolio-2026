using System;
using System.Collections.Generic;
using System.Text;
using JM2D.Data;
using JM2D.Logic.Bag;
using JM2D.Logic.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JM2D.UI
{
    /// 방을 깨면 뜨는 보상 창. 아이템 셋을 보여주고 고른 것을 알린다.
    /// 고른 아이템을 어디에 넣는지는 모른다. 듣는 쪽이 정한다.
    ///
    /// 카드는 코드로 만든다. 가방 칸을 코드로 만드는 것과 같은 방식이다.
    public class RewardPanel : MonoBehaviour
    {
        /// 하나를 골랐을 때 알린다.
        public event Action<ItemData> OnPicked;

        [Header("구성")]
        [SerializeField] private GameObject _root;

        [Tooltip("카드 셋이 이 아래에 만들어진다")]
        [SerializeField] private RectTransform _cardRoot;

        [Tooltip("제목. 글꼴도 여기서 빌린다. TMP 기본 글꼴에는 한글이 없다")]
        [SerializeField] private TMP_Text _title;

        [Header("카드 모양")]
        [SerializeField] private Vector2 _cardSize = new Vector2(240f, 320f);
        [SerializeField] private float _cardGap = 32f;
        [SerializeField] private Color _cardColor = new Color(0.16f, 0.16f, 0.2f, 0.95f);
        [SerializeField] private Color _textColor = Color.white;

        [Tooltip("아이템 색을 보여주는 사각형의 한 변")]
        [SerializeField] private float _swatchSize = 56f;

        [Tooltip("제목을 화면 위에서 얼마나 내려 놓을지")]
        [SerializeField] private float _titleOffset = 80f;

        [Header("확인용")]
        [Tooltip("인스펙터 우클릭 메뉴로 띄워 볼 때 쓰는 아이템들")]
        [SerializeField] private ItemData[] _debugCandidates;

        /// 카드 안쪽 여백.
        private const float Margin = 12f;

        /// 지금 창에 떠 있는 후보. 숫자 키로 고를 때 쓴다.
        private readonly List<ItemData> _candidates = new List<ItemData>();

        /// 만들어 둔 카드. 다시 띄울 때 지우고 새로 만든다.
        private readonly List<GameObject> _cards = new List<GameObject>();

        public bool IsOpen => _root.activeSelf;

        /// 자리 계산의 기준이 되는 앵커와 피벗은 코드가 정한다.
        /// 인스펙터에 맡기면 오브젝트를 어떻게 만들었느냐에 따라 기준점이 달라지고, 오류 없이 위치만 틀린다.
        private void Awake()
        {
            // 이 오브젝트 자체도 화면을 덮어야 한다. 빈 오브젝트로 만들면 100 x 100 이라
            // 가림막이 그만큼만 덮이고 화면 가운데 작은 네모로 보인다.
            Stretch((RectTransform)transform);
            Stretch((RectTransform)_root.transform);
            Center(_cardRoot);

            if (_title != null) TopCenter((RectTransform)_title.transform, _titleOffset, 400f, 60f);

            _root.SetActive(false);
        }

        /// 게임이 멈춘 동안에도 Update 는 돌아 숫자 키를 받을 수 있다.
        private void Update()
        {
            if (!IsOpen || Keyboard.current == null) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame) Pick(0);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) Pick(1);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) Pick(2);
        }

        /// 후보를 받아 창을 띄운다. 셋이 아니어도 받은 수만큼 만든다.
        public void Show(IReadOnlyList<ItemData> candidates)
        {
            _candidates.Clear();
            _candidates.AddRange(candidates);

            Build();

            _root.SetActive(true);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        /// 창을 먼저 닫고 알린다. 듣는 쪽이 이 자리에서 다시 띄워도 엉키지 않는다.
        private void Pick(int index)
        {
            if (index < 0 || index >= _candidates.Count) return;

            ItemData picked = _candidates[index];

            Hide();

            OnPicked?.Invoke(picked);
        }

        private void Build()
        {
            foreach (GameObject card in _cards)
                Destroy(card);

            _cards.Clear();

            for (int i = 0; i < _candidates.Count; i++)
                _cards.Add(CreateCard(i, _candidates[i]));
        }

        /// 카드 하나. 가운데를 기준으로 좌우로 펼쳐 놓는다.
        /// 카드 안의 것들은 카드 위쪽에 붙여 내려가며 쌓는다.
        private GameObject CreateCard(int index, ItemData data)
        {
            var background = new GameObject($"Card {index} {data.DisplayName}", typeof(RectTransform));
            background.transform.SetParent(_cardRoot, false);

            var rect = (RectTransform)background.transform;
            Center(rect);
            rect.sizeDelta = _cardSize;

            float step = _cardSize.x + _cardGap;
            rect.anchoredPosition = new Vector2((index - (_candidates.Count - 1) * 0.5f) * step, 0f);

            var image = background.AddComponent<Image>();
            image.color = _cardColor;

            Button button = background.AddComponent<Button>();
            button.onClick.AddListener(() => Pick(index));

            // 번호. 키보드로 고를 때 쓴다. 카드 왼쪽 위에 붙인다.
            TMP_Text number = CreateText(rect, $"{index + 1}", 28f, TextAlignmentOptions.TopLeft);
            var numberRect = (RectTransform)number.transform;
            numberRect.anchorMin = numberRect.anchorMax = new Vector2(0f, 1f);
            numberRect.pivot = new Vector2(0f, 1f);
            numberRect.sizeDelta = new Vector2(48f, 36f);
            numberRect.anchoredPosition = new Vector2(Margin, -Margin);

            // 위에서부터 내려가며 쌓는다. y 는 카드 위쪽 변에서 잰 거리다.
            float y = Margin + 36f;

            var swatch = new GameObject("Swatch", typeof(RectTransform));
            swatch.transform.SetParent(rect, false);
            TopCenter((RectTransform)swatch.transform, y, _swatchSize, _swatchSize);
            swatch.AddComponent<Image>().color = data.Color;
            y += _swatchSize + Margin;

            float textWidth = _cardSize.x - Margin * 2f;

            TMP_Text title = CreateText(rect, $"{data.DisplayName}  {data.Width}x{data.Height}", 24f, TextAlignmentOptions.Top);
            TopCenter((RectTransform)title.transform, y, textWidth, 34f);
            y += 34f + Margin;

            TMP_Text effects = CreateText(rect, Describe(data), 18f, TextAlignmentOptions.Top);
            TopCenter((RectTransform)effects.transform, y, textWidth, _cardSize.y - y - Margin);

            return background;
        }

        /// 글꼴은 제목의 것을 빌린다. TMP 기본 글꼴에는 한글이 없다.
        /// 자리는 부르는 쪽이 Center 나 TopCenter 로 잡는다.
        private TMP_Text CreateText(RectTransform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var label = go.AddComponent<TextMeshProUGUI>();

            if (_title != null) label.font = _title.font;

            label.text = text;
            label.fontSize = fontSize;
            label.color = _textColor;
            label.alignment = alignment;

            // 카드 전체가 버튼이라 글자가 클릭을 가로채면 안 된다.
            label.raycastTarget = false;

            return label;
        }

        /// 부모를 가득 채운다.
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// 부모 한가운데에 놓는다. 크기는 부르는 쪽이 정한다.
        private static void Center(RectTransform rect)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }

        /// 부모 위쪽 변에서 offset 만큼 내려 가운데에 놓는다.
        private static void TopCenter(RectTransform rect, float offset, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(0f, -offset);
        }

        /// 아이템의 효과와 시너지를 사람이 읽을 문장으로 만든다.
        private static string Describe(ItemData data)
        {
            var text = new StringBuilder();

            foreach (ItemModifier modifier in data.Modifiers)
                text.AppendLine(Describe(modifier));

            foreach (ItemSynergy synergy in data.Synergies)
                text.AppendLine($"{Name(synergy.Condition)} 하나당 {Describe(synergy.Modifier)}");

            return text.ToString();
        }

        /// 퍼센트는 0.1 을 10% 로 적는다. 가산과 승산을 구분해 적지 않는다.
        /// 계산 방식은 플레이어에게 보이지 않고, 구분해 적으면 문구만 길어진다.
        private static string Describe(ItemModifier modifier)
        {
            if (modifier.Type == ModifierType.Flat)
                return $"{Name(modifier.Target)} {Signed(modifier.Value)}";

            return $"{Name(modifier.Target)} {Signed(modifier.Value * 100f)}%";
        }

        private static string Signed(float value)
        {
            return value >= 0f ? $"+{value:0.##}" : $"{value:0.##}";
        }

        private static string Name(StatType type)
        {
            switch (type)
            {
                case StatType.MaxHealth: return "최대 체력";
                case StatType.MoveSpeed: return "이동 속도";
                case StatType.AttackDamage: return "공격력";
                case StatType.AttackSpeed: return "공격 속도";
                default: return type.ToString();
            }
        }

        private static string Name(SynergyCondition condition)
        {
            switch (condition)
            {
                case SynergyCondition.Adjacent: return "인접";
                case SynergyCondition.SameRow: return "같은 행";
                case SynergyCondition.SameColumn: return "같은 열";
                case SynergyCondition.Corner: return "모서리";
                default: return condition.ToString();
            }
        }

        /// 플레이 중에 컴포넌트를 우클릭해 부른다. 방을 깨지 않고 창을 확인한다.
        [ContextMenu("확인용으로 띄우기")]
        private void ShowDebugCandidates()
        {
            Show(_debugCandidates);
        }
    }
}
