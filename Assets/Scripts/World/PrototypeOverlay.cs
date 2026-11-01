using Cernobil.Combat;
using Cernobil.Companion;
using Cernobil.Enemies;
using Cernobil.UI;
using UnityEngine;

namespace Cernobil.World
{
    public sealed class PrototypeOverlay : MonoBehaviour
    {
        // Preserve existing scene references and setup methods.
        [SerializeField] private PlayerWeapon weapon;
        [SerializeField] private bool zombieMode;
        [SerializeField] private bool survivalMode;
        [SerializeField] private bool streetMode;
        [SerializeField] private bool runningMode;
        [SerializeField] private StageProgression stage;
        private SurvivalSession session;
        private CompanionRobot robot;
        private WeaponPickup pickup;
        private LevelExit exit;
        private float nextRefresh;
        private float displayedHealth = 1;
        private int zombieCount;

        public void ConfigureCombat(PlayerWeapon value) => weapon = value;
        public void ConfigureZombies() => zombieMode = true;
        public void ConfigureSurvival() => survivalMode = true;
        public void ConfigureStreet() => streetMode = true;
        public void ConfigureRunning() => runningMode = true;
        public void ConfigureStage(StageProgression value) => stage = value;

        private void Start()
        {
            if (weapon == null) weapon = FindFirstObjectByType<PlayerWeapon>();
            if (stage == null) stage = FindFirstObjectByType<StageProgression>();
            session = FindFirstObjectByType<SurvivalSession>();
            robot = FindFirstObjectByType<CompanionRobot>();
            exit = FindFirstObjectByType<LevelExit>();
            if (session != null && session.PlayerHealth != null)
                displayedHealth = session.PlayerHealth.Current / (float)session.PlayerHealth.Maximum;
        }

        private void Update()
        {
            if (session != null && session.PlayerHealth != null)
                displayedHealth = Mathf.MoveTowards(displayedHealth,
                    session.PlayerHealth.Current / (float)session.PlayerHealth.Maximum, Time.unscaledDeltaTime * 3);
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;
            pickup = FindFirstObjectByType<WeaponPickup>();
            zombieCount = FindObjectsByType<ZombieChase>(FindObjectsSortMode.None).Length;
        }

        private void OnGUI()
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            try
            {
                float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
                if (scale <= 0) return;
                GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
                GUI.color = Color.white;
                float width = Screen.width / scale, height = Screen.height / scale;
                if (session != null && session.DamageFlashStrength > 0)
                    HudTheme.Fill(new Rect(0, 0, width, height), new Color(0.9f, 0.08f, 0.06f, session.DamageFlashStrength * 0.12f));
                DrawMission();
                DrawThreats(width);
                DrawHealth(height);
                DrawWeapon(width, height);
                DrawControls(width, height);
                bool isOver = session != null && session.IsGameOver;
                if (!isOver)
                {
                    DrawCompanion(width, height);
                    DrawInteraction(width, height);
                    DrawExit(scale, width, height);
                }
                if (stage != null && (stage.IsLoading || !string.IsNullOrEmpty(stage.Error))) DrawLoading(width, height);
                if (isOver) DrawGameOver(width, height);
            }
            finally { GUI.matrix = previousMatrix; GUI.color = previousColor; }
        }

        private void DrawMission()
        {
            HudTheme.Rounded(new Rect(28, 28, 5, 32), HudTheme.Amber, 2);
            HudTheme.Label(new Rect(45, 25, 350, 28), "SON TAHLİYE", 23, HudTheme.Text, true);
            HudTheme.Label(new Rect(46, 53, 380, 22), "ÇERNOBİL · KARANTİNA BÖLGESİ", 12, HudTheme.Muted);
            HudTheme.Card(new Rect(28, 87, 430, 144));
            HudTheme.Fill(new Rect(28, 105, 3, 108), HudTheme.Amber);
            string heading = stage != null
                ? $"BÖLÜM {stage.StageNumber:00}  /  " + (stage.StageNumber == 1 ? "TERK EDİLMİŞ SOKAK" : stage.StageNumber == 3 ? "SON TAHLİYE" : "İKMAL DEPOSU")
                : streetMode ? "TERK EDİLMİŞ SOKAK" : "DENEME ALANI";
            string goal = stage != null ? stage.Objective : zombieMode || survivalMode ? "Mesafeni koru. Zombileri etkisiz hale getir."
                : weapon != null ? "Nişan al ve hedeflerde atışını dene." : "Hareket et ve çevreyi keşfet.";
            HudTheme.Label(new Rect(49, 100, 388, 24), heading, 13, HudTheme.Amber, true);
            HudTheme.Label(new Rect(49, 133, 384, 82), goal, 18, HudTheme.Text, false, TextAnchor.UpperLeft, true);
        }

