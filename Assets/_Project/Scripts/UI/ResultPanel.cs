using JM2D.Combat;
using JM2D.Core;
using TMPro;
using UnityEngine;

namespace JM2D.UI
{
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] private Health _playerHealth;
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _message;
        [SerializeField] private EnemyCounter _enemyCounter;

        private void OnEnable()
        {
            _playerHealth.OnDied += ShowGameOver;
            _enemyCounter.OnAllEnemiesDead += ShowClear;
        }

        private void OnDisable()
        {
            _playerHealth.OnDied -= ShowGameOver;
            _enemyCounter.OnAllEnemiesDead -= ShowClear;
        }

        private void ShowGameOver()
        {
            Show("게임 오버");
        }

        private void ShowClear()
        {
            Show("클리어");
        }

        private void Show(string message)
        {
            _message.text = message;
            _root.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
