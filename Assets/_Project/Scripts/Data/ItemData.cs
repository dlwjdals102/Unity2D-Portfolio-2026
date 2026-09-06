using System.Collections.Generic;
using JM2D.Logic;
using UnityEngine;

namespace JM2D.Data
{
    /// 아이템이 스탯 하나를 어떻게 바꾸는지. 에셋에 담기는 한 줄이다.
    [System.Serializable]
    public struct ItemModifier
    {
        [Tooltip("어느 스탯을 건드리는가")]
        public StatType Target;

        [Tooltip("가산은 그대로 더하고, 승산 가산은 합쳐서 한 번 곱하고, 승산 승산은 각각 곱한다")]
        public ModifierType Type;

        [Tooltip("퍼센트는 0.2 가 20% 다")]
        public float Value;
    }

    /// 아이템 한 종류. 코드가 아니라 데이터다.
    /// 에셋을 하나 더 만들면 아이템이 하나 는다.
    [CreateAssetMenu(fileName = "ItemData_", menuName = "JM2D/Item Data")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string _displayName;

        [Header("가방에서 차지하는 크기")]
        [Tooltip("각 변은 1 아니면 2 다. 최대 네 칸을 넘지 않는다")]
        [Range(1, 2)]
        [SerializeField] private int _width = 1;

        [Range(1, 2)]
        [SerializeField] private int _height = 1;

        [Tooltip("가방에 그려질 색. 도형 프로토타입이라 모양만으로는 구분되지 않는다")]
        [SerializeField] private Color _color = Color.white;

        [Header("효과")]
        [SerializeField] private ItemModifier[] _modifiers;

        [Header("인접 시너지")]
        [Tooltip("상하좌우로 맞닿은 아이템 1개당 얼마씩 붙는지. 비워두면 시너지가 없다")]
        [SerializeField] private ItemModifier[] _adjacencyBonus;

        public string DisplayName => _displayName;
        public int Width => _width;
        public int Height => _height;
        public Color Color => _color;

        /// 읽기 전용으로 노출한다. 배열을 그대로 주면 밖에서 고쳐 에셋이 오염된다.
        public IReadOnlyList<ItemModifier> Modifiers => _modifiers;

        /// 인접한 아이템 1개당 붙는 양. 비어 있으면 시너지가 없다.
        public IReadOnlyList<ItemModifier> AdjacencyBonus => _adjacencyBonus;
    }
}
