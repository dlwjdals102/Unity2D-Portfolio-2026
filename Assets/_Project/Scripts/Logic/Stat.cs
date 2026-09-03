using System;
using System.Collections.Generic;

namespace JM2D.Logic
{
    /// 기본값에 모디파이어를 얹어 최종값을 낸다.
    /// UnityEngine 에 의존하지 않는다. 씬도 시간도 모른다.
    public class Stat
    {
        /// 모디파이어가 늘거나 줄었을 때 발생한다.
        /// 값이 실제로 달라졌는지는 보지 않는다.
        public event Action OnChanged;

        private readonly float _baseValue;
        private readonly List<StatModifier> _modifiers = new List<StatModifier>();

        private float _cachedValue;
        private bool _isDirty = true;

        public Stat(float baseValue)
        {
            _baseValue = baseValue;
        }

        public float Value
        {
            get
            {
                if (_isDirty)
                {
                    _cachedValue = Calculate();
                    _isDirty = false;
                }

                return _cachedValue;
            }
        }

        /// 정수 스탯용. 반올림 규칙이 이 한 곳에만 있다.
        public int IntValue => (int)Math.Round(Value, MidpointRounding.AwayFromZero);

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            MarkChanged();
        }

        public void RemoveAll()
        {
            _modifiers.Clear();
            MarkChanged();
        }

        /// 특정 출처가 붙인 모디파이어만 전부 제거한다.
        public void RemoveAllFrom(object source)
        {
            _modifiers.RemoveAll(m => m.Source == source);
            MarkChanged();
        }

        private float Calculate()
        {
            float flatSum = 0f;
            float percentAddSum = 0f;
            float percentMultProduct = 1f;

            foreach (StatModifier modifier in _modifiers)
            {
                switch (modifier.Type)
                {
                    case ModifierType.Flat:
                        flatSum += modifier.Value;
                        break;

                    case ModifierType.PercentAdd:
                        percentAddSum += modifier.Value;
                        break;

                    case ModifierType.PercentMult:
                        percentMultProduct *= (1f + modifier.Value);
                        break;
                }
            }

            return (_baseValue + flatSum) * (1f + percentAddSum) * percentMultProduct;
        }

        private void MarkChanged()
        {
            _isDirty = true;
            OnChanged?.Invoke();
        }
    }
}
