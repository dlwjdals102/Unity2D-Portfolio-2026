using System;

namespace JM2D.Logic.Rooms
{
    /// 칸 좌표와 월드 위치 사이의 변환. 이 변환은 여기 한 곳에만 둔다.
    /// 방 가운데가 칸 좌표에 방 크기를 곱한 자리이고, 방은 거기서 반씩 퍼져 있다.
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
    }
}
