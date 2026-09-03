namespace JM2D.Logic
{
    /// 스탯에 얹는 값 하나. 만들어진 뒤 바뀌지 않는다.
    public readonly struct StatModifier
    {
        public readonly ModifierType Type;
        public readonly float Value;
        public readonly object Source;

        public StatModifier(ModifierType type, float value, object source = null)
        {
            Type = type;
            Value = value;
            Source = source;
        }
    }
}
