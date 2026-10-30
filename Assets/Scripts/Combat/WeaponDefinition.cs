using UnityEngine;

namespace Cernobil.Combat
{
    [CreateAssetMenu(menuName = "Cernobil/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private WeaponKind kind;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite sprite;
        [SerializeField] private int magazineSize = 12;
        [SerializeField] private float shotInterval = 0.2f;
        [SerializeField] private float reloadSeconds = 1.2f;
        [SerializeField] private float range = 12;
        [SerializeField] private bool automatic;
        [SerializeField] private Vector2 muzzleOffset;
        public WeaponKind Kind => kind;
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public int MagazineSize => magazineSize;
        public float ShotInterval => shotInterval;
        public float ReloadSeconds => reloadSeconds;
        public float Range => range;
        public bool Automatic => automatic;
        public Vector2 MuzzleOffset => muzzleOffset;

        public void Configure(WeaponKind weaponKind, string label, Sprite art, int capacity,
            float interval, float reload, bool auto, Vector2 barrel)
        {
            kind = weaponKind;
            displayName = label;
            sprite = art;
            magazineSize = Mathf.Max(1, capacity);
            shotInterval = Mathf.Max(0.04f, interval);
            reloadSeconds = Mathf.Max(0.1f, reload);
            automatic = auto;
            muzzleOffset = barrel;
            range = weaponKind == WeaponKind.Rifle ? 14 : 12;
        }
    }
}
