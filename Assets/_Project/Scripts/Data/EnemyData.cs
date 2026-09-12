using UnityEngine;

namespace JM2D.Data
{
    /// 모든 적이 함께 갖는 수치. 대기와 추적에 쓰인다.
    /// 공격 수치는 적마다 달라 자식 데이터가 더한다. 한 클래스에 다 넣으면
    /// 적마다 절반이 늘 비어 있는 에셋이 된다.
    /// 행동은 여기 없다. 적 컨트롤러가 이 값을 읽어 움직인다.
    public abstract class EnemyData : ScriptableObject
    {
        [Header("체력")]
        [SerializeField] private int _maxHealth = 3;

        [Header("이동")]
        [SerializeField] private float _moveSpeed = 3f;

        [Header("감지")]
        [SerializeField] private float _detectRange = 6f;

        [Tooltip("이 밖으로 나가면 추적을 멈춘다. 감지 범위보다 넓어야 한다")]
        [SerializeField] private float _giveUpRange = 8f;

        public int MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float DetectRange => _detectRange;
        public float GiveUpRange => _giveUpRange;

        /// 들어가는 값과 나오는 값이 뒤바뀌면 오류 없이 상태가 깜빡인다.
        /// 고쳐 주지 않고 알리기만 한다.
        protected virtual void OnValidate()
        {
            if (_giveUpRange < _detectRange)
                Debug.LogWarning($"{name}: 추적 포기 범위({_giveUpRange})가 감지 범위({_detectRange})보다 작다", this);
        }
    }
}
