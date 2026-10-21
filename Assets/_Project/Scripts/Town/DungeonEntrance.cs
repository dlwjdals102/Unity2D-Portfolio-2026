using UnityEngine;
using JM2D.Core;

namespace JM2D.Town
{
    /// 던전으로 들어가는 자리. 들어갈 때마다 새 시드로 방이 배치된다.
    public class DungeonEntrance : Interactable
    {
        [Tooltip("씬을 바꾸는 쪽. 마을의 SceneLoader 를 꽂는다")]
        [SerializeField] private SceneLoader _loader;

        protected override void Interact()
        {
            _loader.LoadGameplay();
        }
    }
}
