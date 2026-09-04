using JM2D.Data;

namespace JM2D.Items
{
    /// 가방에 들어 있는 아이템 한 개.
    /// 같은 종류 아이템 둘을 구분하려고 존재한다. 검 두 자루는
    /// 같은 ItemData 를 가리키는 서로 다른 ItemInstance 다.
    public class ItemInstance
    {
        public ItemData Data { get; }

        public ItemInstance(ItemData data)
        {
            Data = data;
        }
    }
}
