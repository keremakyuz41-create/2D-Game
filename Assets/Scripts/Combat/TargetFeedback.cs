using UnityEngine;

namespace Cernobil.Combat
{
    [RequireComponent(typeof(Health), typeof(SpriteRenderer))]
    public sealed class TargetFeedback : MonoBehaviour
    {
        private Health health;
        [SerializeField] private SpriteRenderer visual;
        private Color healthyColor;

        public void ConfigureRenderer(SpriteRenderer value) => visual = value;

        private void Awake()
        {
            health = GetComponent<Health>();
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            healthyColor = visual.color;
        }

        private void OnEnable() => health.Changed += ShowDamage;
        private void OnDisable() => health.Changed -= ShowDamage;

        private void ShowDamage(int current, int maximum)
        {
            visual.color = Color.Lerp(Color.Lerp(Color.black, healthyColor, 0.35f),
                healthyColor, current / (float)maximum);
        }
    }
}
