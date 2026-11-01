using UnityEngine;

namespace JM2D.UI
{
    /// 게임을 끝내는 버튼에 붙인다. 버튼의 OnClick 에 Quit 을 연결해서 쓴다.
    ///
    /// WebGL 에서는 버튼 자신을 끈다. 브라우저에서 돌아가는 빌드는 탭을 스스로 닫을 수 없고,
    /// Application.Quit 이 무엇을 할지는 플레이어 설정의 webGLCloseOnQuit 이 정한다.
    /// 지금 그 값이 0 이라 눌러도 아무 일도 일어나지 않는다.
    /// 눌리지 않는 버튼이 있는 것이 버튼이 없는 것보다 나쁘다.
    public class QuitButton : MonoBehaviour
    {
        private void Awake()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                gameObject.SetActive(false);
            }
        }

        /// 에디터에서는 아무 일도 하지 않는다. 빌드에서만 확인할 수 있다.
        public void Quit()
        {
            Application.Quit();
        }
    }
}
