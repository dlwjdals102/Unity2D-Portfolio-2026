using System.Collections;
using JM2D.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JM2D.Core
{
    /// 버튼의 OnClick 에 연결해서 쓴다.
    /// 어두워지기를 기다린 뒤 씬을 바꾸므로, 부르고 나서 한 박자 뒤에 넘어간다.
    public class SceneLoader : MonoBehaviour
    {
        [Tooltip("씬을 넘을 때 어두워지는 가림막. 씬마다 하나씩 있다")]
        [SerializeField] private SceneFader _fader;

        /// 전환이 시작됐는가. 어두워지는 0.3초 사이에 버튼을 두 번 누르는 것을 막는다.
        private bool _isLoading;

        public void LoadGameplay()
        {
            Begin(SceneNames.Gameplay);
        }

        /// 판을 시작하기 전과 끝낸 뒤에 머무는 곳. 타이틀의 시작 버튼이 여기로 보낸다.
        public void LoadTown()
        {
            Begin(SceneNames.Town);
        }

        public void LoadTitle()
        {
            Begin(SceneNames.Title);
        }

        /// 지금 씬을 처음부터 다시 시작한다.
        public void Restart()
        {
            Begin(SceneManager.GetActiveScene().name);
        }

        private void Begin(string sceneName)
        {
            if (_isLoading) return;
            _isLoading = true;

            StartCoroutine(LoadScene(sceneName));
        }

        private IEnumerator LoadScene(string sceneName)
        {
            // 전환 중에는 아무것도 움직이지 않는다. 가림막은 unscaled 시간으로 돌아 영향이 없다.
            Time.timeScale = 0f;

            yield return _fader.Fade(0f, 1f);

            // 결과 화면에서 0으로 멈춰둔 시간도 여기서 함께 풀린다.
            // timeScale 은 씬을 다시 로드해도 초기화되지 않는다.
            Time.timeScale = 1f;

            SceneManager.LoadScene(sceneName);
        }
    }
}
