using Unity.Cinemachine;
using UnityEngine;

namespace JM2D.Feedback
{
    /// 타격 순간 시간을 짧게 멈춘다. 시간은 하나뿐이므로 씬에 하나만 둔다.
    public class HitStop : MonoBehaviour
    {
        private float _stopLeft;

        private void Awake()
        {
            // 시간을 멈추는 쪽이 책임진다. 멈춘 동안에도 흔들림은 흘러가야 한다.
            // 이 매니저는 MonoBehaviour 가 아니라 씬에 오브젝트가 없다. 인스펙터에 나오지 않는다.
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
        }

        private void Update()
        {
            if (_stopLeft <= 0f) return;

            // timeScale 이 0 이면 deltaTime 도 0 이라 영원히 줄지 않는다.
            // 멈춘 시간을 재는 값은 멈추지 않는 시간이어야 한다.
            _stopLeft -= Time.unscaledDeltaTime;

            if (_stopLeft <= 0f)
                Time.timeScale = 1f;
        }

        private void OnDisable()
        {
            // 멈춘 채로 사라지면 다음 씬이 멈춘 채 시작한다.
            _stopLeft = 0f;
            Time.timeScale = 1f;
        }

        /// 멈춤을 요청한다. 이미 멈춰 있으면 긴 쪽이 남는다.
        public void Stop(float duration)
        {
            // 더하면 창으로 다섯을 한 번에 때릴 때 다섯 배로 멈춘다.
            _stopLeft = Mathf.Max(_stopLeft, duration);
            Time.timeScale = 0f;
        }
    }
}
