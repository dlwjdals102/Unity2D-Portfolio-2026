using UnityEngine;

namespace JM2D.Data
{
    /// 검과 창. 무기 앞에 놓이는 사각형 범위를 더한다. 두 무기는 이 값만 다르고 코드는 같다.
    [CreateAssetMenu(fileName = "WeaponData_", menuName = "JM2D/Weapon Data/Melee")]
    public class MeleeWeaponData : WeaponData
    {
        [Header("범위")]
        [Tooltip("플레이어 중심에서 조준 방향으로 닿는 길이. 적의 몸이 범위에 걸리면 맞는다")]
        [SerializeField] private float _length = 1.6f;

        [Tooltip("조준 방향과 직각인 폭")]
        [SerializeField] private float _width = 2f;

        public float Length => _length;
        public float Width => _width;
    }
}
