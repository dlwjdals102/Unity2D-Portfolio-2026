using JM2D.Combat;
using UnityEngine;

namespace JM2D.Data
{
    /// 활. 어떤 투사체를 쏘는지를 더한다. 투사체의 속도와 수명은 그 투사체의 ProjectileData 에 있다.
    /// 풀의 부모는 씬 오브젝트라 여기 담을 수 없어 BowWeapon 컴포넌트가 갖는다.
    [CreateAssetMenu(fileName = "WeaponData_", menuName = "JM2D/Weapon Data/Bow")]
    public class BowWeaponData : WeaponData
    {
        [Header("활")]
        [SerializeField] private Projectile _projectilePrefab;

        public Projectile ProjectilePrefab => _projectilePrefab;
    }
}
