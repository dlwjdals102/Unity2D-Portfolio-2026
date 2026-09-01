using System.Collections.Generic;
using UnityEngine;

namespace JM2D.Core
{
    /// <summary>
    /// 컴포넌트를 재사용한다. 부족하면 새로 만들고, 돌아온 것은 비활성화해 쌓아둔다.
    /// MonoBehaviour 가 아니다. 쓰는 쪽이 필드로 소유한다.
    /// </summary>
    public class Pool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _idle = new Stack<T>();

        public Pool(T prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        /// 하나 꺼낸다. 쌓아둔 것이 없으면 새로 만든다.
        public T Get()
        {
            T instance = _idle.Count > 0
                ? _idle.Pop()
                : Object.Instantiate(_prefab, _parent);

            instance.gameObject.SetActive(true);
            return instance;
        }

        /// 다 쓴 것을 돌려준다.
        public void Release(T instance)
        {
            if (!instance.gameObject.activeSelf) return;

            instance.gameObject.SetActive(false);
            _idle.Push(instance);
        }
    }
}
