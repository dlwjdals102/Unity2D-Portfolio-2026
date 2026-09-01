using UnityEngine;
using UnityEngine.SceneManagement;

namespace JM2D.Core
{
    /// 버튼의 OnClick 에 연결해서 쓴다.
    public class SceneLoader : MonoBehaviour
    {
        public void LoadGameplay()
        {
            SceneManager.LoadScene(SceneNames.Gameplay);
        }
    }
}
