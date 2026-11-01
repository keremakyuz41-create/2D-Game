using System.Collections;
using Cernobil.Combat;
using Cernobil.Enemies;
using Cernobil.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cernobil.Companion;

namespace Cernobil.World
{
    public sealed class StageProgression : MonoBehaviour
    {
        [SerializeField] private int stageNumber = 1;
        [SerializeField] private string nextScene;
        [SerializeField] private Health playerHealth;
        [SerializeField] private PlayerWeapon weapon;
        [SerializeField] private WeaponCrate crate;
        [SerializeField] private SantralGate santralGate;
        [SerializeField] private ShelterGate shelterGate;
        public void SetShelter(ShelterGate gate) => shelterGate = gate;
        public void SetNextScene(string scene) => nextScene = scene;
        public void SetGate(SantralGate gate) => santralGate = gate;
        private ZombieChase[] zombies;
        private Cernobil.Dialogue.NpcDialogue npc;
        public bool IsLoading { get; private set; }
        public string Error { get; private set; }
        public int StageNumber => stageNumber;
        public bool CanGoNext => !string.IsNullOrEmpty(nextScene) && (santralGate == null || santralGate.PassageOpen);
        public int RemainingZombies
        {
            get
            {
                int count = 0;
                if (zombies != null) foreach (var zombie in zombies) if (zombie != null) count++;
                return count;
            }
        }
        public string Objective => stageNumber == 1 ? "Sağdaki çıkışa ilerle → BÖLÜM 2"
            : stageNumber == 3 ? (shelterGate != null && shelterGate.Completed ? "Tahliye tamamlandı. Sığınaktasın!"
                : shelterGate != null && shelterGate.Authorized ? "Giriş iznin var. Açılan kapıdan içeri ilerle."
                : "Görevliye yaklaş. E ile konuş, sığınağa giriş izni al.")
            : CanGoNext && santralGate != null ? "Kapı açık. En sağdaki çıkıştan sığınağa ilerle → BÖLÜM 3"
            : npc != null && npc.MissionCompleted ? "Tahliye görevi tamamlandı. Bölge güvenli!"
            : weapon.CurrentKind != WeaponKind.Rifle
                ? (crate != null ? "Kutuyu vurarak kır, tüfeği E ile al." : "Tüfeğe yaklaş ve E ile al.")
                : RemainingZombies > 0 ? "Tüfek kuşanıldı. Kalan zombi: " + RemainingZombies
                : santralGate != null ? (santralGate.PassageOpen ? (npc != null
                    ? (npc.MissionAssigned ? "Bölge temizlendi. Teknisyene geri dön." : "Teknisyene yaklaş, E ile tahliye görevini al.")
                    : "Kapı açık. Santral alanına geç.")
                    : "Santral kapısına ilerle. Robota emir ver.") : "Bölge temizlendi. Tüfek sende!";

        public void Configure(int number, string next, Health health, PlayerWeapon gun, WeaponCrate supplyCrate)
        {
            stageNumber = number;
            nextScene = next;
            playerHealth = health;
            weapon = gun;
            crate = supplyCrate;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetTransfer() => LevelTransfer.Clear();

        private void Start()
        {
            zombies = FindObjectsByType<ZombieChase>(FindObjectsSortMode.None);
            npc = FindFirstObjectByType<Cernobil.Dialogue.NpcDialogue>();
            if (LevelTransfer.TryTake(gameObject.scene.name, out var snapshot))
            {
                playerHealth.RestoreCurrent(snapshot.Health);
                weapon.RestoreLoadout(snapshot.Weapon, snapshot.Ammo);
            }
        }

        public void GoToNext()
        {
            if (IsLoading || !playerHealth.IsAlive || !CanGoNext || Cernobil.Dialogue.NpcDialogue.BlocksPlayerInput) return;
            if (!Application.CanStreamedLevelBeLoaded(nextScene))
            {
                Error = "Bölüm yüklenemedi. Sahnenin Build Settings listesinde açık olduğunu kontrol et.";
                return;
            }
            IsLoading = true;
            LevelTransfer.Store(nextScene, new PlayerSnapshot(playerHealth.Current, weapon.CurrentKind, weapon.Ammo));
            playerHealth.GetComponent<PlayerMovement>().enabled = false;
            weapon.enabled = false;
            foreach (var chase in FindObjectsByType<ZombieChase>(FindObjectsSortMode.None)) chase.enabled = false;
            foreach (var attack in FindObjectsByType<ZombieAttack>(FindObjectsSortMode.None)) attack.enabled = false;
            StartCoroutine(LoadNext());
        }

        private IEnumerator LoadNext()
        {
            yield return null;
            yield return SceneManager.LoadSceneAsync(nextScene);
        }
    }
}
