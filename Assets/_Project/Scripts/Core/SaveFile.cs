using System;
using System.IO;
using UnityEngine;
using JM2D.Logic.Progress;

namespace JM2D.Core
{
    /// SaveData 를 파일에 쓰고 읽는다. 해금 규칙은 모르고, 씬에 붙지 않는다.
    /// 파일이 씬 사이의 전달 수단이라 씬을 넘나드는 오브젝트나 싱글턴을 두지 않는다.
    public class SaveFile
    {
        /// 포맷이 바뀌면 올린다. 저장된 값이 이것과 다르면 기본값으로 시작한다.
        private const int CurrentVersion = 1;

        private const string FileName = "save.json";

        /// 빌드와 에디터가 서로 다른 자리를 쓴다. Assets 아래에 두면 빌드에 포함되지 않는다.
        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        /// 어떤 경우에도 null 을 돌려주지 않는다. 읽을 수 없으면 처음 켠 것으로 친다.
        public SaveData Load()
        {
            if (!File.Exists(FilePath)) return NewSave();

            try
            {
                string json = File.ReadAllText(FilePath);
                SaveData save = JsonUtility.FromJson<SaveData>(json);

                // FromJson 은 모양만 맞으면 통과해, 빈 중괄호 하나에도 모든 필드가 0인 객체를 돌려준다.
                if (save == null || save.Version != CurrentVersion) return NewSave();

                return save;
            }
            catch (Exception e)
            {
                // 복구한 상황이라 멈추지 않는다. 조용히 삼키면 '저장이 안 된다' 의 원인을 찾을 수 없다.
#if UNITY_EDITOR
                Debug.LogWarning($"저장 파일을 읽지 못해 처음 상태로 시작한다. {e.Message}");
#endif
                return NewSave();
            }
        }

        /// 값이 바뀔 때마다 부른다. 끌 때 한 번 쓰는 방식은 OnApplicationQuit 가 불리지 않으면 기록을 잃는다.
        public void Save(SaveData save)
        {
            save.Version = CurrentVersion;

            // 두 번째 인자가 줄을 나눠 쓴다. 사람이 열어 고칠 수 있어야 '망가뜨려도 멈추지 않는다' 를 확인한다.
            string json = JsonUtility.ToJson(save, true);

            try
            {
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                // 저장에 실패해도 플레이를 멈출 이유는 없다. 다음 저장에서 다시 시도한다.
#if UNITY_EDITOR
                Debug.LogWarning($"저장하지 못했다. {e.Message}");
#endif
            }
        }

        private static SaveData NewSave()
        {
            return new SaveData { Version = CurrentVersion };
        }
    }
}
