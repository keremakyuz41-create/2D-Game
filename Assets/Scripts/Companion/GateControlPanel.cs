using UnityEngine;

namespace Cernobil.Companion
{
    public sealed class GateControlPanel : MonoBehaviour
    {
        [SerializeField] private SantralGate gate;
        [SerializeField] private CompanionRobot authorizedRobot;
        [SerializeField] private SpriteRenderer indicatorLight;

        public void Configure(SantralGate targetGate, CompanionRobot robot, SpriteRenderer indicator)
        {
            gate = targetGate;
            authorizedRobot = robot;
            indicatorLight = indicator;
        }

        public bool TryActivate(CompanionRobot actor)
        {
            if (actor != authorizedRobot || Vector2.Distance(actor.transform.position, transform.position) > 0.5f)
                return false;
            if (indicatorLight != null) indicatorLight.color = new Color(0.3f, 1, 0.5f);
            gate.UnlockFromPanel();
            return true;
        }
    }
}
