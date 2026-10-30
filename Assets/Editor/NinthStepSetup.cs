using System;
using System.IO;
using Cernobil.Combat;
using Cernobil.Companion;
using Cernobil.Dialogue;
using Cernobil.Player;
using Cernobil.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cernobil.Editor
{
    public static class NinthStepSetup
    {
        private const string First = "Assets/Scenes/11_NpcBolum1.unity";
        private const string Second = "Assets/Scenes/12_NpcBolum2.unity";

        [MenuItem("Cernobil/Adim 9 - Yerel Yapay Zeka NPC")]
        public static void CreateScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Önce oyunu durdur", "Üstteki Stop düğmesine bas, sonra tekrar dene.", "Tamam");
                return;
            }
            if (!File.Exists("Assets/Scenes/09_RobotBolum1.unity") || !File.Exists("Assets/Scenes/10_RobotBolum2.unity"))
            {
                EditorUtility.DisplayDialog("Robot sahneleri gerekli", "Önce Adım 8 sahnelerini oluştur.", "Tamam");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            // Re-running the menu does not overwrite edits made to the new scenes.
            if (File.Exists(First) || File.Exists(Second))
            {
                EditorUtility.DisplayDialog("NPC sahneleri mevcut", "Mevcut 11_NpcBolum1 / 12_NpcBolum2 sahnelerini kullan. Sahne değişikliklerin korundu.", "Tamam");
                return;
            }
            var second = EditorSceneManager.OpenScene("Assets/Scenes/10_RobotBolum2.unity");
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            var gate = UnityEngine.Object.FindFirstObjectByType<SantralGate>();
            var stage = UnityEngine.Object.FindFirstObjectByType<StageProgression>();
            var art = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Street/Survivor.png");
            if (player == null || gate == null || stage == null || art == null)
                throw new InvalidOperationException("Oyuncu, santral kapısı, bölüm veya NPC görseli bulunamadı.");
            var npc = new GameObject("Mikhail - Yerel AI Teknisyen");
            npc.transform.position = new Vector3(12.6f, -3.2f, 0);
            var visual = new GameObject("Teknisyen Gorunumu");
            visual.transform.SetParent(npc.transform, false);
            visual.transform.localScale = Vector3.one * 1.8f;
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = art;
            renderer.color = new Color(0.65f, 0.9f, 1f);
            renderer.sortingOrder = 132;
            npc.AddComponent<NpcDialogue>().Configure(player.transform, player.GetComponent<Health>(), gate, stage);
            // Non-solid NPC leaves the narrow route and NavMesh unchanged.
            EditorSceneManager.SaveScene(second, Second);
            var first = EditorSceneManager.OpenScene("Assets/Scenes/09_RobotBolum1.unity");
            UnityEngine.Object.FindFirstObjectByType<StageProgression>().SetNextScene("12_NpcBolum2");
            EditorSceneManager.SaveScene(first, First);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(First, true), new EditorBuildSettingsScene(Second, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Adım 9 hazır. İkinci bölümde robotla kapıyı aç, teknisyene yaklaş ve E'ye bas. Ollama modeli: qwen2.5:3b");
        }
    }
}
