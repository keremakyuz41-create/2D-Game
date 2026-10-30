using System;
using System.IO;
using Cernobil.Combat;
using Cernobil.Enemies;
using Cernobil.Player;
using Cernobil.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cernobil.Editor
{
    public static class FourthStepSetup
    {
        [MenuItem("Cernobil/Adim 4 - Saldiri ve Can Sahnesini Olustur")]
        public static void CreateScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Oyun modunu durdur", "Önce üstteki Stop düğmesine bas.", "Tamam");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string sourcePath = "Assets/Scenes/03_ZombiDenemesi.unity";
            if (!File.Exists(sourcePath))
            {
                EditorUtility.DisplayDialog("Zombi sahnesi gerekli", "Önce Adim 3 sahnesini oluştur.", "Tamam");
                return;
            }
            var scene = EditorSceneManager.OpenScene(sourcePath);
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            var zombies = UnityEngine.Object.FindObjectsByType<ZombieChase>(FindObjectsSortMode.None);
            if (player == null || zombies.Length == 0) throw new InvalidOperationException("Oyuncu veya zombiler bulunamadı.");
            var health = player.gameObject.AddComponent<Health>();
            health.Configure(100, false, 0.6f);
            player.gameObject.AddComponent<PlayerLife>();
            foreach (var zombie in zombies)
                zombie.gameObject.AddComponent<ZombieAttack>().SetTarget(health);
            var session = new GameObject("Hayatta Kalma Oturumu").AddComponent<SurvivalSession>();
            session.SetPlayer(health);
            UnityEngine.Object.FindFirstObjectByType<PrototypeOverlay>().ConfigureSurvival();
            const string path = "Assets/Scenes/04_HayattaKalma.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player.gameObject;
            Debug.Log("Adim 4 hazır: 100 can, yakın mesafede 10 hasar, ölüm ekranı ve yeniden başlama.");
        }
    }
}
