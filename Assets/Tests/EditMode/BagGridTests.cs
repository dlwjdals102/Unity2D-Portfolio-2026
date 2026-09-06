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
    }
}
