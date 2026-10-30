using System;
using System.IO;
using Cernobil.Combat;
using Cernobil.Companion;
using Cernobil.Dialogue;
using Cernobil.Enemies;
using Cernobil.Player;
using Cernobil.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cernobil.Editor
{
    public static class TenthStepSetup
    {
        private const string First = "Assets/Scenes/13_FinalBolum1.unity";
        private const string Second = "Assets/Scenes/14_FinalBolum2.unity";
        private const string Third = "Assets/Scenes/15_SonTahliye.unity";

        [MenuItem("Cernobil/Final - Siginak Sahnesini Ac")]
        public static void OpenShelter()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Third)) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(Third);
            var door = GameObject.Find("Acilan Siginak Kepengi").GetComponent<SpriteRenderer>();
            if (door.sprite.name != "Shelter Door Crop")
                door.sprite = CroppedDoor();
            door.transform.localScale = new Vector3(4.15f / door.sprite.bounds.size.x, 3.2f / door.sprite.bounds.size.y, 1);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        [MenuItem("Cernobil/Adim 10 - Ucuncu Bolum Siginak ve Serbest Konusma")]
        public static void CreateScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { EditorUtility.DisplayDialog("Önce oyunu durdur", "Üstteki Stop düğmesine bas.", "Tamam"); return; }
            if (!File.Exists("Assets/Scenes/11_NpcBolum1.unity") || !File.Exists("Assets/Scenes/12_NpcBolum2.unity"))
            { EditorUtility.DisplayDialog("Önce Adım 9", "Önce yerel NPC sahnelerini oluştur.", "Tamam"); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(First) || File.Exists(Second) || File.Exists(Third))
            { EditorUtility.DisplayDialog("Final sahneleri mevcut", "13_FinalBolum1, 14_FinalBolum2 ve 15_SonTahliye sahnelerini kullan. Mevcut düzenlemelerinin üzerine yazılmadı.", "Tamam"); return; }

            Sprite background = Import("Assets/Art/Shelter/ShelterCourtyard.png", 31f, false);
            Sprite guard = Import("Assets/Art/Shelter/ShelterGuard.png", 2.45f, true);
            Sprite metal = CroppedDoor();
            Sprite robot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Companion/Robot.png");
            Sprite white = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/White.png");
            if (background == null || guard == null || metal == null || robot == null || white == null)
                throw new InvalidOperationException("Sığınak görsellerinden biri bulunamadı.");

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/12_NpcBolum2.unity");
            foreach (var npc in UnityEngine.Object.FindObjectsByType<NpcDialogue>(FindObjectsSortMode.None))
                UnityEngine.Object.DestroyImmediate(npc.gameObject);
            var flow = UnityEngine.Object.FindFirstObjectByType<StageProgression>();
            flow.SetNextScene("15_SonTahliye");
            foreach (var oldExit in UnityEngine.Object.FindObjectsByType<LevelExit>(FindObjectsSortMode.None)) UnityEngine.Object.DestroyImmediate(oldExit.gameObject);
            var exit = new GameObject("Sag Cikis - Siginak Bolum 3");
            exit.transform.position = new Vector3(13.9f, 0, 0);
            var trigger = exit.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.1f, 14);
            exit.AddComponent<LevelExit>().Configure(flow);
            for (int i = -3; i <= 3; i++)
            {
                var marker = Draw(white, "Siginak cikis isareti", new Vector2(13.8f, i * 1.5f), new Vector2(0.12f, 0.65f), -700);
                marker.color = new Color(0.3f, 0.9f, 0.72f, 0.7f);
            }
            EditorSceneManager.SaveScene(scene, Second);

            CreateShelter(background, guard, metal, robot);

            scene = EditorSceneManager.OpenScene("Assets/Scenes/11_NpcBolum1.unity");
            UnityEngine.Object.FindFirstObjectByType<StageProgression>().SetNextScene("14_FinalBolum2");
            EditorSceneManager.SaveScene(scene, First);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(First, true), new EditorBuildSettingsScene(Second, true), new EditorBuildSettingsScene(Third, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Adım 10 hazır. İkinci bölümde robot kapısını açıp en sağa git. Üçüncü bölümde görevliyle E ile serbest konuş, izin alıp kapıdan geç.");
        }

        private static void CreateShelter(Sprite background, Sprite guardArt, Sprite doorArt, Sprite robotArt)
        {
            // Clone only the player rig (including run animation, gun and camera configuration) into a clean scene.
            var source = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            var player = UnityEngine.Object.Instantiate(source.gameObject);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.MoveGameObjectToScene(player, scene);
            SceneManager.SetActiveScene(scene);
            foreach (var other in new[] { source.gameObject.scene }) EditorSceneManager.CloseScene(other, true);
            player.name = "Player";
            player.transform.position = new Vector3(0, -4.5f, 0);
            var health = player.GetComponent<Health>();
            var weapon = player.GetComponent<PlayerWeapon>();
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.75f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.08f, 0.06f);
            cameraObject.AddComponent<AudioListener>();
            Draw(background, "Siginak Avlusu", Vector2.zero, new Vector2(31, 31f * background.rect.height / background.rect.width), -1000);

            Wall("Sol sinir", new Vector2(-14.5f, 0), new Vector2(1, 18));
            Wall("Sag sinir", new Vector2(14.5f, 0), new Vector2(1, 18));
            Wall("Alt sinir", new Vector2(0, -8.2f), new Vector2(30, 0.6f));
            Wall("Bunker ust sinir", new Vector2(0, 8.3f), new Vector2(30, 0.6f));
            Wall("Bunker sol duvar", new Vector2(-9.1f, 6.3f), new Vector2(14, 3.8f));
            Wall("Bunker sag duvar", new Vector2(9.1f, 6.3f), new Vector2(14, 3.8f));
            Wall("Cadir ve sandiklar", new Vector2(-12, 2.2f), new Vector2(3, 4));
            Wall("Hurda minibüs", new Vector2(12, 1.2f), new Vector2(3.2f, 3.5f));

            var stage = new GameObject("Bolum Akisi").AddComponent<StageProgression>();
            stage.Configure(3, null, health, weapon, null);
            var session = new GameObject("Hayatta Kalma Oturumu").AddComponent<SurvivalSession>();
            session.SetPlayer(health);
            var hud = new GameObject("Modern Oyun Arayuzu").AddComponent<PrototypeOverlay>();
            hud.ConfigureCombat(weapon); hud.ConfigureStage(stage); hud.ConfigureSurvival(); hud.ConfigureStreet(); hud.ConfigureRunning();

            var doorObject = new GameObject("Kilitli Siginak Kapisi");
            doorObject.transform.position = new Vector3(0, 4.6f, 0);
            var collider = doorObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(4.6f, 0.65f);
            doorObject.AddComponent<NavigationObstacle2D>();
            var door = Draw(doorArt, "Acilan Siginak Kepengi", new Vector2(-0.1f, 6.1f), new Vector2(4.15f, 3.2f), -500);
            door.transform.SetParent(doorObject.transform, true);
            var shelter = doorObject.AddComponent<ShelterGate>();
            stage.SetShelter(shelter);
            var guard = Draw(guardArt, "Siginak Gorevlisi Mikhail", new Vector2(0.15f, 2.4f), new Vector2(guardArt.bounds.size.x, guardArt.bounds.size.y), 76);
            var npc = guard.gameObject.AddComponent<NpcDialogue>();
            npc.ConfigureGuard(player.transform, health, stage, shelter, guardArt);
            shelter.Configure(collider, door, npc, health);

            var destination = new GameObject("Siginak Ici - Tahliye Cikisi");
            destination.transform.position = new Vector3(0, 6.2f, 0);
            var finish = destination.AddComponent<BoxCollider2D>();
            finish.isTrigger = true;
            finish.size = new Vector2(3.2f, 1.1f);
            destination.AddComponent<ShelterExit>().Configure(shelter);
            var navigation = new GameObject("Siginak Robot NavMesh").AddComponent<CompanionNavigationWorld>();
            var robot = Draw(robotArt, "Yardimci Bakim Robotu", new Vector2(-2, -5), new Vector2(robotArt.bounds.size.x, robotArt.bounds.size.y), 150);
            robot.gameObject.AddComponent<ShelterRobotFollower>().Configure(navigation, player.transform, health, shelter);
            EditorSceneManager.SaveScene(scene, Third);
        }

        private static void Wall(string name, Vector2 position, Vector2 size)
        {
            var wall = new GameObject(name); wall.transform.position = position;
            wall.AddComponent<BoxCollider2D>().size = size;
            wall.AddComponent<NavigationObstacle2D>();
        }
        private static Sprite CroppedDoor()
        {
            const string path = "Assets/Art/Shelter/ShelterDoor.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
            var original = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Companion/Gate.png");
            var cropped = Sprite.Create(original.texture, new Rect(221, 37, 582, 1463), new Vector2(0.5f, 0.5f), 1463 / 3.2f);
            cropped.name = "Shelter Door Crop";
            AssetDatabase.CreateAsset(cropped, path);
            return cropped;
        }
        private static SpriteRenderer Draw(Sprite art, string name, Vector2 position, Vector2 size, int order)
        {
            var item = new GameObject(name); item.transform.position = position;
            item.transform.localScale = new Vector3(size.x / art.bounds.size.x, size.y / art.bounds.size.y, 1);
            var renderer = item.AddComponent<SpriteRenderer>(); renderer.sprite = art; renderer.sortingOrder = order;
            return renderer;
        }
        private static Sprite Import(string path, float dimension, bool height)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Görsel bulunamadı: " + path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.GetSourceTextureWidthAndHeight(out int width, out int pixelsHigh);
            importer.spritePixelsPerUnit = (height ? pixelsHigh : width) / dimension;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
