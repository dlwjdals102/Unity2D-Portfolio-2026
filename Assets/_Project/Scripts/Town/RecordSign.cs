using TMPro;
using UnityEngine;
using JM2D.Core;
using JM2D.Logic.Progress;

namespace JM2D.Town
{
    /// 마을에 세워 둔 기록판. 저장된 기록을 글로 보여준다.
    /// 무기대가 왜 잠겨 있는지를 화면에서 설명하는 자리다.
    ///
    /// 한 번만 읽는다. 기록은 던전에서만 바뀌고, 마을로 돌아올 때 씬이 다시 로드되어
    /// Start 가 또 돈다. 무기대가 다가올 때마다 파일을 다시 읽는 것은 마을 안에서
    /// 고른 무기가 바뀌기 때문이고, 기록은 마을에서 바뀌지 않는다.
    public class RecordSign : MonoBehaviour
    {
        [Tooltip("기록이 적힐 글자. 씬에 적어 둔 내용은 Start 에서 덮어쓴다")]
        [SerializeField] private TMP_Text _text;

        private readonly SaveFile _file = new SaveFile();

        private void Start()
        {
            SaveData save = _file.Load();

            // 한 판도 하지 않았으면 '최고 0번째 방' 이 되어 어색하다.
            // 처음 켠 사람이 보는 글이라 따로 둔다.
            if (save.BestRoomsReached <= 0)
            {
                _text.text = "현재 기록\n아직 없습니다";
                return;
            }

            _text.text = $"현재 기록\n보스 처치 {save.BossKills}회\n최고 {save.BestRoomsReached}번째 방";
        }
    }
}
