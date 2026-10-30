using UnityEngine;

namespace Cernobil.World
{
    public sealed class ShelterExit : MonoBehaviour
    {
        [SerializeField] private ShelterGate gate;
        public void Configure(ShelterGate value) => gate = value;
        private void OnTriggerEnter2D(Collider2D other) => gate.TryFinish(other);
        private void OnTriggerStay2D(Collider2D other) => gate.TryFinish(other);
    }
}
