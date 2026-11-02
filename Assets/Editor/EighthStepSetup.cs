using System;
using System.IO;
using Cernobil.Combat;
using Cernobil.Companion;
using Cernobil.Enemies;
using Cernobil.Player;
using Cernobil.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cernobil.Editor
{
    public static class EighthStepSetup
    {
        private const string FirstPath = "Assets/Scenes/09_RobotBolum1.unity";
        private const string SecondPath = "Assets/Scenes/10_RobotBolum2.unity";

        [MenuItem("Cernobil/Adim 8 - Yardimci Robot ve Santral Kapisi")]
        public static void CreateScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Oyun modunu durdur", "Önce üstteki Stop düğmesine bas.", "Tamam");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists("Assets/Scenes/07_Bolum1.unity") || !File.Exists("Assets/Scenes/08_Bolum2.unity"))
            {
                EditorUtility.DisplayDialog("Önce Adım 7", "Önce iki bölümlü kutu ve tüfek sahnelerini oluştur.", "Tamam");
                return;
            }
            var robotArt = ImportArt("Robot.png", 1.15f);
            var gateArt = ImportArt("Gate.png", 3.2f);
            var panelArt = ImportArt("Panel.png", 1.5f);
            var white = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/White.png");
            if (white == null) throw new InvalidOperationException("Temel sprite bulunamadı.");

            var first = EditorSceneManager.OpenScene("Assets/Scenes/07_Bolum1.unity");
            UnityEngine.Object.FindFirstObjectByType<StageProgression>().SetNextScene("10_RobotBolum2");
            EditorSceneManager.SaveScene(first, FirstPath);
            var second = EditorSceneManager.OpenScene("Assets/Scenes/08_Bolum2.unity");
            AddCompanionPuzzle(white, robotArt, gateArt, panelArt);
            EditorSceneManager.SaveScene(second, SecondPath);
            CompanionNavMeshValidation.ValidateScene();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(FirstPath, true),
                new EditorBuildSettingsScene(SecondPath, true)
            };
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(FirstPath);
            Debug.Log("Adım 8 hazır. İkinci bölümde sağ kapının yanına git ve E ile robota emir ver.");
        }

        private static void AddCompanionPuzzle(Sprite white, Sprite robotArt, Sprite gateArt, Sprite panelArt)
        {
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            if (player == null) throw new InvalidOperationException("Oyuncu bulunamadı.");
            var world = new GameObject("Robot Navigation World").AddComponent<CompanionNavigationWorld>();

            Wall(white, "Santral alt duvar", new Vector2(9.5f, -5.25f), new Vector2(0.7f, 5.5f));
            Wall(white, "Santral orta duvar", new Vector2(9.5f, 2.45f), new Vector2(0.7f, 3.9f));
            Wall(white, "Santral ust duvar", new Vector2(9.5f, 6.3f), new Vector2(0.7f, 1.8f));
            Wall(white, "Servis pompa blogu", new Vector2(11.1f, 3.2f), new Vector2(1.1f, 1.5f));
            // Gap: y=4.4..5.4 is 1.0 unit. Player's 1.4-unit collider cannot pass;
            // the real NavMeshAgent has a 0.2-unit radius and finds this route itself.
            var passage = Draw(white, "Robot servis gecidi", new Vector2(9.5f, 4.9f),
                new Vector2(0.75f, 1), new Color(0.1f, 0.35f, 0.35f), -850);
            for (int i = 0; i < 3; i++)
                Draw(white, "Servis zemin isareti", new Vector2(9.2f + i * 0.3f, 4.9f),
                    new Vector2(0.08f, 0.7f), new Color(0.2f, 0.75f, 0.75f, 0.65f), -800);

            var gateObject = new GameObject("Kilitli Santral Kapisi");
            gateObject.transform.position = new Vector3(9.5f, -1, 0);
            var barrier = gateObject.AddComponent<BoxCollider2D>();
            barrier.size = new Vector2(0.7f, 3);
            var shutter = new GameObject("Acilan Metal Kapi");
            shutter.transform.SetParent(gateObject.transform, false);
            var gateRenderer = shutter.AddComponent<SpriteRenderer>();
            gateRenderer.sprite = gateArt;
            gateRenderer.sortingOrder = 190;
            var gate = gateObject.AddComponent<SantralGate>();
            gate.Configure(barrier, shutter.transform, world);

            var command = Draw(white, "Robot Emir Noktasi", new Vector2(7, -1),
                new Vector2(1.1f, 1.1f), new Color(0.9f, 0.7f, 0.15f, 0.4f), -800);
            var panelObject = new GameObject("Santral Kontrol Paneli");
            panelObject.transform.position = new Vector3(12.5f, 0, 0);
            var panelVisual = new GameObject("Panel Gorunumu");
            panelVisual.transform.SetParent(panelObject.transform, false);
            panelVisual.transform.localPosition = new Vector3(0, 0.8f, 0);
            var panelRenderer = panelVisual.AddComponent<SpriteRenderer>();
            panelRenderer.sprite = panelArt;
            panelRenderer.sortingOrder = 110;
            panelVisual.AddComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.6f);
            var indicator = Draw(white, "Panel gostergesi", new Vector2(12.5f, 1.05f),
                new Vector2(0.2f, 0.12f), Color.cyan, 120).GetComponent<SpriteRenderer>();
            indicator.transform.SetParent(panelObject.transform, true);
            var panel = panelObject.AddComponent<GateControlPanel>();

            var robotObject = new GameObject("Yardimci Bakim Robotu");
            robotObject.transform.position = new Vector3(-12.2f, -3.2f, 0);
            var robotRenderer = robotObject.AddComponent<SpriteRenderer>();
            robotRenderer.sprite = robotArt;
            robotRenderer.sortingOrder = 100;
            robotObject.AddComponent<RobotObstacleSensor>();
            var robot = robotObject.AddComponent<CompanionRobot>();
            robot.Configure(world, player.transform, player.GetComponent<Health>(), panel, gate,
                command.transform, robotRenderer);
            panel.Configure(gate, robot, indicator);
            new GameObject("Robot Gorev Bilgisi").AddComponent<CompanionTaskHUD>().Configure(robot);
            UnityEngine.Object.FindFirstObjectByType<StageProgression>().SetGate(gate);

            foreach (var collider in UnityEngine.Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None))
            {
                if (collider.isTrigger || collider.GetComponentInParent<PlayerMovement>() != null ||
                    collider.GetComponentInParent<ZombieChase>() != null) continue;
                if (collider.GetComponent<NavigationObstacle2D>() == null)
                    collider.gameObject.AddComponent<NavigationObstacle2D>();
            }
            ValidateLayout(player, barrier);
        }

        private static void ValidateLayout(PlayerMovement player, BoxCollider2D gate)
        {
            if (player.GetComponent<BoxCollider2D>().size.y <= 1)
                throw new InvalidOperationException("Servis geçidi oyuncunun geçemeyeceği kadar dar olmalı.");
            if (!gate.enabled || UnityEngine.Object.FindObjectsByType<ZombieChase>(FindObjectsSortMode.None).Length != 5)
                throw new InvalidOperationException("Kapı başlangıcı veya beş zombili ikinci bölüm korunamadı.");
        }

        private static void Wall(Sprite white, string name, Vector2 position, Vector2 size)
        {
            var wall = Draw(white, name, position, size, new Color(0.23f, 0.27f, 0.25f), 80);
            // Collider size stays in the scaled object's local unit square.
            wall.AddComponent<BoxCollider2D>();
            Draw(white, name + " metal hat", position, new Vector2(size.x * 0.4f, size.y),
                new Color(0.42f, 0.43f, 0.34f), 81);
            for (float y = -size.y / 2 + 0.3f; y < size.y / 2; y += 0.7f)
                Draw(white, name + " percin", position + new Vector2(0, y), new Vector2(size.x * 0.7f, 0.06f),
                    new Color(0.65f, 0.55f, 0.24f), 82);
        }

        private static GameObject Draw(Sprite sprite, string name, Vector2 position, Vector2 size, Color color, int order)
        {
            var item = new GameObject(name);
            item.transform.position = position;
            item.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return item;
        }

        private static Sprite ImportArt(string file, float worldHeight)
        {
            string path = "Assets/Art/Companion/" + file;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Görsel bulunamadı: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            importer.spritePixelsPerUnit = height / worldHeight;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
