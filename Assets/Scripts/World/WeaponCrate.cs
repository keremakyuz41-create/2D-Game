using Cernobil.Combat;
using UnityEngine;

namespace Cernobil.World
{
    [RequireComponent(typeof(Health))]
    public sealed class WeaponCrate : MonoBehaviour
    {
        [SerializeField] private GameObject contents;
        private Health health;
        public void Configure(GameObject pickup) => contents = pickup;
        private void Awake() => health = GetComponent<Health>();
        private void OnEnable() => health.Died += Open;
        private void OnDisable() => health.Died -= Open;
        private void Open()
        {
            if (contents != null) contents.SetActive(true);
        }
    }
}
