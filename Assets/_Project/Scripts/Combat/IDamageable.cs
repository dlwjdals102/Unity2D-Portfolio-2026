namespace JM2D.Combat
{
    /// 피해를 받을 수 있는 것. 피해를 주는 쪽은 상대가 무엇인지 몰라도 된다.
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}
