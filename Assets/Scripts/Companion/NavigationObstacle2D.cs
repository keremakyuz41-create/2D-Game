using UnityEngine;

namespace Cernobil.Companion
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class NavigationObstacle2D : MonoBehaviour
    {
        public BoxCollider2D Collider => GetComponent<BoxCollider2D>();
    }
}
