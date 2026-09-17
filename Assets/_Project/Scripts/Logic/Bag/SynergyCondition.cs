namespace JM2D.Logic.Bag
{
    /// 시너지가 무엇을 세는지. 데이터 에셋이 조건을 적을 때 쓴다.
    /// 값 하나가 BagGrid 의 세는 메서드 하나와 짝을 이룬다.
    ///
    /// 에셋에는 이름이 아니라 순서 번호로 저장된다. 새 값은 끝에만 붙인다.
    /// 중간에 끼우면 기존 에셋의 번호가 오류 없이 다른 조건을 가리키게 된다.
    public enum SynergyCondition
    {
        /// 상하좌우로 맞닿은 다른 아이템. CountAdjacent
        Adjacent,

        /// 걸친 행에 있는 다른 아이템. CountInSameRow
        SameRow,

        /// 걸친 열에 있는 다른 아이템. CountInSameColumn
        SameColumn,

        /// 차지한 귀퉁이 칸. CountCorners
        Corner
    }
}
