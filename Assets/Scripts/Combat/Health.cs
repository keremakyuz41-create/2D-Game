using System;
using UnityEngine;

namespace Cernobil.Combat
{
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maximumHealth = 3;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField, Min(0)] private float damageCooldown;
        private float nextDamageTime;
        public int Current { get; private set; }
        public int Maximum => maximumHealth;
        public bool IsAlive => Current > 0;
        public void RestoreCurrent(int value)
        {
            Current = Mathf.Clamp(value, 1, maximumHealth);
            Changed?.Invoke(Current, maximumHealth);
        }
        public event Action<int, int> Changed;
        public event Action Died;

        public void Configure(int maximum, bool destroyWhenDead, float cooldown = 0)
        {
            maximumHealth = Mathf.Max(1, maximum);
            destroyOnDeath = destroyWhenDead;
            damageCooldown = Mathf.Max(0, cooldown);
        }

        private void Awake()
        {
            Current = maximumHealth;
            nextDamageTime = float.NegativeInfinity;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || Current <= 0 || Time.time < nextDamageTime) return;
            nextDamageTime = Time.time + damageCooldown;
            Current = Mathf.Max(0, Current - amount);
            Changed?.Invoke(Current, maximumHealth);
            if (Current != 0) return;
            Died?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
        }
    }
}
