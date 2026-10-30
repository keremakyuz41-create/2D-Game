using System;
using System.IO;
using Cernobil.Combat;
using Cernobil.Dialogue;
using Cernobil.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cernobil.Editor
{
    [InitializeOnLoad]
    public static class RestartPlayValidation
    {
        private const string Armed = "Cernobil.RestartValidation";
        private static int phase;
        private static double started;
        static RestartPlayValidation()
        {
            EditorApplication.playModeStateChanged += state => {
                if (!SessionState.GetBool(Armed, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode) { phase = 1; started = EditorApplication.timeSinceStartup; }
                else if (state == PlayModeStateChange.ExitingPlayMode && phase != 0) Finish(false, "Test durduruldu.");
            };
            EditorApplication.update += Tick;
        }
        [MenuItem("Cernobil/Final - Bastan Oyna Testi")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/Scenes/15_SonTahliye.unity");
            SessionState.SetBool(Armed, true);
            EditorApplication.isPlaying = true;
        }
        private static void Tick()
        {
            if (phase == 0 || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            if (EditorApplication.timeSinceStartup - started > 15) { Finish(false, "Zaman aşımı."); return; }
            if (EditorApplication.timeSinceStartup - started < 1) return;
            if (phase == 1)
            {
                // A stale transfer must not carry late-game health/rifle/ammo into a new run.
                LevelTransfer.Store("13_FinalBolum1", new PlayerSnapshot(20, WeaponKind.Rifle, 2));
                phase = 2;
                UnityEngine.Object.FindFirstObjectByType<SurvivalSession>().RestartGame();
            }
            else if (SceneManager.GetActiveScene().name == "13_FinalBolum1")
            {
                var session = UnityEngine.Object.FindFirstObjectByType<SurvivalSession>();
                var stage = UnityEngine.Object.FindFirstObjectByType<StageProgression>();
                var gun = UnityEngine.Object.FindFirstObjectByType<PlayerWeapon>();
                if (session == null || stage == null || gun == null) return;
                bool valid = stage.StageNumber == 1 && session.PlayerHealth.Current == session.PlayerHealth.Maximum &&
                    gun.CurrentKind == WeaponKind.Pistol && gun.Ammo == gun.MagazineSize && !NpcDialogue.BlocksPlayerInput;
                Finish(valid, valid ? "Bölüm 3 → Bölüm 1; tam can, tabanca, dolu şarjör ve açık kontroller doğrulandı." : "Başlangıç durumu beklenen değerlerde değil.");
            }
        }
        private static void Finish(bool success, string detail)
        {
            File.WriteAllText("Bastan-Oyna-Test-Sonuclari.json", "{\"status\":\"" + (success ? "passed" : "failed") + "\",\"detail\":\"" + detail + "\"}");
            phase = 0; SessionState.SetBool(Armed, false);
            if (success) Debug.Log(detail); else Debug.LogWarning(detail);
        }
    }
}
