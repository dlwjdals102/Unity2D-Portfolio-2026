using UnityEngine;
using UnityEngine.InputSystem;
using JM2D.Player;

namespace JM2D.Town
{
    /// 가까이 가면 안내가 뜨고 E 를 누르면 무언가 일어나는 것.
    /// 무엇이 일어나는지는 자식이 정한다. 던전 입구와 무기 선택대가 자식이다.
    [RequireComponent(typeof(Collider2D))]
    public abstract class Interactable : MonoBehaviour
    {
        [Tooltip("가까이 가면 켜지는 안내. 비워 두면 아무것도 띄우지 않는다")]
        [SerializeField] private GameObject _prompt;

        /// 플레이어가 범위 안에 있는가. 트리거로 판단한다.
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

        /// 마을에서 움직이는 것은 플레이어뿐이지만 무엇을 기준으로 거를지는 정해야 한다.
        /// 태그는 오타가 조용히 지나가고 레이어는 마을에 하나를 더 만들어야 한다.
        /// 컴포넌트로 거르면 이름이 바뀔 때 컴파일러가 잡아 준다.
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMotor>() == null) return;

            _isPlayerNear = true;

            // 안내를 켜기 전에 자식에게 알린다. 무기대는 이때 저장 파일을 읽어 글을 다시 쓴다.
            // 그래야 다른 무기대에서 무기를 바꾸고 와도 '지금 든 것' 표시가 어긋나지 않는다.
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

        /// 플레이어가 막 다가왔을 때. 안내를 켜기 직전에 불린다.
        /// 보일 글이 바뀌지 않는 것(던전 입구)은 그대로 두면 된다.
        protected virtual void OnPlayerNear() { }
    }
}
