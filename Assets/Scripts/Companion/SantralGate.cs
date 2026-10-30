using System.Collections;
using UnityEngine;

namespace Cernobil.Companion
{
    public sealed class SantralGate : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D barrier;
        [SerializeField] private Transform shutter;
        [SerializeField] private CompanionNavigationWorld navigation;
        [SerializeField] private SpriteRenderer statusLamp;
        [SerializeField] private float openingSeconds = 0.7f;
        [SerializeField] private float liftDistance = 3.2f;
        public bool IsOpen { get; private set; }
        public bool PassageOpen => IsOpen && !barrier.enabled;

        public void Configure(BoxCollider2D collider, Transform visibleShutter, CompanionNavigationWorld world)
        {
            barrier = collider;
            shutter = visibleShutter;
            navigation = world;
        }

        public void ConfigureAppearance(SpriteRenderer lamp, float seconds, float travel)
        {
            statusLamp = lamp;
            openingSeconds = Mathf.Max(0.1f, seconds);
            liftDistance = travel;
        }

        internal void UnlockFromPanel()
        {
            if (IsOpen) return;
            IsOpen = true;
            if (statusLamp != null) statusLamp.color = new Color(1, 0.65f, 0.15f);
            StartCoroutine(OpenShutter());
        }

        private IEnumerator OpenShutter()
        {
            Vector3 start = shutter.localPosition;
            float elapsed = 0;
            while (elapsed < openingSeconds)
            {
                elapsed += Time.deltaTime;
                shutter.localPosition = start + Vector3.up * (Mathf.SmoothStep(0, 1, elapsed / openingSeconds) * liftDistance);
                yield return null;
            }
            barrier.enabled = false;
            if (statusLamp != null) statusLamp.color = new Color(0.2f, 0.95f, 0.55f);
            navigation.RequestRebuild();
        }
    }
}
