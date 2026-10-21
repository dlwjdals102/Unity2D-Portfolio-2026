using System.IO;
using UnityEditor;
using UnityEngine;
using JM2D.Core;
using JM2D.Logic.Progress;

namespace JM2D.Editor
{
    /// 저장 파일을 눈으로 확인하는 자리. 메뉴 > JM2D 에 들어간다.
    /// 에디터 어셈블리라 빌드에 들어가지 않고, 씬에 오브젝트를 두지 않아 지우는 것을 잊을 일도 없다.
    ///
    /// 게임이 저장을 부르는 자리는 아직 없다(계단 4에서 잇는다).
    /// 그때까지 읽고 쓰는 것이 도는지 보려고 만든 것이다.
    public static class SaveFileMenu
    {
        private const string Root = "JM2D/저장 파일/";

        private static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

        [MenuItem(Root + "자리 열기")]
        private static void OpenFolder()
        {
            Debug.Log(FilePath);

            EditorUtility.RevealInFinder(FilePath);
        }

        [MenuItem(Root + "읽어 보기")]
        private static void LoadAndLog()
        {
            SaveData save = new SaveFile().Load();

            Debug.Log($"포맷 {save.Version}  보스 처치 {save.BossKills}  " +
                      $"최고 기록 {save.BestRoomsReached}번째 방  고른 무기 {save.SelectedWeapon}");

            for (int i = 0; i < UnlockRules.WeaponCount; i++)
            {
                string state = UnlockRules.IsUnlocked(i, save)
                    ? "열림"
                    : $"잠김 ({UnlockRules.ConditionText(i)})";

                Debug.Log($"  무기 {i}  {state}");
            }
        }

        /// 판을 하나 치른 셈 친다. 보스를 잡고 여덟 번째 방까지 간 판이다.
        /// 규칙과 파일이 함께 도는지 보는 것이라 UnlockRules 를 지나간다.
        [MenuItem(Root + "보스 잡은 판 하나 기록하기")]
        private static void RecordWinningRun()
        {
            var file = new SaveFile();

            SaveData after = UnlockRules.After(file.Load(), bossKilled: true, roomsReached: 8);

            file.Save(after);

            Debug.Log($"기록했다. 보스 처치 {after.BossKills}  최고 기록 {after.BestRoomsReached}");
        }

        [MenuItem(Root + "세 번째 방에서 죽은 판 기록하기")]
        private static void RecordLosingRun()
        {
            var file = new SaveFile();

            SaveData after = UnlockRules.After(file.Load(), bossKilled: false, roomsReached: 3);

            file.Save(after);

            Debug.Log($"기록했다. 보스 처치 {after.BossKills}  최고 기록 {after.BestRoomsReached}");
        }

        /// 파일이 없을 때 처음 상태로 시작하는지 보려고 쓴다.
        /// 확인 창을 띄우는 것은 눌러 놓고 잊어버리는 일을 막으려는 것이다.
        [MenuItem(Root + "저장 파일 지우기")]
        private static void DeleteFile()
        {
            if (!File.Exists(FilePath))
            {
                Debug.Log("지울 파일이 없다.");
                return;
            }

            bool ok = EditorUtility.DisplayDialog(
                "저장 파일 지우기",
                $"이 파일을 지운다.\n{FilePath}",
                "지운다", "그만둔다");

            if (!ok) return;

            File.Delete(FilePath);

            Debug.Log("지웠다. 다음 로드는 처음 상태로 시작한다.");
        }
    }
}
