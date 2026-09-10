using NUnit.Framework;
using JM2D.Logic;

namespace JM2D.Tests
{
    public class BagGridTests
    {
        /// 테스트용 가짜 아이템. 크기만 있으면 그리드는 만족한다.
        /// Runtime 의 ItemInstance 를 쓰지 않아도 되는 것이 IGridItem 의 값어치다.
        private class 아이템 : IGridItem
        {
            public int Width { get; }
            public int Height { get; }

            public 아이템(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        private static BagGrid 새그리드() => new BagGrid(5, 5);

        [Test]
        public void 빈_그리드에_놓으면_성공한다()
        {
            var grid = 새그리드();

            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 0, 0));
        }

        [Test]
        public void 같은_칸에_또_놓으면_실패한다()
        {
            var grid = 새그리드();
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 2, 2), "준비: 첫 배치");

            Assert.IsFalse(grid.TryPlace(new 아이템(1, 1), 2, 2));
        }

        /// 한 칸만 겹쳐도 실패해야 한다.
        /// 왼쪽 위 칸만 검사하는 구현이면 여기서 통과해 버린다.
        [Test]
        public void 한_칸만_겹쳐도_실패한다()
        {
            var grid = 새그리드();
            Assert.IsTrue(grid.TryPlace(new 아이템(2, 1), 1, 1), "준비: (1,1) (2,1) 차지");

            Assert.IsFalse(grid.TryPlace(new 아이템(2, 2), 2, 0));   // (2,1) 에서 부딪힌다
        }

        [Test]
        public void 그리드_밖으로_나가면_실패한다()
        {
            var grid = 새그리드();

            Assert.IsFalse(grid.TryPlace(new 아이템(1, 1), 5, 0));
            Assert.IsFalse(grid.TryPlace(new 아이템(1, 1), -1, 0));
        }

        /// 왼쪽 위만 보면 x=4 가 5 보다 작아 통과해 버린다.
        /// 오른쪽 아래 끝을 봐야 잡힌다.
        [Test]
        public void 오른쪽_아래로_넘치면_실패한다()
        {
            var grid = 새그리드();

            Assert.IsFalse(grid.TryPlace(new 아이템(2, 2), 4, 4));
        }

        [Test]
        public void 오른쪽_아래_끝에_딱_맞으면_성공한다()
        {
            var grid = 새그리드();

            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 4, 4));
            Assert.IsTrue(grid.TryPlace(new 아이템(2, 2), 0, 3));
        }

        [Test]
        public void 놓으면_차지한_칸이_전부_찬다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 1);
            Assert.IsTrue(grid.TryPlace(검, 1, 1), "준비: 검 배치");

            Assert.AreSame(검, grid.GetAt(1, 1));
            Assert.AreSame(검, grid.GetAt(2, 1));
            Assert.IsNull(grid.GetAt(3, 1));
            Assert.IsNull(grid.GetAt(1, 2));
        }

        [Test]
        public void 빼면_차지하던_칸이_전부_빈다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 2);
            Assert.IsTrue(grid.TryPlace(검, 1, 1), "준비: 검 배치");

            grid.Remove(검);

            Assert.IsNull(grid.GetAt(1, 1));
            Assert.IsNull(grid.GetAt(2, 1));
            Assert.IsNull(grid.GetAt(1, 2));
            Assert.IsNull(grid.GetAt(2, 2));
        }

        [Test]
        public void 뺀_자리에_다시_놓을_수_있다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 2);
            Assert.IsTrue(grid.TryPlace(검, 1, 1), "준비: 검 배치");
            grid.Remove(검);

            Assert.IsTrue(grid.TryPlace(new 아이템(2, 2), 1, 1));
        }

        /// 하나만 뺐는데 다른 것까지 사라지면 안 된다.
        [Test]
        public void 뺄_때_다른_아이템은_남는다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            var 부츠 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 0, 0), "준비: 검 배치");
            Assert.IsTrue(grid.TryPlace(부츠, 1, 0), "준비: 부츠 배치");

            grid.Remove(검);

            Assert.IsNull(grid.GetAt(0, 0));
            Assert.AreSame(부츠, grid.GetAt(1, 0));
        }

        [Test]
        public void 스물다섯_개를_채우면_더_못_놓는다()
        {
            var grid = 새그리드();

            for (int y = 0; y < 5; y++)
                for (int x = 0; x < 5; x++)
                    Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), x, y), $"({x},{y}) 에서 실패");

            Assert.IsFalse(grid.TryPlace(new 아이템(1, 1), 0, 0));
        }

        /// 세로로는 안 들어가는 자리에 눕혀서 넣는다.
        /// 회전 자체는 아이템 쪽 일이라, 그리드는 크기가 다른 아이템으로만 본다.
        [Test]
        public void 눕히면_안_들어가던_자리에_들어간다()
        {
            var grid = 새그리드();
            Assert.IsTrue(grid.TryPlace(new 아이템(5, 3), 0, 0), "준비: 위쪽 세 줄 채우기");

            Assert.IsFalse(grid.TryPlace(new 아이템(1, 3), 0, 3));   // 세로 3 은 안 들어간다
            Assert.IsTrue(grid.TryPlace(new 아이템(3, 1), 0, 3));    // 눕히면 들어간다
        }

        // ── 아래 넷은 놓인 아이템을 위치와 함께 돌려주는 조회를 검사한다 ──

        [Test]
        public void 빈_그리드에서는_아무것도_안_나온다()
        {
            var grid = 새그리드();

            Assert.IsEmpty(grid.GetPlacedItems());
        }

        [Test]
        public void 놓은_아이템의_왼쪽_위_좌표가_나온다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 1);
            Assert.IsTrue(grid.TryPlace(검, 3, 2), "준비: 검 배치");

            var placed = grid.GetPlacedItems();

            Assert.AreEqual(1, placed.Count);
            Assert.AreSame(검, placed[0].Item);
            Assert.AreEqual(3, placed[0].X);
            Assert.AreEqual(2, placed[0].Y);
        }

        /// 네 칸을 덮어도 한 번만 나와야 한다.
        [Test]
        public void 여러_칸을_덮어도_한_번만_나온다()
        {
            var grid = 새그리드();
            Assert.IsTrue(grid.TryPlace(new 아이템(2, 2), 1, 1), "준비: 2x2 배치");

            Assert.AreEqual(1, grid.GetPlacedItems().Count);
        }

        [Test]
        public void 뺀_아이템은_나오지_않는다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            var 부츠 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 0, 0), "준비: 검 배치");
            Assert.IsTrue(grid.TryPlace(부츠, 2, 2), "준비: 부츠 배치");

            grid.Remove(검);

            var placed = grid.GetPlacedItems();

            Assert.AreEqual(1, placed.Count);
            Assert.AreSame(부츠, placed[0].Item);
        }

        // ── 아래 여덟은 인접 세기를 검사한다 ──

        [Test]
        public void 혼자_있으면_인접이_없다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 2, 2), "준비: 검 배치");

            Assert.AreEqual(0, grid.CountAdjacent(검));
        }

        /// 2x2 는 자기 칸끼리 맞닿아 있다. 자기 자신을 세면 안 된다.
        [Test]
        public void 큰_아이템도_혼자면_인접이_없다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 2);
            Assert.IsTrue(grid.TryPlace(검, 1, 1), "준비: 2x2 배치");

            Assert.AreEqual(0, grid.CountAdjacent(검));
        }

        [Test]
        public void 상하좌우로_맞닿으면_센다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 2, 2), "준비: 검 배치");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 1, 2), "준비: 왼쪽");

            Assert.AreEqual(1, grid.CountAdjacent(검));
        }

        /// 대각선을 세면 한가운데에 놓기만 해도 이득이 커진다.
        [Test]
        public void 대각선은_세지_않는다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 2, 2), "준비: 검 배치");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 1, 1), "준비: 왼쪽 위 대각선");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 3, 3), "준비: 오른쪽 아래 대각선");

            Assert.AreEqual(0, grid.CountAdjacent(검));
        }

        [Test]
        public void 사방을_채우면_넷이다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 2, 2), "준비: 검 배치");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 1, 2), "준비: 왼쪽");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 3, 2), "준비: 오른쪽");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 2, 1), "준비: 위");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 2, 3), "준비: 아래");

            Assert.AreEqual(4, grid.CountAdjacent(검));
        }

        /// 2x2 와 1x2 가 두 칸에서 맞닿아도 인접한 아이템은 하나다.
        /// List 로 세면 여기서 2 가 나오고, 큰 아이템 옆이 무조건 유리해진다.
        [Test]
        public void 두_칸에서_맞닿아도_하나로_센다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 2);
            Assert.IsTrue(grid.TryPlace(검, 1, 1), "준비: 2x2 배치");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 2), 3, 1), "준비: 오른쪽에 1x2");

            Assert.AreEqual(1, grid.CountAdjacent(검));
        }

        [Test]
        public void 모서리에_있어도_격자_밖을_보지_않는다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 0, 0), "준비: 왼쪽 위 모서리");

            Assert.DoesNotThrow(() => grid.CountAdjacent(검));
            Assert.AreEqual(0, grid.CountAdjacent(검));
        }

        [Test]
        public void 옆_아이템을_빼면_인접이_줄어든다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            var 부츠 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 2, 2), "준비: 검 배치");
            Assert.IsTrue(grid.TryPlace(부츠, 1, 2), "준비: 부츠 배치");
            Assert.AreEqual(1, grid.CountAdjacent(검), "준비: 붙어 있는 상태");

            grid.Remove(부츠);

            Assert.AreEqual(0, grid.CountAdjacent(검));
        }

        // ── 아래 여섯은 같은 행 세기를 검사한다 ──

        [Test]
        public void 같은_행에_아무도_없으면_0이다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 2, 2), "준비: 검 배치");

            Assert.AreEqual(0, grid.CountInSameRow(검));
        }

        /// 인접과 달리 떨어져 있어도 센다.
        [Test]
        public void 같은_행의_다른_아이템을_센다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 0, 2), "준비: 검 배치");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 2, 2), "준비: 한 칸 건너");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 4, 2), "준비: 오른쪽 끝");

            Assert.AreEqual(2, grid.CountInSameRow(검));
        }

        /// 같은 열에 있는 것도, 멀리 있는 것도 세지 않는다.
        [Test]
        public void 다른_행에_있는_아이템은_세지_않는다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 2, 2), "준비: 검 배치");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 2, 1), "준비: 바로 위");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 0, 4), "준비: 먼 곳");

            Assert.AreEqual(0, grid.CountInSameRow(검));
        }

        /// 왼쪽 위 칸의 행만 보면 0 이 나온다.
        [Test]
        public void 두_행에_걸친_아이템은_두_행을_모두_본다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 2);
            Assert.IsTrue(grid.TryPlace(검, 0, 1), "준비: 2x2 를 1~2행에");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 4, 2), "준비: 아래쪽 행에만");

            Assert.AreEqual(1, grid.CountInSameRow(검));
        }

        /// 부츠를 1행과 2행에서 한 번씩 만난다. List 로 세면 2 가 나온다.
        [Test]
        public void 같은_아이템이_두_행에_걸쳐_있어도_한_번만_센다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 2);
            Assert.IsTrue(grid.TryPlace(검, 0, 1), "준비: 2x2 를 1~2행에");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 2), 3, 1), "준비: 1x2 를 1~2행에");

            Assert.AreEqual(1, grid.CountInSameRow(검));
        }

        /// 2x2 는 행을 훑는 동안 자기 칸을 네 번 만난다.
        [Test]
        public void 같은_행에서_자기_자신은_세지_않는다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 2);
            Assert.IsTrue(grid.TryPlace(검, 1, 1), "준비: 2x2 배치");

            Assert.AreEqual(0, grid.CountInSameRow(검));
        }

        // ── 아래 셋은 같은 열 세기를 검사한다. 같은 행과 대칭이다 ──

        [Test]
        public void 같은_열의_다른_아이템을_센다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 2, 0), "준비: 검 배치");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 2, 2), "준비: 한 칸 건너");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 2, 4), "준비: 아래쪽 끝");

            Assert.AreEqual(2, grid.CountInSameColumn(검));
        }

        /// 바로 옆에 붙어 있어도 열이 다르면 세지 않는다.
        [Test]
        public void 다른_열에_있는_아이템은_세지_않는다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 2, 2), "준비: 검 배치");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 1, 2), "준비: 바로 왼쪽");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 4, 0), "준비: 먼 곳");

            Assert.AreEqual(0, grid.CountInSameColumn(검));
        }

        /// 왼쪽 위 칸의 열만 보면 0 이 나온다.
        [Test]
        public void 두_열에_걸친_아이템은_두_열을_모두_본다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 2);
            Assert.IsTrue(grid.TryPlace(검, 1, 0), "준비: 2x2 를 1~2열에");
            Assert.IsTrue(grid.TryPlace(new 아이템(1, 1), 2, 4), "준비: 오른쪽 열에만");

            Assert.AreEqual(1, grid.CountInSameColumn(검));
        }

        // ── 아래 다섯은 모서리 세기를 검사한다 ──

        [Test]
        public void 가운데에_있으면_모서리가_0이다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(검, 2, 2), "준비: 검 배치");

            Assert.AreEqual(0, grid.CountCorners(검));
        }

        /// 한 그리드에 넷을 함께 놓는다. 남의 귀퉁이를 자기 것으로 세지 않는지도 함께 본다.
        [Test]
        public void 네_귀퉁이_각각에서_1이_나온다()
        {
            var grid = 새그리드();
            var 왼쪽위 = new 아이템(1, 1);
            var 오른쪽위 = new 아이템(1, 1);
            var 왼쪽아래 = new 아이템(1, 1);
            var 오른쪽아래 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(왼쪽위, 0, 0), "준비: 왼쪽 위");
            Assert.IsTrue(grid.TryPlace(오른쪽위, 4, 0), "준비: 오른쪽 위");
            Assert.IsTrue(grid.TryPlace(왼쪽아래, 0, 4), "준비: 왼쪽 아래");
            Assert.IsTrue(grid.TryPlace(오른쪽아래, 4, 4), "준비: 오른쪽 아래");

            Assert.AreEqual(1, grid.CountCorners(왼쪽위), "왼쪽 위");
            Assert.AreEqual(1, grid.CountCorners(오른쪽위), "오른쪽 위");
            Assert.AreEqual(1, grid.CountCorners(왼쪽아래), "왼쪽 아래");
            Assert.AreEqual(1, grid.CountCorners(오른쪽아래), "오른쪽 아래");
        }

        /// 왼쪽 위 좌표 (3,3) 은 귀퉁이가 아니다.
        /// 아이템의 위치를 보는 구현이면 0 이 나온다. 귀퉁이 칸을 봐야 잡힌다.
        [Test]
        public void 큰_아이템이_오른쪽_아래를_덮으면_1이다()
        {
            var grid = 새그리드();
            var 검 = new 아이템(2, 2);
            Assert.IsTrue(grid.TryPlace(검, 3, 3), "준비: 2x2 를 (3,3) 에");

            Assert.AreEqual(1, grid.CountCorners(검));
        }

        /// 가장자리에 붙어 있어도 귀퉁이 칸을 덮지 않으면 0 이다.
        [Test]
        public void 귀퉁이에_닿지_않으면_크기와_무관하게_0이다()
        {
            var grid = 새그리드();
            var 가운데 = new 아이템(2, 2);
            var 오른쪽_가장자리 = new 아이템(1, 2);
            var 아래_가장자리 = new 아이템(3, 1);
            Assert.IsTrue(grid.TryPlace(가운데, 1, 1), "준비: 2x2 를 가운데에");
            Assert.IsTrue(grid.TryPlace(오른쪽_가장자리, 4, 1), "준비: 1x2 를 오른쪽 가장자리에");
            Assert.IsTrue(grid.TryPlace(아래_가장자리, 1, 4), "준비: 3x1 을 아래 가장자리에");

            Assert.AreEqual(0, grid.CountCorners(가운데), "가운데");
            Assert.AreEqual(0, grid.CountCorners(오른쪽_가장자리), "오른쪽 가장자리");
            Assert.AreEqual(0, grid.CountCorners(아래_가장자리), "아래 가장자리");
        }

        /// 정사각형에서는 x 와 y 를 뒤바꿔 적어도 드러나지 않는다.
        /// 가로세로가 다르면 뒤바뀐 좌표가 격자 밖을 가리켜 예외가 난다.
        [Test]
        public void 가로세로가_다른_그리드에서도_귀퉁이를_찾는다()
        {
            var grid = new BagGrid(6, 4);
            var 왼쪽위 = new 아이템(1, 1);
            var 오른쪽위 = new 아이템(1, 1);
            var 왼쪽아래 = new 아이템(1, 1);
            var 오른쪽아래 = new 아이템(1, 1);
            Assert.IsTrue(grid.TryPlace(왼쪽위, 0, 0), "준비: 왼쪽 위");
            Assert.IsTrue(grid.TryPlace(오른쪽위, 5, 0), "준비: 오른쪽 위");
            Assert.IsTrue(grid.TryPlace(왼쪽아래, 0, 3), "준비: 왼쪽 아래");
            Assert.IsTrue(grid.TryPlace(오른쪽아래, 5, 3), "준비: 오른쪽 아래");

            Assert.AreEqual(1, grid.CountCorners(왼쪽위), "왼쪽 위");
            Assert.AreEqual(1, grid.CountCorners(오른쪽위), "오른쪽 위");
            Assert.AreEqual(1, grid.CountCorners(왼쪽아래), "왼쪽 아래");
            Assert.AreEqual(1, grid.CountCorners(오른쪽아래), "오른쪽 아래");
        }
    }
}
