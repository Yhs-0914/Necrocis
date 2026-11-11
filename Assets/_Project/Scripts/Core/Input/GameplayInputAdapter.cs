using UnityEngine;
using UnityEngine.InputSystem;

namespace Necrocis
{
    public sealed class GameplayInputAdapter
    {
        private readonly InputManager owner;
        private bool waitForNeutral;
        public GameplayInputAdapter(InputManager owner) => this.owner = owner;
        internal void ClearHeldIntent() => waitForNeutral = true;

        public bool CanReadGameplay
        {
            get
            {
                if (owner == null || !owner.isActiveAndEnabled || owner.IsRetiring || owner.Contexts.BlocksGameplay || Time.timeScale <= Mathf.Epsilon) return false;
                if (waitForNeutral)
                {
                    if (owner.MoveAction.ReadValue<Vector2>().sqrMagnitude > .0001f
                        || owner.MeleeAttackAction.IsPressed() || owner.RangedAttackAction.IsPressed()
                        || owner.Skill1Action.IsPressed() || owner.Skill2Action.IsPressed()
                        || owner.DashAction.IsPressed() || owner.TraversalHoldAction.IsPressed()) return false;
                    waitForNeutral = false;
                }
                return true;
            }
        }
        public Vector2 Move => CanReadGameplay ? owner.MoveAction.ReadValue<Vector2>() : Vector2.zero;
        public bool TraversalHeld => CanReadGameplay && owner.TraversalHoldAction.IsPressed();
        public float ScrollY => CanReadGameplay && Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
    }
}
