using System.Collections.Generic;
using JM2D.Enemy;
using UnityEngine;

namespace JM2D.Data
{
    /// 한 웨이브에 나올 적 한 종류와 그 수.
    [System.Serializable]
    public struct WaveEntry
    {
        [Tooltip("EnemyBase 를 가진 적 프리팹")]
        public EnemyBase Prefab;

        [Min(1)]
        public int Count;
    }

    /// 웨이브 하나. 여러 종류를 섞을 수 있다.
    /// 배열을 클래스로 한 겹 감싼 이유는 Unity 직렬화가 배열의 배열을 저장하지 못하기 때문이다.
    [System.Serializable]
    public struct Wave
    {
        public WaveEntry[] Entries;
    }

    /// 방 하나에서 벌어지는 전투. 웨이브가 적힌 순서대로 나온다.
    /// 에셋을 하나 더 만들면 방에서 나올 수 있는 전투가 하나 는다.
    [CreateAssetMenu(fileName = "EncounterData_", menuName = "JM2D/Encounter Data")]
    public class EncounterData : ScriptableObject
    {
        [Tooltip("앞에서부터 하나씩 나온다. 앞 웨이브를 모두 잡아야 다음이 나온다")]
        [SerializeField] private Wave[] _waves;

        /// 읽기 전용으로 노출한다. 배열을 그대로 주면 밖에서 고쳐 에셋이 오염된다.
        /// 웨이브 안의 Entries 는 struct 안의 배열이라 여기서 막지 못한다. 읽기만 한다.
        public IReadOnlyList<Wave> Waves => _waves;

        /// 빈 칸이 있으면 방을 깰 수 없게 되므로 알린다. 고쳐 주지는 않는다.
        private void OnValidate()
        {
            if (_waves == null || _waves.Length == 0)
            {
                Debug.LogWarning($"{name}: 웨이브가 하나도 없다", this);
                return;
            }

            for (int i = 0; i < _waves.Length; i++)
            {
                WaveEntry[] entries = _waves[i].Entries;

                if (entries == null || entries.Length == 0)
                {
                    Debug.LogWarning($"{name}: {i} 번째 웨이브가 비어 있다", this);
                    continue;
                }

                foreach (WaveEntry entry in entries)
                {
                    if (entry.Prefab == null)
                        Debug.LogWarning($"{name}: {i} 번째 웨이브에 적 프리팹이 빈 칸이 있다", this);
                }
            }
        }
    }
}
