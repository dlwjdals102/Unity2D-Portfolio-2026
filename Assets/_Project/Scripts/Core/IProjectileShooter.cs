using JM2D.Combat;

namespace JM2D.Core
{
    /// 씬의 투사체 풀을 받아 쓰는 것.
    /// 프리팹은 씬의 물건을 가리킬 수 없어, 판 도중에 만드는 쪽이 넣어 준다.
    public interface IProjectileShooter
    {
        void SetProjectilePool(ProjectilePool pool);
    }
}
