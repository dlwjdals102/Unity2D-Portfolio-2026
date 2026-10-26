using JM2D.Core;
using JM2D.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JM2D.UI
{
    /// Esc 로 열고 닫는 메뉴. 열려 있으면 시간이 멈추고 공격 입력이 막힌다.
    /// 던전과 마을에 같은 것이 하나씩 있다.
    ///
    /// Esc 는 보상 창이 '고르지 않고 넘어가기' 로 먼저 쓰고 있다. Update 가 도는 순서는
    /// 보장되지 않으므로 둘이 같은 프레임에 그 키를 읽는다. 그래서 다른 창이 열려 있는
    /// 동안에는 이쪽이 Esc 를 받지 않는다.
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;

        [SerializeField] private PlayerInputReader _playerInput;
        [SerializeField] private SceneLoader _loader;

        private bool IsOpen => _panel.activeSelf;

        /// 시간이 멈춘 동안에도 Update 는 돈다. 보상 창이 그 성질을 이미 쓰고 있다.
        private void Update()
        {
            if (Keyboard.current == null) return;
            if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

            // 둘은 다른 때를 막는다. 앞의 것은 창이 떠 있는 동안이고,
            // 뒤의 것은 창이 이 프레임에 Esc 로 닫히는 순간이다.
            if (IsOpen) Resume();
            else if (!_playerInput.IsWindowOpen && !_playerInput.IsEscapeUsedThisFrame) Open();
        }

        private void Open()
        {
            _panel.SetActive(true);
            Time.timeScale = 0f;
            _playerInput.SetFireBlocked(true);
        }

        /// 버튼이 부르고 Esc 로도 부른다.
        public void Resume()
        {
            _panel.SetActive(false);
            Time.timeScale = 1f;
            _playerInput.SetFireBlocked(false);
        }

        /// 버튼이 부른다. 멈춘 것은 SceneLoader 가 되돌린다.
        public void GoTitle()
        {
            _loader.LoadTitle();
        }

        /// 버튼이 부른다. 에디터에서는 아무 일도 하지 않는다.
        public void Quit()
        {
            Application.Quit();
        }
    }
}
