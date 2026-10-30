using UnityEngine;

namespace Cernobil.Companion
{
    public sealed class RobotObstacleSensor : MonoBehaviour
    {
        public float NearestDistance { get; private set; } = 0.8f;
        public float NearestAngle { get; private set; }
        public bool Blocked => NearestDistance < 0.14f;

        public void Observe(Vector2 direction)
        {
            NearestDistance = 0.8f;
            NearestAngle = 0;
            if (direction.sqrMagnitude < 0.001f) return;
            direction.Normalize();
            foreach (float angle in new[] { -30f, 0, 30f })
            {
                Vector2 ray = Quaternion.Euler(0, 0, angle) * direction;
                foreach (var hit in Physics2D.RaycastAll(transform.position, ray, 0.8f))
                {
                    if (hit.collider.isTrigger || hit.collider.GetComponent<NavigationObstacle2D>() == null) continue;
                    if (hit.distance < NearestDistance)
                    {
                        NearestDistance = hit.distance;
                        NearestAngle = angle;
                    }
                    break;
                }
                Debug.DrawRay(transform.position, ray * 0.8f, Color.cyan);
            }
        }
    }
}
