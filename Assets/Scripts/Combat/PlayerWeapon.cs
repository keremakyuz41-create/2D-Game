using UnityEngine;

namespace Cernobil.Combat
{
    public sealed class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private Transform gunPivot;
        [SerializeField, Min(1)] private int magazineSize = 12;
        [SerializeField, Min(0.01f)] private float shotInterval = 0.2f;
        [SerializeField, Min(0.1f)] private float reloadSeconds = 1.2f;
        [SerializeField, Min(1f)] private float range = 12f;
        [SerializeField] private bool handheldVisual;
        [SerializeField] private SpriteRenderer gunVisual;
        [SerializeField] private SpriteRenderer muzzleFlash;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Vector2 handOffset = new Vector2(0.72f, 0.08f);
        private float flashEndTime;
        private Camera gameCamera;
        private LineRenderer tracer;
        private Material tracerMaterial;
        private Vector2 aim = Vector2.right;
        private float nextShotTime;
        private float reloadEndTime;
        private float tracerEndTime;
        [SerializeField] private WeaponDefinition pistol;
        [SerializeField] private WeaponDefinition rifle;
        private WeaponDefinition equipped;
        private bool requireTriggerRelease;

        public int Ammo { get; private set; }
        public int MagazineSize => magazineSize;
        public bool IsReloading { get; private set; }
        public Sprite DisplaySprite => gunVisual != null ? gunVisual.sprite : null;
        public float ReloadProgress => IsReloading ? Mathf.Clamp01(1 - (reloadEndTime - Time.time) / reloadSeconds) : 0;
        public WeaponKind CurrentKind => equipped != null ? equipped.Kind : WeaponKind.Pistol;
        public string DisplayName => equipped != null ? equipped.DisplayName : "Tabanca";

        public void ConfigureLoadout(WeaponDefinition pistolDefinition, WeaponDefinition rifleDefinition)
        {
            pistol = pistolDefinition;
            rifle = rifleDefinition;
        }

        public bool EquipRifle()
        {
            if (rifle == null || CurrentKind == WeaponKind.Rifle) return false;
            ApplyWeapon(rifle);
            requireTriggerRelease = true;
            return true;
        }

        public void RestoreLoadout(WeaponKind kind, int ammo)
        {
            ApplyWeapon(kind == WeaponKind.Rifle && rifle != null ? rifle : pistol);
            Ammo = Mathf.Clamp(ammo, 0, magazineSize);
            requireTriggerRelease = true;
        }

        private void ApplyWeapon(WeaponDefinition definition)
        {
            if (definition == null) return;
            equipped = definition;
            magazineSize = definition.MagazineSize;
            shotInterval = definition.ShotInterval;
            reloadSeconds = definition.ReloadSeconds;
            range = definition.Range;
            if (gunVisual != null && definition.Sprite != null) gunVisual.sprite = definition.Sprite;
            if (muzzle != null) muzzle.localPosition = definition.MuzzleOffset;
            IsReloading = false;
            Ammo = magazineSize;
            nextShotTime = Time.time + 0.15f;
        }

        public void SetGunPivot(Transform value) => gunPivot = value;

        public void ConfigureHandheld(SpriteRenderer gun, Transform barrelTip, SpriteRenderer flash)
        {
            handheldVisual = true;
            gunVisual = gun;
            muzzle = barrelTip;
            muzzleFlash = flash;
        }

        private void Awake()
        {
            gameCamera = Camera.main;
            Ammo = magazineSize;
            ApplyWeapon(pistol);
            tracer = gameObject.AddComponent<LineRenderer>();
            tracerMaterial = new Material(Shader.Find("Sprites/Default"));
            tracer.sharedMaterial = tracerMaterial;
            tracer.useWorldSpace = true;
            tracer.positionCount = 2;
            tracer.startWidth = 0.055f;
            tracer.endWidth = 0.025f;
            tracer.startColor = new Color(1f, 0.9f, 0.3f);
            tracer.endColor = new Color(1f, 0.6f, 0.15f);
            tracer.sortingOrder = 25;
            tracer.enabled = false;
        }

