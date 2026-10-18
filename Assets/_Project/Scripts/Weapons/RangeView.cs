using UnityEngine;

namespace JM2D.Weapons
{
    /// 근접 무기가 공격한 범위를 잠깐 보인다. 판정에는 관여하지 않는다.
    /// 1x1 네모 스프라이트를 크기와 회전만 바꿔 쓴다.
    ///
    /// 공격 애니메이션이 붙을 때 다시 봤고 고칠 것이 없었다.
    /// MeleeWeapon.Attack 안에서 판정과 나란히 불려, 판정이 칼 뻗는 프레임으로 밀릴 때 함께 밀렸다.
    [RequireComponent(typeof(SpriteRenderer))]
    public class RangeView : MonoBehaviour
    {
        private SpriteRenderer _renderer;

        private float _timeLeft;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.enabled = false;
        }

        private void Update()
        {
            if (_timeLeft <= 0f) return;

            _timeLeft -= Time.deltaTime;

            if (_timeLeft <= 0f)
                _renderer.enabled = false;
        }

        /// 월드 좌표의 중심, 크기, 회전(도)으로 time 초 동안 보인다.
        public void Show(Vector2 center, Vector2 size, float angleDegrees, float time)
        {
            transform.SetPositionAndRotation(center, Quaternion.Euler(0f, 0f, angleDegrees));
            transform.localScale = new Vector3(size.x, size.y, 1f);

            _renderer.enabled = true;
            _timeLeft = time;
        }
    }
}
