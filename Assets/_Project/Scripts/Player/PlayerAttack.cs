using UnityEngine;
using JM2D.Combat;

namespace JM2D.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [Header("발사")]
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private float _fireInterval = 0.15f;

        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerMotor _motor;

        private float _fireCooldownLeft;

        private bool CanFire => _fireCooldownLeft <= 0f && !_motor.IsDashing;

        private void Update()
        {
            if (_fireCooldownLeft > 0f)
                _fireCooldownLeft -= Time.deltaTime;

            if (_input.FireHeld && CanFire)
            {
                Projectile p = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
                p.Launch(_input.AimDirection);

                _fireCooldownLeft = _fireInterval;
            }
        }
    }
}
