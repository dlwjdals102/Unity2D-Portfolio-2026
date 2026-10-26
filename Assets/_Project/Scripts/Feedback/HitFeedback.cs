using JM2D.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace JM2D.Feedback
{
    /// 타격 순간 씬 전체에서 한 번 일어나는 일을 모은다. 시간을 짧게 멈추고 타격음을 낸다.
    /// 둘 다 씬에 하나여야 하는 것이다. 시간은 하나뿐이고,
    /// 소리는 맞은 적이 그 프레임에 사라져도 끝까지 들려야 한다.
    [RequireComponent(typeof(AudioSource))]
    public class HitFeedback : MonoBehaviour
    {
        [Tooltip("맞을 때마다 한 번 난다")]
        [SerializeField] private AudioClip _hitClip;

        [Tooltip("창이 열려 시간을 멈춰 두었는지 묻는다. 그 동안에는 멈춤을 풀지 않는다")]
        [SerializeField] private PlayerInputReader _playerInput;

        private AudioSource _audio;

        private float _stopLeft;

        /// 소리를 마지막으로 낸 프레임. 한 번의 공격이 여럿을 맞히면 같은 프레임에 여러 번 불린다.
        /// Time.frameCount 와 겹치지 않는 값으로 시작해야 첫 타격이 막히지 않는다.
        private int _lastSoundFrame = -1;

        private void Awake()
        {
            // 시간을 멈추는 쪽이 책임진다. 멈춘 동안에도 흔들림은 흘러가야 한다.
            // 이 매니저는 MonoBehaviour 가 아니라 씬에 오브젝트가 없다. 인스펙터에 나오지 않는다.
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;

            _audio = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (_stopLeft <= 0f) return;

            // timeScale 이 0 이면 deltaTime 도 0 이라 영원히 줄지 않는다.
            // 멈춘 시간을 재는 값은 멈추지 않는 시간이어야 한다.
            _stopLeft -= Time.unscaledDeltaTime;

            // 창이 열려 있으면 되돌리지 않는다. timeScale 의 주인이 둘이고
            // 히트스톱은 0.01초, 창은 고를 때까지라, 짧은 쪽이 긴 쪽을 풀어 버린다.
            // 보상 창이 뜬 채로 돌아다닐 수 있던 것이 이것 때문이었다.
            // 창이 닫힐 때 1 로 되돌리는 것은 창을 띄운 쪽이 이미 하고 있다.
            if (_stopLeft <= 0f && !_playerInput.IsWindowOpen)
                Time.timeScale = 1f;
        }

        private void OnDisable()
        {
            // 멈춘 채로 사라지면 다음 씬이 멈춘 채 시작한다.
            _stopLeft = 0f;
            Time.timeScale = 1f;
        }

        /// 타격이 일어났다고 알린다. 맞은 쪽마다 한 번 불린다.
        /// 멈춤은 이미 멈춰 있으면 긴 쪽이 남고, 소리는 한 프레임에 한 번만 난다.
        public void Hit(float stopTime)
        {
            // 더하면 창으로 다섯을 한 번에 때릴 때 다섯 배로 멈춘다.
            _stopLeft = Mathf.Max(_stopLeft, stopTime);
            Time.timeScale = 0f;

            // 멈춤이 겹쳐 길어지지 않는 것과 같은 규칙이다. 한 번의 공격은 한 번만 들린다.
            // 소리 길이나 멈춤 시간으로 막으면 그 사이에 들어온 다음 공격까지 조용해진다.
            if (_lastSoundFrame == Time.frameCount) return;

            _lastSoundFrame = Time.frameCount;

            // Play 가 아니다. Play 는 인스펙터에 꽂힌 clip 을 틀고 연달아 부르면 앞의 것을 끊는다.
            _audio.PlayOneShot(_hitClip);
        }
    }
}
