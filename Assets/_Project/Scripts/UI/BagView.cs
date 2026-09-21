using System.Collections.Generic;
using JM2D.Data;
using JM2D.Items;
using JM2D.Logic.Bag;
using JM2D.Player;
using JM2D.Rooms;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JM2D.UI
{
    /// 가방 격자를 화면에 그리고 Tab 으로 열고 닫는다.
    ///
    /// 칸 위치를 코드로 계산한다. Grid Layout Group 을 쓰면 칸 배치는 공짜지만
    /// 여러 칸을 덮는 아이템을 얹을 때 레이아웃과 싸워야 한다.
    ///
    /// 놓을 수 있는 것은 손에 든 아이템 하나다. 손은 ItemInventory 가 갖고 방 보상이 채운다.
    /// 휠 팔레트는 디버그 체크를 켰을 때만 도는 확인용이다.
    public class BagView : MonoBehaviour
    {
        [Header("구성")]
        [SerializeField] private ItemInventory _inventory;
        [SerializeField] private GameObject _panel;
        [SerializeField] private RectTransform _cellRoot;
        [SerializeField] private Image _cellPrefab;
        [SerializeField] private Image _itemPrefab;

        [Tooltip("전투가 시작되면 가방을 닫고 잠근다. 방을 깨면 푼다")]
        [SerializeField] private RoomCombat _roomCombat;

        [Tooltip("배치에 따라 스탯이 어떻게 바뀌는지 보여준다. 디버그용이며 Phase 5 에서 정리한다")]
        [SerializeField] private PlayerStats _stats;
        [SerializeField] private TMP_Text _statText;

        [Header("크기")]
        [SerializeField] private float _cellSize = 64f;

        [Tooltip("칸 사이 여백. 위치는 그대로 두고 크기만 줄여서 만든다")]
        [SerializeField] private float _padding = 4f;

        [Header("미리보기")]
        [Range(0f, 1f)]
        [SerializeField] private float _previewAlpha = 0.6f;

        [Header("이름표")]
        [Tooltip("아이템 위에 적는 이름. 글꼴은 스탯 표시의 것을 빌려 쓴다")]
        [SerializeField] private float _labelFontSize = 16f;
        [SerializeField] private Color _labelColor = Color.black;

        [Header("디버그")]
        [Tooltip("켜면 휠로 아무 아이템이나 손에 넣는다. 배치를 만들어 볼 때 쓴다")]
        [SerializeField] private bool _debugPalette;

        [Tooltip("디버그 팔레트의 휠 순서")]
        [SerializeField] private ItemData[] _palette;

        private readonly List<Image> _itemViews = new List<Image>();

        private Image _previewView;
        private TMP_Text _previewLabel;

        /// 디버그 팔레트에서 몇 번째를 골랐는가. -1 이면 아직 고르지 않았다.
        private int _selectedIndex = -1;

        /// 전투 중인가. 잠긴 동안에는 Tab 이 아무 일도 하지 않는다.
        private bool _isLocked;

        private void Awake()
        {
            BuildCells();
            BuildPreview();
            UpdateInfoText();

            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            _roomCombat.OnCombatStarted += Lock;
            _roomCombat.OnRoomCleared += Unlock;
        }

        /// 스탯이 바뀌면 곧바로 다시 쓴다. 무기, 아이템, 시너지 어느 쪽이 바꿔도 같다.
        /// 스탯은 PlayerStats.Awake 에서 만들어져, 그보다 먼저 돌 수 있는 Awake 나 OnEnable 이 아니라 여기서 구독한다.
        /// 구독 전에 바뀐 값(무기는 Start 에서 얹힌다)을 놓치지 않게 끝에서 한 번 쓴다.
        private void Start()
        {
            if (_stats == null) return;

            _stats.AttackDamage.OnChanged += UpdateInfoText;
            _stats.MoveSpeed.OnChanged += UpdateInfoText;
            _stats.AttackSpeed.OnChanged += UpdateInfoText;
            _stats.MaxHealth.OnChanged += UpdateInfoText;

            UpdateInfoText();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.tabKey.wasPressedThisFrame && !_isLocked)
                Toggle();

            if (!_panel.activeSelf) return;

            if (Keyboard.current.rKey.wasPressedThisFrame && _inventory.Hand != null)
            {
                _inventory.RotateHand();
                UpdateInfoText();
            }

            if (Mouse.current == null) return;

            if (_debugPalette) ReadSelectionWheel();

            UpdatePreview();

            if (Mouse.current.leftButton.wasPressedThisFrame) TryPlaceAtMouse();
            if (Mouse.current.rightButton.wasPressedThisFrame) TryRemoveAtMouse();
        }

        private void OnDisable()
        {
            _roomCombat.OnCombatStarted -= Lock;
            _roomCombat.OnRoomCleared -= Unlock;
        }

        private void OnDestroy()
        {
            if (_stats == null) return;

            _stats.AttackDamage.OnChanged -= UpdateInfoText;
            _stats.MoveSpeed.OnChanged -= UpdateInfoText;
            _stats.AttackSpeed.OnChanged -= UpdateInfoText;
            _stats.MaxHealth.OnChanged -= UpdateInfoText;
        }

        /// 밖에서 연다. 방 보상을 고르면 곧바로 놓을 수 있게 열어 준다.
        /// 잠겨 있거나 이미 열려 있으면 아무 일도 하지 않는다.
        public void Open()
        {
            if (_isLocked || _panel.activeSelf) return;

            Toggle();
        }

        /// 전투가 시작되면 잠그고, 열려 있으면 닫는다.
        /// 방은 받지만 쓰지 않는다. 짝이 되는 두 이벤트의 모양을 맞춰 둔 것이다.
        private void Lock(Room room)
        {
            _isLocked = true;

            if (_panel.activeSelf) Toggle();
        }

        /// 방을 깨면 다시 열 수 있다.
        private void Unlock(Room room)
        {
            _isLocked = false;
        }

        /// 창을 닫아도 손에 든 것은 그대로 남는다. 다음에 열면 여전히 들고 있다.
        /// 놓을 자리를 못 정한 채로 다음 방에 갈 수 있어야 한다.
        private void Toggle()
        {
            bool opening = !_panel.activeSelf;

            _panel.SetActive(opening);

            if (!opening)
                _previewView.gameObject.SetActive(false);

            // 열 때도 다시 쓴다. 손은 가방 밖(보상)에서 채워져, 열어 보기 전까지 글이 옛것이다.
            UpdateInfoText();
        }

        /// 디버그 전용. 휠을 굴리면 팔레트를 한 칸씩 넘겨 그 아이템을 손에 넣는다.
        /// 들고 있던 것은 덮어쓴다. 끝에 닿으면 반대쪽 끝으로 돌아간다.
        /// 휠 값의 크기는 기기마다 달라 방향만 본다. 한 프레임에 여러 칸을 굴려도 한 칸이다.
        private void ReadSelectionWheel()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (scroll == 0f || _palette.Length == 0) return;

            int step = scroll < 0f ? 1 : -1;    // 아래로 굴리면 다음 아이템

            if (_selectedIndex < 0)
                _selectedIndex = step > 0 ? 0 : _palette.Length - 1;
            else
                _selectedIndex = (_selectedIndex + step + _palette.Length) % _palette.Length;

            _inventory.PutInHand(_palette[_selectedIndex]);
            UpdateInfoText();
        }

        /// 마우스가 가리키는 칸에 놓을 모습을 반투명하게 보여준다.
        /// 놓을 수 없으면 빨강이라 클릭하기 전에 알 수 있다.
        private void UpdatePreview()
        {
            ItemInstance hand = _inventory.Hand;

            if (hand == null || !TryGetCellUnderMouse(out int x, out int y))
            {
                _previewView.gameObject.SetActive(false);
                return;
            }

            _previewView.gameObject.SetActive(true);
            _previewLabel.text = hand.Data.DisplayName;

            bool canPlace = _inventory.Grid.CanPlace(hand, x, y);
            Color color = canPlace ? hand.Data.Color : Color.red;
            color.a = _previewAlpha;
            _previewView.color = color;

            PlaceAt(_previewView.rectTransform, x, y, hand.Width, hand.Height);
        }

        private void TryPlaceAtMouse()
        {
            ItemInstance hand = _inventory.Hand;

            if (hand == null) return;

            if (!TryGetCellUnderMouse(out int x, out int y)) return;

            // 놓고 나면 손이 비어 이름을 읽을 수 없다. 먼저 담아 둔다.
            string placed = hand.Data.DisplayName;

            if (!_inventory.TryPlaceHand(x, y))
            {
                Debug.Log($"[가방] ({x},{y}) 에는 놓을 수 없다");
                return;
            }

            Debug.Log($"[가방] {placed} 을 ({x},{y}) 에 놓았다");

            _previewView.gameObject.SetActive(false);
            Redraw();
        }

        private void TryRemoveAtMouse()
        {
            if (!TryGetCellUnderMouse(out int x, out int y)) return;

            // 그리드는 IGridItem 만 알므로 여기서 되돌린다.
            // 넣는 것이 ItemInstance 뿐이라 실패할 일은 없지만 조용히 넘긴다.
            if (_inventory.Grid.GetAt(x, y) is not ItemInstance instance) return;

            _inventory.Remove(instance);
            Redraw();

            Debug.Log($"[가방] {instance.Data.DisplayName} 을 뺐다");
        }

        /// 마우스가 어느 칸 위에 있는지. 격자 밖이면 false.
        private bool TryGetCellUnderMouse(out int x, out int y)
        {
            x = y = -1;

            Vector2 screen = Mouse.current.position.ReadValue();

            // Screen Space - Overlay 라 카메라는 null 을 넘긴다.
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _cellRoot, screen, null, out Vector2 local))
                return false;

            // CellRoot 의 피벗이 왼쪽 위라 local.y 는 아래로 갈수록 음수다.
            x = Mathf.FloorToInt(local.x / _cellSize);
            y = Mathf.FloorToInt(-local.y / _cellSize);

            BagGrid grid = _inventory.Grid;
            return x >= 0 && y >= 0 && x < grid.Width && y < grid.Height;
        }

        private void BuildCells()
        {
            BagGrid grid = _inventory.Grid;

            // 격자의 기준점을 왼쪽 위로 못 박는다.
            // 인스펙터 설정에 맡기면 피벗이 가운데인 채로 좌표가 어긋난다.
            _cellRoot.anchorMin = new Vector2(0f, 1f);
            _cellRoot.anchorMax = new Vector2(0f, 1f);
            _cellRoot.pivot = new Vector2(0f, 1f);

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    Image cell = Instantiate(_cellPrefab, _cellRoot);
                    cell.name = $"Cell ({x},{y})";
                    PlaceAt(cell.rectTransform, x, y, 1, 1);
                }
            }

            _cellRoot.sizeDelta = new Vector2(grid.Width * _cellSize, grid.Height * _cellSize);
        }

        private void BuildPreview()
        {
            _previewView = Instantiate(_itemPrefab, _cellRoot);
            _previewView.name = "Preview";
            _previewView.raycastTarget = false;
            _previewView.gameObject.SetActive(false);

            _previewLabel = CreateLabel(_previewView);
        }

        /// 놓인 것을 전부 지우고 그리드를 훑어 다시 그린다.
        /// 25칸이라 비용이 없고, 화면과 그리드가 어긋날 수 없다.
        private void Redraw()
        {
            foreach (Image view in _itemViews)
                Destroy(view.gameObject);

            _itemViews.Clear();

            // 여러 칸을 덮는 아이템도 왼쪽 위 좌표로 한 번만 나온다.
            foreach (PlacedItem placed in _inventory.Grid.GetPlacedItems())
                DrawItem((ItemInstance)placed.Item, placed.X, placed.Y);

            // 새로 그린 아이템이 미리보기를 덮지 않게 맨 앞으로 올린다.
            _previewView.transform.SetAsLastSibling();

            UpdateInfoText();
        }

        /// 손에 든 것과 스탯을 보여준다. 손이나 배치가 바뀔 때마다 갱신한다.
        /// 스탯에는 시너지가 반영된 값이 그대로 보인다.
        private void UpdateInfoText()
        {
            if (_statText == null || _stats == null) return;

            ItemInstance hand = _inventory.Hand;

            string held = hand == null
                ? "없음"
                : $"{hand.Data.DisplayName} ({hand.Width}x{hand.Height})";

            _statText.text =
                $"손      {held}\n\n" +
                $"공격력  {_stats.AttackDamage.IntValue}\n" +
                $"이동    {_stats.MoveSpeed.Value:F2}\n" +
                $"연사    {_stats.AttackSpeed.Value:F2}\n" +
                $"최대체력 {_stats.MaxHealth.IntValue}";
        }

        private void DrawItem(ItemInstance instance, int x, int y)
        {
            Image view = Instantiate(_itemPrefab, _cellRoot);
            view.name = $"Item {instance.Data.DisplayName} ({x},{y})";
            view.color = instance.Data.Color;

            PlaceAt(view.rectTransform, x, y, instance.Width, instance.Height);
            CreateLabel(view).text = instance.Data.DisplayName;
            _itemViews.Add(view);
        }

        /// 아이템 위에 이름을 적는다. 아이템을 가득 채우고 가운데에 쓴다.
        /// 프리팹을 고치지 않으려고 코드에서 만든다. 아이템과 함께 지워진다.
        private TMP_Text CreateLabel(Image parent)
        {
            var go = new GameObject("Name", typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var label = go.AddComponent<TextMeshProUGUI>();

            // TMP 기본 글꼴에는 한글이 없다. 스탯 표시가 쓰는 글꼴을 빌린다.
            if (_statText != null) label.font = _statText.font;

            label.fontSize = _labelFontSize;
            label.color = _labelColor;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            return label;
        }

        /// 격자 좌표를 UI 좌표로 옮긴다.
        /// UI 는 위쪽이 +y 인데 격자는 위쪽이 y=0 이라 y 를 뒤집는다.
        private void PlaceAt(RectTransform rt, int x, int y, int width, int height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            rt.sizeDelta = new Vector2(width * _cellSize - _padding, height * _cellSize - _padding);
            rt.anchoredPosition = new Vector2(x * _cellSize, -y * _cellSize);
        }
    }
}
