using System;
using UnityEngine;

namespace JM2D.View
{
    /// 이 캐릭터의 Animator 를 다루는 한 자리. 플레이어와 적이 같은 컴포넌트를 쓴다.
    /// 멈춤과 이동은 스스로 속도를 보고 정하고, 공격은 공격을 아는 쪽이 알려 준다.
    /// Animator 파라미터가 여기 모여 있어 무엇이 있는지 한 파일에서 보인다.
    ///
    /// EnemyBase 의 Phase 나 PlayerMotor 의 IsMoving 을 쓰지 않는 이유가 있다.
    /// 전자는 적을 더할 때마다 Animator 와 함께 고쳐야 하고,
    /// 후자는 입력 기준이라 벽에 막혀 못 가는 동안에도 참이다.
    [RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
    public class CharacterAnimator : MonoBehaviour
    {
        /// 이보다 느리면 멈춘 것으로 친다.
        /// 적끼리 밀리거나 벽에 눌릴 때 생기는 속도를 거르기 위한 값이다.
        /// 가장 느린 적이 2.2 이고 플레이어가 6 이라 둘 다 걸리지 않는다.
        private const float MinMoveSpeed = 0.1f;

        /// 이름으로 매번 찾으면 해시를 다시 계산한다.
        /// 값이 인스턴스마다 다르지 않아 static 이고, 런타임 호출이라 const 가 될 수 없다.
        private static readonly int IsMovingId = Animator.StringToHash("IsMoving");
        private static readonly int AttackId = Animator.StringToHash("Attack");
        private static readonly int AttackSpeedId = Animator.StringToHash("AttackSpeed");

        private Animator _animator;
        private Rigidbody2D _rb;

        /// 휘두르는 동작에서 칼이 뻗는 프레임에 일어난다. 공격 판정을 여기에 맞춘다.
        public event Action AttackFrameReached;

        /// 휘두르는 동작이 끝나는 프레임에 일어난다. 그동안 몸이 돌지 않게 막는 쪽이 듣는다.
        public event Action AttackEnded;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // 제곱근을 뽑지 않으려고 양쪽을 제곱한 채로 견준다. 크기 비교라 결과가 같다.
            _animator.SetBool(IsMovingId, _rb.linearVelocity.sqrMagnitude > MinMoveSpeed * MinMoveSpeed);
        }

        /// 휘두르는 동작을 한 번 돌린다. attacksPerSecond 는 초당 공격 횟수다.
        /// 클립을 1초로 맞춰 두어 그 값이 그대로 배속이 되고, 동작 길이가 공격 간격과 같아진다.
        public void PlayAttack(float attacksPerSecond)
        {
            // 배속을 먼저 넘긴다. 트리거가 앞서면 그 공격만 이전 배속으로 돈다.
            _animator.SetFloat(AttackSpeedId, attacksPerSecond);
            _animator.SetTrigger(AttackId);
        }

        /// 애니메이션 이벤트가 부른다. Unity 콜백이라 이름이 On 으로 시작한다.
        /// 이 이름은 클립 안에 문자열로 적혀 있어 바꾸면 조용히 끊어진다. 고칠 때 클립도 함께 본다.
        /// 같은 오브젝트의 다른 컴포넌트에 같은 이름의 public 메서드를 두지 않는다. 둘 다 불린다.
        public void OnAttackFrame()
        {
            AttackFrameReached?.Invoke();
        }

        /// 애니메이션 이벤트가 부른다. 위와 같은 주의가 그대로 적용된다.
        /// 이름은 클립 안에 문자열로 적혀 있어 바꾸면 조용히 끊어진다.
        public void OnAttackEnd()
        {
            AttackEnded?.Invoke();
        }
    }
}
