using System;

namespace JM2D.Logic.Rooms
{
    /// 칸 좌표와 월드 위치 사이의 변환. 이 변환은 여기 한 곳에만 둔다.
    /// 방 가운데가 칸 좌표에 방 크기를 곱한 자리이고, 방은 거기서 반씩 퍼져 있다.
    /// 이 모양을 아는 곳이 여기뿐이라 방 안쪽 판정도 여기 둔다.
    public static class RoomSpace
    {
        /// 칸의 가운데를 월드 위치로 바꾼다.
        public static void ToWorld(RoomCell cell, int roomWidth, int roomHeight, out float x, out float y)
        {
            x = cell.X * roomWidth;
            y = cell.Y * roomHeight;
        }

        /// 월드 위치가 속한 칸을 찾는다.
        /// 두 방의 경계는 양의 방향 방에 속한다. 은행가 반올림을 쓰지 않는다.
        public static RoomCell ToCell(float x, float y, int roomWidth, int roomHeight)
        {
            // 0.5 를 더해 내리면 가장 가까운 칸이 되고, 경계(정확히 .5)는 위쪽으로 간다.
            int cellX = (int)Math.Floor(x / roomWidth + 0.5f);
            int cellY = (int)Math.Floor(y / roomHeight + 0.5f);
            return new RoomCell(cellX, cellY);
        }

        /// 위치가 이 칸의 방 안으로 여유 거리 이상 들어와 있는지 본다.
        /// 여유 거리는 방 바깥 경계에서 잰다. 딱 맞으면 안쪽이다.
        public static bool IsInside(RoomCell cell, float x, float y, int roomWidth, int roomHeight, float margin)
        {
            ToWorld(cell, roomWidth, roomHeight, out float centerX, out float centerY);

            float dx = Math.Abs(x - centerX);
            float dy = Math.Abs(y - centerY);

            // 방은 가운데에서 폭의 반만큼 퍼져 있다. 정수 나눗셈이 되지 않게 0.5f 를 곱한다.
            float limitX = roomWidth * 0.5f - margin;
            float limitY = roomHeight * 0.5f - margin;

            return dx <= limitX && dy <= limitY;
        }
    }
}
