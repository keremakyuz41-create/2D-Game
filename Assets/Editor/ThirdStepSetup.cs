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
    public static class ThirdStepSetup
    {
        [MenuItem("Cernobil/Adim 3 - Yavas Zombileri Olustur")]
        public static void CreateScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Oyun modunu durdur", "Önce üstteki Stop düğmesine bas.", "Tamam");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string sourcePath = "Assets/Scenes/02_AtisDenemesi.unity";
            if (!File.Exists(sourcePath))
            {
                EditorUtility.DisplayDialog("Atış sahnesi gerekli", "Önce Cernobil menüsünden Adim 2 sahnesini oluştur.", "Tamam");
                return;
            }
            var tile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/White.png");
            if (tile == null) throw new InvalidOperationException("White.png sprite bulunamadı.");
            var scene = EditorSceneManager.OpenScene(sourcePath);
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            if (player == null) throw new InvalidOperationException("Oyuncu bulunamadı.");
            var targets = UnityEngine.Object.FindObjectsByType<Health>(FindObjectsSortMode.InstanceID);
            if (targets.Length != 3) throw new InvalidOperationException("Adim 2 sahnesinde üç deneme hedefi bekleniyor.");
            // Open starting positions keep the first chase demonstration clear of barriers.
            var positions = new[] { new Vector2(-1, -2), new Vector2(1, -4), new Vector2(-5, -6) };
            player.transform.position = new Vector3(-5, -2, 0);
            for (int i = 0; i < targets.Length; i++)
            {
                var zombie = targets[i].gameObject;
                zombie.name = "Yavas Zombi " + (i + 1);
                zombie.transform.position = positions[i];
                zombie.GetComponent<SpriteRenderer>().color = new Color(0.38f, 0.49f, 0.28f);
                var body = zombie.AddComponent<Rigidbody2D>();
                body.gravityScale = 0;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                zombie.AddComponent<ZombieChase>().SetTarget(player.transform);

                Detail(zombie.transform, tile, "Bas", new Vector2(0, 0.2f),
                    new Vector2(0.4f, 0.34f), new Color(0.61f, 0.67f, 0.43f));
                Detail(zombie.transform, tile, "Sol goz", new Vector2(-0.1f, 0.22f),
                    new Vector2(0.055f, 0.06f), new Color(0.9f, 0.25f, 0.1f));
                Detail(zombie.transform, tile, "Sag goz", new Vector2(0.1f, 0.22f),
                    new Vector2(0.055f, 0.06f), new Color(0.9f, 0.25f, 0.1f));
            }
            UnityEngine.Object.FindFirstObjectByType<PrototypeOverlay>().ConfigureZombies();
            const string scenePath = "Assets/Scenes/03_ZombiDenemesi.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player.gameObject;
            Debug.Log("Adim 3 hazır: 3 yavaş zombi oyuncuya yaklaşır. Her biri 3 isabette yok olur.");
        }

        private static void Detail(Transform parent, Sprite sprite, string name, Vector2 position,
            Vector2 size, Color color)
        {
            var detail = new GameObject(name);
            detail.transform.SetParent(parent, false);
            detail.transform.localPosition = position;
            detail.transform.localScale = new Vector3(size.x, size.y, 1);
            var visual = detail.AddComponent<SpriteRenderer>();
            visual.sprite = sprite;
            visual.color = color;
            visual.sortingOrder = 11;
        }
    }
}
