using UnityEngine;

namespace Cernobil.Player
{
    public sealed class KeyboardMovementInput : MonoBehaviour, IMovementInput
    {
        public Vector2 ReadMovement()
        {
            if (Cernobil.Dialogue.NpcDialogue.BlocksPlayerInput) return Vector2.zero;
            return Vector2.ClampMagnitude(new Vector2(
                Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
        }
    }
}
