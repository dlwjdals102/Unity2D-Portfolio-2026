namespace JM2D.Logic.Bag
{
    /// 그리드에 놓이는 것. 그리드는 이것이 무엇인지 모르고 차지하는 크기만 안다.
    /// Logic 이 Runtime 을 참조할 수 없어 여기서 정의하고 Runtime 쪽에서 구현한다.
    /// 회전이 반영된 크기를 돌려주는 것이 구현의 책임이라 그리드는 회전을 모른다.
    public interface IGridItem
    {
        int Width { get; }
        int Height { get; }
    }
}
