using System;
using JM2D.Combat;
using JM2D.Enemy;
using UnityEngine;

namespace JM2D.Core
{
    /// 살아 있는 적이 모두 죽으면 한 번 알린다.
    /// 적이 태어날 때 스스로 알리므로, 씬에 놓인 적과 판 도중에 태어난 적을 같은 길로 센다.
    [DefaultExecutionOrder(-100)]
    public class EnemyCounter : MonoBehaviour
    {
        public event Action OnAllEnemiesDead;

        private int _alive;
        private bool _hasDeathToCheck;

        /// 씬이 열릴 때 적의 Awake 보다 먼저 구독해야 해서 실행 순서를 앞당겼다.
        private void OnEnable()
        {
            EnemyBase.Spawned += OnEnemySpawned;
        }

        /// 0 판정은 여기서 한다. 죽는 순간 바로 보면 그 죽음에서 갈라져 나올 적이 아직 없다.
        private void LateUpdate()
        {
            if (!_hasDeathToCheck) return;
            _hasDeathToCheck = false;

            if (_alive == 0)
                OnAllEnemiesDead?.Invoke();
        }

        private void OnDisable()
        {
            EnemyBase.Spawned -= OnEnemySpawned;
        }

        private void OnEnemySpawned(Health health)
        {
            _alive++;
            // 해제하지 않는다. 구독 목록은 적의 Health 가 들고 있어 적이 파괴될 때 함께 사라진다.
            // 적을 풀링해 Health 를 다시 쓰게 되면 두 번 구독되므로 그때 해제를 넣는다.
            health.OnDied += OnEnemyDied;
        }

        private void OnEnemyDied()
        {
            _alive--;
            _hasDeathToCheck = true;
        }
    }
}
