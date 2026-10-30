using Cernobil.Combat;
using UnityEngine;

namespace Cernobil.Enemies
{
    public sealed class ZombieAttack : MonoBehaviour
    {
        [SerializeField] private Health target;
        [SerializeField, Min(1)] private int damage = 10;
        [SerializeField, Min(0.1f)] private float attackInterval = 1.3f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.2f;
        private float nextAttackTime;

        public void SetTarget(Health value) => target = value;
        public void SetRange(float value) => attackRange = Mathf.Max(0.1f, value);
        private void Awake() => nextAttackTime = Time.time + attackInterval;

        private void FixedUpdate()
        {
            if (target == null || !target.IsAlive || Time.time < nextAttackTime) return;
            Vector2 offset = target.transform.position - transform.position;
            float distance = offset.magnitude;
            if (distance > attackRange) return;
            nextAttackTime = Time.time + attackInterval;

            // A nearby wall blocks a melee attack as well as a shot.
            foreach (var hit in Physics2D.RaycastAll(transform.position, offset.normalized, distance))
            {
                if (hit.collider.isTrigger || hit.transform.IsChildOf(transform)) continue;
                if (hit.collider.GetComponentInParent<Health>() == target) target.TakeDamage(damage);
                return;
            }
        }
    }
}
