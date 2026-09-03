using UnityEngine;

namespace JM2D.Data
{
    /// 투사체가 어떻게 날아가는지를 담는다.
    /// 피해량은 여기 없다. 발사하는 쪽이 정해서 넘긴다.
    [CreateAssetMenu(fileName = "ProjectileData_", menuName = "JM2D/Projectile Data")]
    public class ProjectileData : ScriptableObject
    {
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _lifetime = 3f;

        public float Speed => _speed;
        public float Lifetime => _lifetime;
    }
}
