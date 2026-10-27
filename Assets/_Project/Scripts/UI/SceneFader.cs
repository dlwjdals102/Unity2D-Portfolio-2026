using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JM2D.UI
{
    /// 씬이 바뀔 때 화면을 덮는 검은 가림막. 씬마다 하나씩 있다.
    ///
    /// 오브젝트가 씬을 넘지 않는 것이 이 프로젝트의 결정이라, 하나를 들고 다니는 대신
    /// 씬마다 둔다. 들어올 때는 스스로 밝아지고, 나갈 때는 SceneLoader 가 어두워지기를 기다린다.
    public class SceneFader : MonoBehaviour
    {
        [SerializeField] private Image _image;

        [Min(0.01f)]
        [SerializeField] private float _fadeTime = 0.3f;

        /// 씬이 열리는 첫 프레임부터 덮여 있어야 한다. Awake 는 첫 렌더보다 먼저 돌아,
        /// 여기서 알파를 올리면 씬에 어떤 값으로 저장돼 있든 검게 시작한다.
        ///
        /// 이 세 줄이 '알파 1 로 저장해야 한다' 는 숨은 요구를 없앤다. 그 요구가 있던 동안에는
        /// 에디터에서 화면이 검게 덮여 가림막을 꺼 두게 되고, 꺼진 채로 저장하면
        /// 그 씬에서 페이드가 조용히 사라진다. 한 번 겪었다.
        private void Awake()
        {
            SetAlpha(1f);
        }

        /// 씬에 들어오면 밝아진다.
        private void Start()
        {
            StartCoroutine(Fade(1f, 0f));
        }

        /// 밖에서 기다릴 수 있도록 코루틴을 돌려준다. 나갈 때는 SceneLoader 가 돌린다.
        ///
        /// 시간이 멈춰 있어도 돌아야 해서 unscaledDeltaTime 으로 센다.
        /// 결과 창과 Esc 메뉴가 timeScale 을 0 으로 두고, 전환 중에도 0 이다.
        ///
        /// 돌고 있는 것을 멈추는 장치는 두지 않는다. 들어온 직후 0.3초 안에 나가면
        /// 밝아지던 것과 어두워지는 것이 같은 알파를 다퉈 깜빡이는데, 그 뒤에는
        /// 어두워지는 쪽만 남아 정상으로 돌아온다. 알려진 한계로 둔다.
        public IEnumerator Fade(float from, float to)
        {
            float left = _fadeTime;

            while (left > 0f)
            {
                left -= Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(to, from, Mathf.Clamp01(left / _fadeTime)));

                yield return null;
            }

            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            Color color = _image.color;
            color.a = alpha;

            _image.color = color;
        }
    }
}
