using System.Collections;
using JM2D.Feedback;
using Unity.Cinemachine;
using UnityEngine;

namespace JM2D.Enemy
{
    /// 보스가 2페이즈로 넘어가는 순간의 연출. 보스에 붙인다.
    /// 색과 세기를 이 컴포넌트가 들고 있어, Boss 는 '넘어갔다' 는 것만 알리고 무슨 색인지 모른다.
    /// CinemachineImpulseSource 는 HitReaction 이 요구하므로 함께 따라온다.
    [RequireComponent(typeof(Boss), typeof(HitReaction))]
    public class BossPhaseFeedback : MonoBehaviour
    {
        [Tooltip("2페이즈로 넘어가면 몸이 이 색으로 물든다. 한 번 바뀌면 계속 유지된다. " +
                 "붉은 계열은 피하는 것이 좋다. 적 여섯의 피격 번쩍임이 모두 붉어서 겹친다")]
        [SerializeField] private Color _phase2Color = new Color(0.55f, 0.3f, 0.85f);

        [Tooltip("한 번 흔드는 폭. 0.2 면 타격과 같은 크기다")]
        [SerializeField] private float _shakeForce = 0.2f;

        [Tooltip("몇 번 끊어 흔드는가")]
        [Min(1)]
        [SerializeField] private int _shakeCount = 3;

        [Tooltip("흔드는 사이의 간격. 임펄스 소스의 Impulse Duration(0.2)보다 짧으면 " +
                 "앞의 것이 아직 울리는 중에 겹쳐 한 번으로 읽힌다")]
        [SerializeField] private float _shakeInterval = 0.2f;

        private Boss _boss;
        private HitReaction _reaction;
        private CinemachineImpulseSource _impulse;

        /// 흔드는 중인 코루틴. 시작한 쪽이 멈출 책임을 진다.
        private Coroutine _shaking;

        private void Awake()
        {
            _boss = GetComponent<Boss>();
            _reaction = GetComponent<HitReaction>();
            _impulse = GetComponent<CinemachineImpulseSource>();
        }

        private void OnEnable()
        {
            _boss.OnPhase2Started += OnPhase2Started;
        }

        private void OnDisable()
        {
            _boss.OnPhase2Started -= OnPhase2Started;

            // StopAllCoroutines 를 쓰지 않는다. 같은 오브젝트의 다른 컴포넌트가 돌리는 것까지 멈춘다.
            if (_shaking != null)
                StopCoroutine(_shaking);
        }

        /// 색은 HitReaction 을 거쳐 바꾼다. 스프라이트를 직접 물들이면
        /// 다음 피격의 번쩍임이 끝날 때 1페이즈 색으로 조용히 되돌아간다.
        private void OnPhase2Started()
        {
            _reaction.SetBaseColor(_phase2Color);
            _shaking = StartCoroutine(Shake());
        }

        /// 끊어서 여러 번 흔든다. 한 프레임에 몰아 부르면 임펄스가 전부 합쳐져
        /// 한 번 센 흔들림이 되고, 그러면 타격 셰이크와 구분되지 않는다.
        /// 기다리는 것이 흔드는 것보다 먼저인 이유도 같다. 2페이즈로 넘어가게 한 그 타격이
        /// 이미 흔들고 있어서, 곧바로 흔들면 첫 번째가 거기 묻혀 두 번으로 보인다.
        private IEnumerator Shake()
        {
            for (int i = 0; i < _shakeCount; i++)
            {
                yield return new WaitForSeconds(_shakeInterval);
                _impulse.GenerateImpulseWithForce(_shakeForce);
            }
        }
    }
}
