using System.Collections.Generic;
using JM2D.Data;
using JM2D.Items;
using JM2D.Logic;
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

        [Header("크기")]
        [SerializeField] private float _cellSize = 64f;

        [Tooltip("칸 사이 여백. 위치는 그대로 두고 크기만 줄여서 만든다")]
        [SerializeField] private float _padding = 4f;

        [Header("고를 수 있는 아이템 (숫자 키 순서, 임시)")]
        [SerializeField] private ItemData[] _palette;

        private readonly List<Image> _itemViews = new List<Image>();
        private readonly HashSet<IGridItem> _drawn = new HashSet<IGridItem>();

        private ItemData _selected;

        private void Awake()
        {
            BuildCells();
            _panel.SetActive(false);

            if (_palette.Length > 0) _selected = _palette[0];
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.tabKey.wasPressedThisFrame)
                _panel.SetActive(!_panel.activeSelf);

            if (!_panel.activeSelf) return;

            ReadSelectionKeys();

            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame) TryPlaceAtMouse();
            if (Mouse.current.rightButton.wasPressedThisFrame) TryRemoveAtMouse();
        }

        private void ReadSelectionKeys()
        {
            Key[] digits = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };

            for (int i = 0; i < digits.Length && i < _palette.Length; i++)
            {
                if (!Keyboard.current[digits[i]].wasPressedThisFrame) continue;

                _selected = _palette[i];
                Debug.Log($"[가방] {_selected.DisplayName} 선택 ({_selected.Width}x{_selected.Height})");
            }
        }

        private void TryPlaceAtMouse()
        {
            if (_selected == null) return;
            if (!TryGetCellUnderMouse(out int x, out int y)) return;

            var instance = new ItemInstance(_selected);

            if (_inventory.TryPlace(instance, x, y))
            {
                Redraw();
                Debug.Log($"[가방] {_selected.DisplayName} 을 ({x},{y}) 에 놓았다");
            }
            else
            {
                Debug.Log($"[가방] ({x},{y}) 에는 놓을 수 없다");
            }
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

        /// 놓인 것을 전부 지우고 그리드를 훑어 다시 그린다.
        /// 25칸이라 비용이 없고, 화면과 그리드가 어긋날 수 없다.
        private void Redraw()
        {
            foreach (Image view in _itemViews)
                Destroy(view.gameObject);

            _itemViews.Clear();
            _drawn.Clear();

            BagGrid grid = _inventory.Grid;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    IGridItem item = grid.GetAt(x, y);

                    // 여러 칸을 덮는 아이템은 처음 만난 칸이 왼쪽 위다.
                    if (item == null || !_drawn.Add(item)) continue;

                    DrawItem((ItemInstance)item, x, y);
                }
            }
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
