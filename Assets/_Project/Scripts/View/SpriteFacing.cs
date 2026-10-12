using UnityEngine;

namespace JM2D.View
{
    /// 바라보는 쪽으로 스프라이트를 뒤집는다. 어느 쪽을 볼지는 밖에서 정해 준다.
    /// 플레이어는 조준 방향, 적은 가는 방향을 넘긴다.
    /// 스프라이트가 오른쪽을 보고 있다고 가정한다.
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFacing : MonoBehaviour
    {
        /// 이보다 작은 x 는 위아래로 본 것으로 치고 지금 방향을 유지한다.
        private const float Deadzone = 0.01f;

        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        /// 바라볼 방향의 x 성분을 넘긴다. 양수면 오른쪽, 음수면 왼쪽이다.
        public void Face(float x)
        {
            // 위아래로만 볼 때 x 가 0 이 된다. 그때 뒤집으면 오른쪽으로 튄다.
            if (Mathf.Abs(x) < Deadzone) return;

            _renderer.flipX = x < 0f;
        }
    }
}
