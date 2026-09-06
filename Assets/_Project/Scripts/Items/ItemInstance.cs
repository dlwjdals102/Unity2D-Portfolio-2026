using JM2D.Data;
using JM2D.Logic;

namespace JM2D.Items
{
    /// 가방에 들어 있는 아이템 한 개.
    /// 같은 종류 아이템 둘을 구분하려고 존재한다. 검 두 자루는
    /// 같은 ItemData 를 가리키는 서로 다른 ItemInstance 다.
    public class ItemInstance : IGridItem
    {
        public ItemData Data { get; }

        /// 눕혔는가. 그리드는 이 사실을 모르고 크기만 본다.
        public bool IsRotated { get; private set; }

        public int Width => IsRotated ? Data.Height : Data.Width;
        public int Height => IsRotated ? Data.Width : Data.Height;

        public ItemInstance(ItemData data)
        {
            Data = data;
        }

        /// 놓기 전에만 부른다. 놓은 뒤에는 빼서 다시 놓는다.
        public void Rotate()
        {
            IsRotated = !IsRotated;
        }
    }
}
