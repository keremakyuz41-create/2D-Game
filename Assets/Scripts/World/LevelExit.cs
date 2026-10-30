using Cernobil.Player;
using UnityEngine;

namespace Cernobil.World
{
    public sealed class LevelExit : MonoBehaviour
    {
        [SerializeField] private StageProgression progression;
        public void Configure(StageProgression value) => progression = value;
        private void OnTriggerEnter2D(Collider2D other) => TryExit(other);
        private void OnTriggerStay2D(Collider2D other) => TryExit(other);
        private void TryExit(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerMovement>() != null) progression.GoToNext();
        }

    }
}
