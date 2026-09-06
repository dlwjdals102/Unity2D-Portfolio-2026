namespace JM2D.Logic
{
    /// 그리드에 놓인 아이템 하나와 그 왼쪽 위 좌표.
    /// 그리드는 위치를 저장하지 않는다. 물어볼 때 찾아서 이 모양으로 돌려준다.
    public readonly struct PlacedItem
    {
        public readonly IGridItem Item;
        public readonly int X;
        public readonly int Y;

        public PlacedItem(IGridItem item, int x, int y)
        {
            Item = item;
            X = x;
            Y = y;
        }
    }
}
