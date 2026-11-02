using UnityEngine;
using UnityEngine.InputSystem;
using JM2D.Player;

namespace JM2D.Town
{
    /// 가까이 가면 안내가 뜨고 E 를 누르면 무언가 일어난다. 무엇이 일어나는지는 자식이 정한다.
    [RequireComponent(typeof(Collider2D))]
    public abstract class Interactable : MonoBehaviour
    {
        [Tooltip("가까이 가면 켜지는 안내. 비워 두면 아무것도 띄우지 않는다")]
        [SerializeField] private GameObject _prompt;

        private bool _isPlayerNear;

        private void Awake()
        {
            ShowPrompt(false);
        }

        private void Update()
        {
            if (!_isPlayerNear || Keyboard.current == null) return;

            if (Keyboard.current.eKey.wasPressedThisFrame) Interact();
        }

        /// 태그는 오타가 조용히 지나가고 레이어는 마을에 하나를 더 만들어야 한다.
        /// 컴포넌트로 거르면 이름이 바뀔 때 컴파일러가 잡아 준다.
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMotor>() == null) return;

            _isPlayerNear = true;

            // 안내를 켜기 전에 알린다. 무기대가 이때 파일을 읽어, 다른 자리에서 바꾸고 와도 표시가 맞는다.
            OnPlayerNear();

            ShowPrompt(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMotor>() == null) return;

            _isPlayerNear = false;
            ShowPrompt(false);
        }

        private void ShowPrompt(bool show)
        {
            if (_prompt == null) return;

            _prompt.SetActive(show);
        }

        /// E 를 눌렀을 때 일어나는 일. 자식이 채운다.
        protected abstract void Interact();

        /// 안내를 켜기 직전에 불린다. 보일 글이 바뀌지 않는 것(던전 입구)은 그대로 두면 된다.
        protected virtual void OnPlayerNear() { }
    }
}
