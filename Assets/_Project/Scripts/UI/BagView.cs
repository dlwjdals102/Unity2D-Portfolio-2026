using System.Collections.Generic;
using JM2D.Data;
using JM2D.Items;
using JM2D.Logic;
using JM2D.Player;
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
    /// 아이템 고르기(숫자 키)는 임시다. 아이템 획득이 생기면 그쪽으로 옮긴다.
    public class BagView : MonoBehaviour
    {
        [Header("구성")]
        [SerializeField] private ItemInventory _inventory;
        [SerializeField] private GameObject _panel;
        [SerializeField] private RectTransform _cellRoot;
        [SerializeField] private Image _cellPrefab;
        [SerializeField] private Image _itemPrefab;

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

        [Header("고를 수 있는 아이템 (숫자 키 순서, 임시)")]
        [SerializeField] private ItemData[] _palette;

        private readonly List<Image> _itemViews = new List<Image>();

        /// 놓기 전의 아이템. 회전 상태를 들고 있다가 그대로 그리드에 넘어간다.
        private ItemInstance _preview;
        private Image _previewView;

        private void Awake()
        {
            BuildCells();
            BuildPreview();
            UpdateStatText();

            _panel.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.tabKey.wasPressedThisFrame)
                Toggle();

            if (!_panel.activeSelf) return;

            ReadSelectionKeys();

            if (Keyboard.current.rKey.wasPressedThisFrame && _preview != null)
                _preview.Rotate();

            if (Mouse.current == null) return;

            UpdatePreview();

            if (Mouse.current.leftButton.wasPressedThisFrame) TryPlaceAtMouse();
            if (Mouse.current.rightButton.wasPressedThisFrame) TryRemoveAtMouse();
        }

        /// 닫을 때 고른 것을 비운다.
        /// 창을 닫는 것은 배치를 마쳤다는 뜻이라, 손에 든 채로 두지 않는다.
        private void Toggle()
        {
            bool opening = !_panel.activeSelf;

            _panel.SetActive(opening);

            if (!opening)
            {
                _preview = null;
                _previewView.gameObject.SetActive(false);
            }
        }

        private void ReadSelectionKeys()
        {
            Key[] digits = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };

            for (int i = 0; i < digits.Length && i < _palette.Length; i++)
            {
                if (!Keyboard.current[digits[i]].wasPressedThisFrame) continue;

                _preview = new ItemInstance(_palette[i]);
                Debug.Log($"[가방] {_preview.Data.DisplayName} 선택 ({_preview.Width}x{_preview.Height})");
            }
        }

        /// 마우스가 가리키는 칸에 놓을 모습을 반투명하게 보여준다.
        /// 놓을 수 없으면 빨강이라 클릭하기 전에 알 수 있다.
        private void UpdatePreview()
        {
            if (_preview == null || !TryGetCellUnderMouse(out int x, out int y))
            {
                _previewView.gameObject.SetActive(false);
                return;
            }

            _previewView.gameObject.SetActive(true);

            bool canPlace = _inventory.Grid.CanPlace(_preview, x, y);
            Color color = canPlace ? _preview.Data.Color : Color.red;
            color.a = _previewAlpha;
            _previewView.color = color;

            PlaceAt(_previewView.rectTransform, x, y, _preview.Width, _preview.Height);
        }

        private void TryPlaceAtMouse()
        {
            if (_preview == null)
            {
                Debug.Log("[가방] 놓을 아이템을 먼저 고르세요 (1~4)");
                return;
            }

            if (!TryGetCellUnderMouse(out int x, out int y)) return;

            if (!_inventory.TryPlace(_preview, x, y))
            {
                Debug.Log($"[가방] ({x},{y}) 에는 놓을 수 없다");
                return;
            }

            Debug.Log($"[가방] {_preview.Data.DisplayName} 을 ({x},{y}) 에 놓았다");

            // 놓은 인스턴스는 그리드 것이 되었으므로 새로 만든다.
            // 방향은 이어받는다. 같은 아이템을 여러 개 놓을 때 매번 돌리지 않아도 된다.
            ItemData data = _preview.Data;
            bool wasRotated = _preview.IsRotated;

            _preview = new ItemInstance(data);
            if (wasRotated) _preview.Rotate();

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

            UpdateStatText();
        }

        /// 배치가 바뀔 때마다 갱신한다. 시너지가 반영된 값이 그대로 보인다.
        private void UpdateStatText()
        {
            if (_statText == null || _stats == null) return;

            _statText.text =
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
            _itemViews.Add(view);
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
