namespace JM2D.Logic
{
    /// 모디파이어가 어떻게 적용되는지. 선언 순서가 곧 계산 순서다.
    public enum ModifierType
    {
        /// 그냥 더한다. 공격력 +5
        Flat,

        /// 같은 종류끼리 더한 뒤 한 번 곱한다. +20% 둘이면 +40%
        PercentAdd,

        /// 각각 따로 곱한다. +20% 둘이면 +44%
        PercentMult
    }
}
