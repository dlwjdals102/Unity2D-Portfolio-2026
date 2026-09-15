using UnityEngine;
using UnityEngine.InputSystem;

namespace JM2D.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        private PlayerControls _controls;

        /// 플레이어에서 마우스를 향하는 방향. 크기는 1.
        public Vector2 AimDirection { get; private set; }
        /// 이번 프레임의 이동 입력. 크기는 0~1.
        public Vector2 MoveInput { get; private set; }
        /// 대시를 눌렀고 아직 소비되지 않았는가.
        public bool DashRequested { get; private set; }
        /// 발사 버튼이 지금 눌려 있는가.
        public bool FireHeld { get; private set; }

        /// 이번 프레임에 누른 무기 칸. 0, 1, 2 이고 누르지 않았으면 -1.
        /// 확인용이라 입력 에셋이 아니라 키보드를 바로 읽는다. 가방의 Tab 과 같다.
        public int WeaponSlotPressed { get; private set; }

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

            FireHeld = _controls.Player.Fire.IsPressed();
            WeaponSlotPressed = ReadWeaponSlot();
        }

        private void OnDashPerformed(InputAction.CallbackContext ctx) { DashRequested = true; }
        /// PlayerMotor가 대시 요청을 가져갔다고 알린다.
        public void ConsumeDashRequest() { DashRequested = false; }

        private int ReadWeaponSlot()
        {
            if (Keyboard.current == null) return -1;

            if (Keyboard.current.digit1Key.wasPressedThisFrame) return 0;
            if (Keyboard.current.digit2Key.wasPressedThisFrame) return 1;
            if (Keyboard.current.digit3Key.wasPressedThisFrame) return 2;

            return -1;
        }
    }
}
