using Cernobil.Combat;
using UnityEngine;

namespace Cernobil.World
{
    public sealed class WeaponPickup : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon playerWeapon;
        [SerializeField] private Health playerHealth;
        [SerializeField] private float pickupRange = 2.5f;
        public bool CanPickup => playerWeapon != null && playerHealth != null && playerHealth.IsAlive &&
            Vector2.Distance(playerWeapon.transform.position, transform.position) <= pickupRange;

        public void Configure(PlayerWeapon weapon, Health health)
        {
            playerWeapon = weapon;
            playerHealth = health;
        }

        private void Update()
        {
            if (!Cernobil.Dialogue.NpcDialogue.BlocksPlayerInput && CanPickup && Input.GetKeyDown(KeyCode.E) && playerWeapon.EquipRifle()) Destroy(gameObject);
        }

    }
}