        private void DrawThreats(float width)
        {
            if (weapon == null) return;
            var card = new Rect(width - 196, 28, 168, 54);
            HudTheme.Card(card);
            HudTheme.Rounded(new Rect(card.x + 16, card.y + 24, 6, 6), zombieCount > 0 ? HudTheme.Amber : HudTheme.Mint, 3);
            HudTheme.Label(new Rect(card.x + 31, card.y + 10, 88, 34), "ZOMBİ", 12, HudTheme.Muted, true);
            HudTheme.Label(new Rect(card.x + 121, card.y + 5, 33, 44), zombieCount.ToString("00"), 26, HudTheme.Text, true, TextAnchor.MiddleRight);
        }

        private void DrawHealth(float height)
        {
            if (session == null || session.PlayerHealth == null) return;
            Health health = session.PlayerHealth;
            Color accent = health.Current <= health.Maximum * 0.25f ? HudTheme.Danger
                : health.Current <= health.Maximum * 0.5f ? HudTheme.Amber : HudTheme.Mint;
            var card = new Rect(28, height - 134, 280, 94);
            HudTheme.Card(card);
            HudTheme.Rounded(new Rect(card.x + 16, card.y + 16, 34, 34), new Color(accent.r, accent.g, accent.b, 0.12f), 8);
            HudTheme.Fill(new Rect(card.x + 31, card.y + 24, 4, 18), accent);
            HudTheme.Fill(new Rect(card.x + 24, card.y + 31, 18, 4), accent);
            HudTheme.Label(new Rect(card.x + 62, card.y + 8, 98, 22), "CAN DURUMU", 11, HudTheme.Muted, true);
            HudTheme.Label(new Rect(card.x + 62, card.y + 36, 88, 27), health.Current.ToString(), 26, accent, true);
            HudTheme.Label(new Rect(card.x + 128, card.y + 35, 60, 23), "/ " + health.Maximum, 14, HudTheme.Muted);
            if (health.Current > 0 && health.Current <= health.Maximum * 0.25f)
                HudTheme.Label(new Rect(card.x + 180, card.y + 25, 84, 25), "KRİTİK", 11, HudTheme.Danger, true, TextAnchor.MiddleRight);
            HudTheme.Rounded(new Rect(card.x + 17, card.y + 72, 246, 7), new Color(1, 1, 1, 0.08f), 3);
            if (displayedHealth > 0) HudTheme.Rounded(new Rect(card.x + 17, card.y + 72, 246 * displayedHealth, 7), accent, 3);
        }

        private void DrawWeapon(float width, float height)
        {
            if (weapon == null) return;
            var card = new Rect(width - 348, height - 178, 320, 138);
            HudTheme.Card(card);
            Color accent = weapon.Ammo == 0 ? HudTheme.Danger : weapon.Ammo <= weapon.MagazineSize / 4 ? HudTheme.Amber : HudTheme.Text;
            HudTheme.Label(new Rect(card.x + 20, card.y + 8, 166, 22), "AKTİF SİLAH", 11, HudTheme.Muted, true);
            HudTheme.Label(new Rect(card.x + 20, card.y + 35, 185, 25), weapon.DisplayName.ToUpperInvariant(), 16, HudTheme.Text, true);
            HudTheme.DrawSprite(new Rect(card.x + 211, card.y + 13, 90, 43), weapon.DisplaySprite);
            HudTheme.Label(new Rect(card.x + 18, card.y + 58, 80, 56), weapon.Ammo.ToString("00"), 44, accent, true);
            HudTheme.Label(new Rect(card.x + 93, card.y + 78, 64, 29), "/ " + weapon.MagazineSize.ToString("00"), 18, HudTheme.Muted);
            string state = weapon.IsReloading ? "DOLDURULUYOR" : weapon.Ammo == 0 ? "ŞARJÖR BOŞ" : "ŞARJÖR";
            HudTheme.Label(new Rect(card.x + 170, card.y + 66, 129, 18), state, 10, weapon.IsReloading ? HudTheme.Amber : accent, true, TextAnchor.MiddleRight);
            HudTheme.Key(new Rect(card.x + 202, card.y + 88, 25, 23), "R", HudTheme.Muted);
            HudTheme.Label(new Rect(card.x + 235, card.y + 88, 64, 23), "DOLDUR", 10, HudTheme.Muted, true, TextAnchor.MiddleRight);
            float fill = weapon.IsReloading ? weapon.ReloadProgress : weapon.Ammo / (float)weapon.MagazineSize;
            HudTheme.Rounded(new Rect(card.x + 20, card.y + 121, 280, 4), new Color(1, 1, 1, 0.08f), 2);
            if (fill > 0) HudTheme.Rounded(new Rect(card.x + 20, card.y + 121, 280 * fill, 4), weapon.IsReloading ? HudTheme.Amber : HudTheme.Mint, 2);
        }

        private void DrawControls(float width, float height)
        {
            string controls = "WASD: Hareket" + (runningMode ? "     Shift: Koş" : "") + (weapon != null ? "     Fare: Ateş" : "");
            HudTheme.Label(new Rect(width / 2 - 260, height - 30, 520, 24), controls, 13, HudTheme.Muted, false, TextAnchor.MiddleCenter);
        }

