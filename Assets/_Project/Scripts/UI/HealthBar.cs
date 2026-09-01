using JM2D.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace JM2D.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private Image _fill;

        private void OnEnable()
        {
            _health.OnHealthChanged += Refresh;
        }

        private void Start()
        {
            Refresh(_health.Current, _health.Max);
        }

        private void OnDisable()
        {
            _health.OnHealthChanged -= Refresh;
        }

        

        private void Refresh(int current, int max)
        {
            _fill.fillAmount = (float)current / max;
        }
    }
}
