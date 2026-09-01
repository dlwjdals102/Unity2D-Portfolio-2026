using JM2D.Combat;
using UnityEngine;

namespace JM2D.Player
{
    [RequireComponent(typeof(Health))]
    public class PlayerDeath : MonoBehaviour
    {
        private Health _health;
        private Rigidbody2D _rb;
        private PlayerInputReader _input;
        private PlayerMotor _motor;
        private PlayerAttack _attack;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _rb = GetComponent<Rigidbody2D>();
            _input = GetComponent<PlayerInputReader>();
            _motor = GetComponent<PlayerMotor>();
            _attack = GetComponent<PlayerAttack>();
        }

        private void OnEnable() { _health.OnDied += OnDied; }
        private void OnDisable() { _health.OnDied -= OnDied; }

        private void OnDied()
        {
            _input.enabled = false;
            _motor.enabled = false;
            _attack.enabled = false;

            _rb.linearVelocity = Vector2.zero;
        }
    }
}
