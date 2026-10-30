using UnityEngine;
using Cernobil.Combat;

namespace Cernobil.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class ZombieChase : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0.1f)] private float moveSpeed = 1.1f;
        [SerializeField, Min(0.8f)] private float stopDistance = 1f;
        private Rigidbody2D body;
        private Health targetHealth;

        public void SetTarget(Transform value) => target = value;
        public void SetStopDistance(float value) => stopDistance = Mathf.Max(0.8f, value);

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (target != null) targetHealth = target.GetComponent<Health>();
        }

        private void FixedUpdate()
        {
            if (target == null || (targetHealth != null && !targetHealth.IsAlive))
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 delta = (Vector2)target.position - body.position;
            float distance = delta.magnitude;
            // Approach slowly and stop near the player, without overshooting.
            float speed = Mathf.Min(moveSpeed, Mathf.Max(0, distance - stopDistance) / Time.fixedDeltaTime);
            body.linearVelocity = distance > 0.001f ? delta / distance * speed : Vector2.zero;
        }

        private void OnDisable()
        {
            if (body != null) body.linearVelocity = Vector2.zero;
        }
    }
}
