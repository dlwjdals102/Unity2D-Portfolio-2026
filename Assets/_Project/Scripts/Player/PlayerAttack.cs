using UnityEngine;
using JM2D.Combat;
using JM2D.Core;

namespace JM2D.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [Header("발사")]
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private float _fireInterval = 0.15f;
        [SerializeField] private int _attackDamage = 1;
        [SerializeField] private Transform _poolParent;

        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerMotor _motor;

        private Pool<Projectile> _pool;
        private float _fireCooldownLeft;    

        private bool CanFire => _fireCooldownLeft <= 0f && !_motor.IsDashing;

        private void Awake()
        {
            _pool = new Pool<Projectile>(_projectilePrefab, _poolParent);
        }

        private void Update()
        {
            if (_fireCooldownLeft > 0f)
                _fireCooldownLeft -= Time.deltaTime;

            if (_input.FireHeld && CanFire)
            {
                Projectile p = _pool.Get();
                p.transform.position = transform.position;
                p.Bind(_pool);
                p.Launch(_input.AimDirection, _attackDamage);

                _fireCooldownLeft = _fireInterval;
            }
        }
    }
}
