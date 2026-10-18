using UnityEngine;

namespace JM2D.View
{
    /// 태어날 때 몸이 제 크기로 커진다. 한 프레임에 툭 나타나는 것을 막는다.
    ///
    /// 알파가 아니라 스케일을 쓴다. HitReaction 이 Awake 에서 SpriteRenderer 의 색을
    /// 돌아갈 색으로 캐싱하는데, 같은 오브젝트의 Awake 순서는 보장되지 않는다.
    /// 알파를 0 으로 깔아 두면 그 값이 캐싱돼 한 번 맞은 뒤 투명해질 수 있다.
    public class SpawnScale : MonoBehaviour
    {
        [Tooltip("제 크기가 되기까지 걸리는 시간")]
        [SerializeField] private float _growTime = 0.4f;

        [Tooltip("다 자랄 때까지 꺼 둘 것. 비워 두면 아무것도 끄지 않는다")]
        [SerializeField] private Behaviour _disableWhileGrowing;

        /// 다 자란 크기. 프리팹에 적힌 값이라 코드가 숫자를 들고 있지 않는다.
        private Vector3 _fullScale;

        private float _left;

        private void Awake()
        {
            _fullScale = transform.localScale;
            transform.localScale = Vector3.zero;
            _left = _growTime;

            // 비활성 컴포넌트도 Awake 는 돈다. 여기서 막는 것은 OnEnable 과 Update 다.
            if (_disableWhileGrowing != null)
                _disableWhileGrowing.enabled = false;
        }

        private void Update()
        {
            if (_left <= 0f) return;

            _left -= Time.deltaTime;

            // 0 에서 1 로 간다. 남은 시간을 그대로 쓰면 거꾸로 줄어든다.
            // Clamp01 이 없으면 마지막 프레임에 제 크기를 넘었다가 돌아온다.
            float grown = Mathf.Clamp01(1f - _left / _growTime);

            transform.localScale = _fullScale * grown;

            if (_left <= 0f && _disableWhileGrowing != null)
                _disableWhileGrowing.enabled = true;
        }
    }
}
