using UnityEngine;
using UnityEngine.SceneManagement;

namespace JM2D.Core
{
    /// 버튼의 OnClick 에 연결해서 쓴다.
    public class SceneLoader : MonoBehaviour
    {
        public void LoadGameplay()
        {
            LoadScene(SceneNames.Gameplay);
        }

        public void LoadTitle()
        {
            LoadScene(SceneNames.Title);
        }

        /// 지금 씬을 처음부터 다시 시작한다.
        public void Restart()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        private void LoadScene(string sceneName)
        {
            // 결과 화면에서 0으로 멈춰둔 시간을 되돌린다.
            // timeScale 은 씬을 다시 로드해도 초기화되지 않는다.
            Time.timeScale = 1f;

            SceneManager.LoadScene(sceneName);
        }
    }
}
