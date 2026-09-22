using JM2D.Logic.Rooms;
using UnityEngine;

namespace JM2D.Rooms
{
    /// 거리표를 씬 뷰에 그린다. 가까운 칸일수록 진하다.
    /// 거리표는 눈에 보이지 않는 자료라, 적을 고치기 전에 표가 맞는지 본다.
    ///
    /// 진하기 기준은 그 방에서 실제로 나온 가장 먼 거리다. 방마다 대비가 또렷하게 나온다.
    /// 디버그 전용이고 게임 화면에는 나오지 않는다.
    public class FlowFieldGizmo : MonoBehaviour
    {
        [SerializeField] private RoomFlowField _flowField;

        [Tooltip("끄면 아무것도 그리지 않는다")]
        [SerializeField] private bool _draw = true;

        [Tooltip("켜면 칸마다 거리를 숫자로 적는다. 확인할 때만 켠다")]
        [SerializeField] private bool _showNumbers;

        [Tooltip("켜면 격자가 '막혔다' 고 아는 칸을 빨간 테두리로 그린다. 칠한 타일과 맞는지 볼 때 쓴다")]
        [SerializeField] private bool _showBlocked = true;

        [SerializeField] private Color _color = new Color(1f, 0.85f, 0.2f);

        [Range(0f, 1f)]
        [SerializeField] private float _alpha = 0.4f;

        private void OnDrawGizmos()
        {
            if (!_draw || _flowField == null) return;

            RoomObstacles room = _flowField.Room;
            FlowField field = _flowField.Field;

            if (room == null || field == null) return;

            float farthest = Mathf.Max(1, Farthest(field));

            if (_showBlocked) DrawBlocked(room);

            for (int y = 0; y < field.Height; y++)
                for (int x = 0; x < field.Width; x++)
                {
                    int distance = field.GetDistance(x, y);

                    // 벽, 장애물, 닿을 수 없는 칸은 그리지 않는다.
                    if (distance == FlowField.Unreachable) continue;

                    Vector3 center = room.CellCenterWorld(new RoomCell(x, y));

                    Color color = _color;
                    color.a = _alpha * (1f - distance / farthest);

                    Gizmos.color = color;
                    Gizmos.DrawCube(center, Vector3.one * 0.9f);

#if UNITY_EDITOR
                    if (_showNumbers)
                        UnityEditor.Handles.Label(center, distance.ToString());
#endif
                }
        }

        /// 격자가 막혔다고 아는 칸. 화면에 칠한 타일과 겹쳐 보면 어긋남이 드러난다.
        private static void DrawBlocked(RoomObstacles room)
        {
            RoomGrid grid = room.Grid;

            if (grid == null) return;

            Gizmos.color = Color.red;

            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                {
                    if (grid.IsPassable(x, y)) continue;

                    Gizmos.DrawWireCube(room.CellCenterWorld(new RoomCell(x, y)), Vector3.one * 0.95f);
                }
        }

        /// 이 방에서 가장 먼 칸까지의 거리. 진하기 기준이다.
        private static int Farthest(FlowField field)
        {
            int farthest = 0;

            for (int y = 0; y < field.Height; y++)
                for (int x = 0; x < field.Width; x++)
                    farthest = Mathf.Max(farthest, field.GetDistance(x, y));

            return farthest;
        }
    }
}
