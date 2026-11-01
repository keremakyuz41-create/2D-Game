using Cernobil.Combat;

namespace Cernobil.World
{
    public readonly struct PlayerSnapshot
    {
        public readonly int Health;
        public readonly WeaponKind Weapon;
        public readonly int Ammo;
        public PlayerSnapshot(int health, WeaponKind weapon, int ammo)
        {
            Health = health;
            Weapon = weapon;
            Ammo = ammo;
        }
    }

    public static class LevelTransfer
    {
        private static string destination;
        private static PlayerSnapshot pending;

        public static void Store(string nextScene, PlayerSnapshot state)
        {
            destination = nextScene;
            pending = state;
        }

        public static bool TryTake(string scene, out PlayerSnapshot state)
        {
            bool matches = !string.IsNullOrEmpty(destination) && destination == scene;
            state = matches ? pending : default;
            Clear();
            return matches;
        }

        public static void Clear()
        {
            destination = null;
            pending = default;
        }
    }
}
