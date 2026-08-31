using UnityEngine;

namespace JM2D.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] private float _moveSpeed = 6f;

        [Header("대시")]
        [SerializeField] private float _dashSpeed = 20f;
        [SerializeField] private float _dashDuration = 0.15f;
        [SerializeField] private float _dashCooldown = 0.6f;
        [SerializeField] private float _dashInputBuffer = 0.15f;

        [SerializeField] private PlayerInputReader _input;
        private Rigidbody2D _rb;

        private Vector2 _dashDirection;
        private float _dashTimeLeft;
        private float _cooldownLeft;
        private float _bufferLeft;
        private Vector2 _facing = Vector2.down;

        public bool IsDashing => _dashTimeLeft > 0f;
        private bool IsDashOnCooldown => _cooldownLeft > 0f;
        private bool HasBufferedDash => _bufferLeft > 0f;
        private bool IsMoving => _input.MoveInput.sqrMagnitude > 0.01f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            // 1. 타이머 감소
            if (_dashTimeLeft > 0f) _dashTimeLeft -= Time.fixedDeltaTime;
            if (_cooldownLeft > 0f) _cooldownLeft -= Time.fixedDeltaTime;
            if (_bufferLeft > 0f) _bufferLeft -= Time.fixedDeltaTime;

            // 2-1. 입력을 버퍼에 담는다
            if (_input.DashRequested)
            {
                _input.ConsumeDashRequest();
                _bufferLeft = _dashInputBuffer;
            }

            // 2-2. 조건이 맞으면 대시를 시작한다
            if (HasBufferedDash && !IsDashing && !IsDashOnCooldown)
            {
                _dashDirection = IsMoving ? _input.MoveInput.normalized : _facing;
                _dashTimeLeft = _dashDuration;
                _cooldownLeft = _dashDuration + _dashCooldown;
                _bufferLeft = 0f;
            }

            // 3. 속도 적용
            if (IsDashing)
                _rb.linearVelocity = _dashDirection * _dashSpeed;
            else
            {
                _rb.linearVelocity = _input.MoveInput * _moveSpeed;

                if (IsMoving)
                    _facing = _input.MoveInput.normalized;
            }
        }
    }
}
