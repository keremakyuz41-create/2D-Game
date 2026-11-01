using UnityEngine;

namespace Cernobil.Companion
{
    public sealed class CompanionTaskHUD : MonoBehaviour
    {
        [SerializeField] private CompanionRobot robot;
        public void Configure(CompanionRobot value) => robot = value;

        public CompanionRobot Robot => robot;
    }
}
