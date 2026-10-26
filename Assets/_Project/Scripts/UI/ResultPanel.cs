using System.Text;
using JM2D.Core;
using JM2D.Logic.Progress;
using TMPro;
using UnityEngine;

namespace JM2D.UI
{
    /// 판이 끝났을 때 뜨는 창. 이겼는지 졌는지와 기록, 그리고 이번 판으로 열린 것을 알린다.
    ///
    /// 죽음과 클리어를 직접 듣지 않고 RunRecorder 가 알려 주는 것만 듣는다.
    /// 같은 이벤트를 따로 들으면 저장보다 먼저 글을 쓸 수 있고, 그러면 기록이 한 판 뒤처진다.
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _message;

        [Tooltip("기록과 해금 안내. 결과 글보다 작고 흐리게 둔다")]
        [SerializeField] private TMP_Text _record;

        [SerializeField] private RunRecorder _recorder;

        /// 씬에 켜 둔 채로 저장해도 시작할 때 꺼진다. 가방과 보상 창도 같은 방식이다.
        private void Awake()
        {
            _root.SetActive(false);
        }

        private void OnEnable()
        {
            _recorder.OnRunFinished += Show;
        }

        private void OnDisable()
        {
            _recorder.OnRunFinished -= Show;
        }

        private void Show(bool bossKilled, SaveData save, int newlyUnlocked)
        {
            _message.text = bossKilled ? "클리어" : "게임 오버";
            _record.text = BuildRecord(save, newlyUnlocked);

            _root.SetActive(true);
            Time.timeScale = 0f;
        }

        /// 누적 기록 한 줄과, 열린 것이 있으면 안내 한 줄.
        ///
        /// 이번 판이 몇 번째 방까지 갔는지는 적지 않는다. 화면의 진행 표시가 이미 말하고 있고,
        /// 죽은 자리에 그 숫자가 그대로 남는다.
        private string BuildRecord(SaveData save, int newlyUnlocked)
        {
            var text = new StringBuilder();

            text.AppendLine($"보스 처치 {save.BossKills}회");
            text.Append($"최고 {save.BestRoomsReached}번째 방");

            if (newlyUnlocked > 0)
            {
                text.AppendLine();
                text.Append("새 무기가 열렸습니다.\n " +
                    "마을의 무기대에서 집으세요");
            }

            return text.ToString();
        }
    }
}
