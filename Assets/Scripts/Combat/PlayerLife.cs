using Cernobil.Player;
using UnityEngine;

namespace Cernobil.Combat
{
    [RequireComponent(typeof(Health))]
    public sealed class PlayerLife : MonoBehaviour
    {
        private Health health;

        private void Awake() => health = GetComponent<Health>();
        private void OnEnable() => health.Died += StopPlayer;
        private void OnDisable() => health.Died -= StopPlayer;

        private void StopPlayer()
        {
            GetComponent<PlayerMovement>().enabled = false;
            GetComponent<PlayerWeapon>().enabled = false;
            var body = GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0;
        }
    }
}