        private void Update()
        {
            if (tracer.enabled && Time.time >= tracerEndTime) tracer.enabled = false;
            if (muzzleFlash != null && Time.time >= flashEndTime) muzzleFlash.enabled = false;
            if (!Input.GetMouseButton(0)) requireTriggerRelease = false;
            if (IsReloading && Time.time >= reloadEndTime)
            {
                Ammo = magazineSize;
                IsReloading = false;
            }
            if (Cernobil.Dialogue.NpcDialogue.BlocksPlayerInput)
            {
                requireTriggerRelease = true;
                return;
            }
            if (gameCamera == null) return;
            Vector3 mouse = Input.mousePosition;
            bool pointerInGame = mouse.x >= 0 && mouse.y >= 0 &&
                mouse.x < Screen.width && mouse.y < Screen.height;
            if (!pointerInGame) return;

            Vector2 mouseWorld = gameCamera.ScreenToWorldPoint(mouse);
            if (handheldVisual && gunPivot != null)
            {
                float facing = mouseWorld.x >= transform.position.x ? 1 : -1;
                gunPivot.localPosition = new Vector3(handOffset.x * facing, handOffset.y, 0);
            }
            Vector2 origin = handheldVisual && gunPivot != null ? gunPivot.position : transform.position;
            Vector2 delta = mouseWorld - origin;
            if (delta.sqrMagnitude > 0.001f) aim = delta.normalized;
            if (handheldVisual && muzzle != null)
            {
                float facing = mouseWorld.x >= transform.position.x ? 1 : -1;
                float barrelHeight = Mathf.Abs(muzzle.localPosition.y) * facing;
                muzzle.localPosition = new Vector3(muzzle.localPosition.x, barrelHeight, 0);
                if (delta.magnitude > Mathf.Abs(barrelHeight) + 0.01f)
                {
                    float angle = Mathf.Atan2(delta.y, delta.x) - Mathf.Asin(barrelHeight / delta.magnitude);
                    aim = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                }
            }
            if (gunPivot != null)
                gunPivot.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg);
            if (gunVisual != null) gunVisual.flipY = aim.x < 0;

            if (Input.GetKeyDown(KeyCode.R) && !IsReloading && Ammo < magazineSize)
            {
                IsReloading = true;
                reloadEndTime = Time.time + reloadSeconds;
            }
            bool trigger = equipped != null && equipped.Automatic ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
            if (trigger && !requireTriggerRelease && !IsReloading && Ammo > 0 && Time.time >= nextShotTime)
                Fire();
        }

        private void Fire()
        {
            Ammo--;
            nextShotTime = Time.time + shotInterval;
            Vector2 origin = transform.position;
            Vector2 shotOrigin = handheldVisual && muzzle != null ? muzzle.position : origin;
            Vector2 end = shotOrigin + aim * range;
            // Check the short body-to-hand segment before firing from a hand beside a wall.
            Vector2 toHand = shotOrigin - origin;
            bool blockedAtHand = FirstSolid(origin, toHand.normalized, toHand.magnitude, out var handHit);
            if (blockedAtHand)
            {
                end = handHit.point;
                handHit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(1);
            }
            // Cast from the body center so aiming next to a wall cannot shoot through it.
            // Ignore the shooter's own colliders, then stop at the nearest solid collider.
            if (!blockedAtHand && FirstSolid(shotOrigin, aim, range, out var hit))
            {
                end = hit.point;
                hit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(1);
            }
            Vector2 tracerStart = origin;
            if (handheldVisual && muzzle != null && !blockedAtHand)
                tracerStart = shotOrigin + aim * Mathf.Min(Vector2.Distance(shotOrigin, muzzle.position),
                    Vector2.Distance(shotOrigin, end));
            tracer.SetPosition(0, tracerStart);
            tracer.SetPosition(1, end);
            tracer.enabled = true;
            tracerEndTime = Time.time + 0.07f;
            if (muzzleFlash != null && !blockedAtHand)
            {
                muzzleFlash.enabled = true;
                flashEndTime = Time.time + 0.06f;
            }
        }

        private bool FirstSolid(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D nearest)
        {
            if (distance > 0.001f)
            {
                foreach (var hit in Physics2D.RaycastAll(origin, direction, distance))
                {
                    if (hit.collider.isTrigger || hit.transform.IsChildOf(transform)) continue;
                    nearest = hit;
                    return true;
                }
            }
            nearest = default;
            return false;
        }

        private void OnDisable()
        {
            if (tracer != null) tracer.enabled = false;
            if (muzzleFlash != null) muzzleFlash.enabled = false;
        }

        private void OnDestroy()
        {
            if (tracerMaterial != null) Destroy(tracerMaterial);
        }
    }
}