        private void DrawCompanion(float width, float height)
        {
            if (robot == null) return;
            var card = new Rect(width / 2 - 225, height - 148, 450, 108);
            HudTheme.Card(card);
            HudTheme.Rounded(new Rect(card.x + 17, card.y + 21, 7, 7), HudTheme.Mint, 3);
            HudTheme.Label(new Rect(card.x + 34, card.y + 12, 395, 22), "BAKIM ROBOTU  /  " + (robot.Busy ? "GÖREVDE" : "BAĞLI"), 10, HudTheme.Mint, true);
            string status = robot.CanCommand ? "Kapıyı açmak için emir bekliyor." : robot.Status.Replace("Robot: ", "");
            HudTheme.Label(new Rect(card.x + 18, card.y + 41, 414, 55), status, 16, HudTheme.Text, false, TextAnchor.UpperLeft, true);
        }

        private void DrawInteraction(float width, float height)
        {
            string action = pickup != null && pickup.CanPickup ? "OTOMATİK TÜFEĞİ AL"
                : robot != null && robot.CanCommand ? "ROBOTA KAPIYI AÇ EMRİ VER" : null;
            if (action == null) return;
            var card = new Rect(width / 2 - 200, height - 205, 400, 58);
            HudTheme.Card(card);
            HudTheme.Key(new Rect(card.x + 13, card.y + 11, 36, 36), "E", HudTheme.Amber);
            HudTheme.Label(new Rect(card.x + 63, card.y + 10, 321, 38), action, 13, HudTheme.Text, true);
        }

        private void DrawExit(float scale, float width, float height)
        {
            if (exit == null || weapon == null || weapon.transform.position.x < 8 || Camera.main == null) return;
            Vector3 point = Camera.main.WorldToScreenPoint(exit.transform.position + new Vector3(-0.8f, 1, 0));
            if (point.z <= 0) return;
            var card = new Rect(Mathf.Clamp(point.x / scale - 95, 28, width - 218),
                Mathf.Clamp((Screen.height - point.y) / scale, 210, height - 240), 190, 40);
            HudTheme.Rounded(card, HudTheme.Background, 8);
            HudTheme.Label(card, stage != null && !stage.CanGoNext ? "KAPI KİLİTLİ"
                : "BÖLÜM " + (stage != null ? stage.StageNumber + 1 : 2) + "  →", 13, HudTheme.Mint, true, TextAnchor.MiddleCenter);
        }

        private void DrawLoading(float width, float height)
        {
            HudTheme.Fill(new Rect(0, 0, width, height), new Color(0.02f, 0.035f, 0.04f, 0.97f));
            HudTheme.Label(new Rect(width / 2 - 250, height / 2 - 60, 500, 48),
                stage.IsLoading ? "SONRAKİ BÖLÜM" : "BÖLÜM YÜKLENEMEDİ", 28, HudTheme.Text, true, TextAnchor.MiddleCenter);
            HudTheme.Label(new Rect(width / 2 - 270, height / 2, 540, 45),
                stage.IsLoading ? "Yeni bölge hazırlanıyor..." : stage.Error, 16, HudTheme.Muted, false, TextAnchor.MiddleCenter, true);
            HudTheme.Rounded(new Rect(width / 2 - 120, height / 2 + 70, 240, 4), new Color(1, 1, 1, 0.08f), 2);
            HudTheme.Rounded(new Rect(width / 2 - 120, height / 2 + 70, 240 * (0.3f + Mathf.PingPong(Time.unscaledTime * 0.6f, 0.7f)), 4), HudTheme.Mint, 2);
        }

        private void DrawGameOver(float width, float height)
        {
            HudTheme.Fill(new Rect(0, 0, width, height), new Color(0.015f, 0.025f, 0.03f, 0.85f));
            var card = new Rect(width / 2 - 250, height / 2 - 143, 500, 286);
            HudTheme.Card(card);
            HudTheme.Label(new Rect(card.x, card.y + 25, 500, 24), "TAHLİYE BAŞARISIZ", 11, HudTheme.Danger, true, TextAnchor.MiddleCenter);
            HudTheme.Label(new Rect(card.x + 20, card.y + 63, 460, 48), "HAYATTA KALAMADIN", 29, HudTheme.Text, true, TextAnchor.MiddleCenter);
            HudTheme.Label(new Rect(card.x + 30, card.y + 125, 440, 30), "Kaldığın bölümden yeniden dene.", 16, HudTheme.Muted, false, TextAnchor.MiddleCenter);
            if (HudTheme.Button(new Rect(card.x + 70, card.y + 181, 360, 48), "BÖLÜMÜ YENİDEN DENE")) session.RestartLevel();
            HudTheme.Label(new Rect(card.x, card.y + 243, 500, 23), "veya ENTER tuşuna bas", 11, HudTheme.Muted, false, TextAnchor.MiddleCenter);
        }
    }
}
