using UnityEngine;

namespace Cernobil.World
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 minPosition = new Vector2(-5f, -3f);
        [SerializeField] private Vector2 maxPosition = new Vector2(5f, 3f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;
        private Vector3 velocity;

        public void SetTarget(Transform value) => target = value;

        public void ConfigureBounds(Vector2 minimum, Vector2 maximum)
        {
            minPosition = minimum;
            maxPosition = maximum;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 destination = new Vector3(
                Mathf.Clamp(target.position.x, minPosition.x, maxPosition.x),
                Mathf.Clamp(target.position.y, minPosition.y, maxPosition.y), -10f);
            transform.position = Vector3.SmoothDamp(transform.position, destination,
                ref velocity, smoothTime);
        }
    }
}
