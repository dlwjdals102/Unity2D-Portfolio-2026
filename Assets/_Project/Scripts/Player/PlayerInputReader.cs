using UnityEngine;
using UnityEngine.InputSystem;

namespace JM2D.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        private PlayerControls _controls;

        /// 가방이나 보상 창이 열려 있는가. 열린 쪽이 켜고 끈다.
        private bool _isFireBlocked;

        /// 플레이어에서 마우스를 향하는 방향. 크기는 1.
        public Vector2 AimDirection { get; private set; }
        /// 이번 프레임의 이동 입력. 크기는 0~1.
        public Vector2 MoveInput { get; private set; }
        /// 대시를 눌렀고 아직 소비되지 않았는가.
        public bool DashRequested { get; private set; }
        /// 발사 버튼이 지금 눌려 있는가.
        public bool FireHeld { get; private set; }

#if UNITY_EDITOR
        /// 이번 프레임에 누른 무기 칸. 0, 1, 2 이고 누르지 않았으면 -1.
        /// 확인용이라 입력 에셋이 아니라 키보드를 바로 읽는다. 가방의 Tab 과 같다.
        ///
        /// 무기를 고르는 정식 경로는 마을의 무기대다. 이 길은 해금을 지나쳐 잠긴 무기도 들려서
        /// 빌드에 남기지 않는다. 개발 중에 던전에서 무기를 바꿔 보는 데만 쓴다.
        public int WeaponSlotPressed { get; private set; }
#endif

        private void Awake()
        {
            _controls = new PlayerControls();
        }
        private void OnEnable() 
        {
            _controls.Player.Enable();
            _controls.Player.Dash.performed += OnDashPerformed;
        }
        private void OnDisable() 
        {
            _controls.Player.Dash.performed -= OnDashPerformed;
            _controls.Player.Disable();
        }
        private void OnDestroy()
        {
            _controls.Dispose();
        }

        private void Update() 
        {
            // 게임패드 미세 조작을 살리려고 normalized 대신 ClampMagnitude를 쓴다
            MoveInput = Vector2.ClampMagnitude(_controls.Player.Move.ReadValue<Vector2>(), 1f);

            Vector2 screenPos = _controls.Player.Aim.ReadValue<Vector2>();
            Vector3 worldPos = _camera.ScreenToWorldPoint(screenPos);
            AimDirection = ((Vector2)worldPos - (Vector2)transform.position).normalized;

            // 창이 열려 있으면 누르고 있어도 눌리지 않은 것으로 읽는다.
            // 창을 보려고 누른 클릭이 공격으로 나가지 않게 하는 자리다.
            FireHeld = !_isFireBlocked && _controls.Player.Fire.IsPressed();

#if UNITY_EDITOR
            WeaponSlotPressed = ReadWeaponSlot();
#endif
        }

        /// 창을 여는 쪽이 공격 입력을 막는다. 가방과 보상 창 둘이 쓴다.
        /// 둘이 동시에 열리지 않아 참거짓 하나로 두었다. 세 번째가 생기면 겹침을 세야 한다.
        public void SetFireBlocked(bool blocked) { _isFireBlocked = blocked; }

        private void OnDashPerformed(InputAction.CallbackContext ctx) { DashRequested = true; }
        /// PlayerMotor가 대시 요청을 가져갔다고 알린다.
        public void ConsumeDashRequest() { DashRequested = false; }

#if UNITY_EDITOR
        private int ReadWeaponSlot()
        {
            if (Keyboard.current == null) return -1;

            if (Keyboard.current.digit1Key.wasPressedThisFrame) return 0;
            if (Keyboard.current.digit2Key.wasPressedThisFrame) return 1;
            if (Keyboard.current.digit3Key.wasPressedThisFrame) return 2;

            return -1;
        }
#endif
    }
}
