using System;
using JM2D.Combat;
using JM2D.Enemy;
using UnityEngine;

namespace JM2D.Core
{
    /// 씬에 놓인 적이 전부 죽으면 알린다.
    /// EnemyBase 로 찾으므로 적 종류가 늘어도 고치지 않는다.
    public class EnemyCounter : MonoBehaviour
    {
        public event Action OnAllEnemiesDead;

        private int _alive;

        private void Start()
        {
            var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            _alive = enemies.Length;

            foreach (var enemy in enemies)
                enemy.GetComponent<Health>().OnDied += OnEnemyDied;
        }

        private void OnEnemyDied()
        {
            _alive--;

            if (_alive <= 0)
                OnAllEnemiesDead?.Invoke();
        }
    }
}
