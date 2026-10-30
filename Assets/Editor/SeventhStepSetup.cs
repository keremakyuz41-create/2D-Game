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
    public static class SeventhStepSetup
    {
        private const string ArtRoot = "Assets/Art/Street/";
        private const string Source = "Assets/Scenes/06_KosuVeSilah.unity";
        private const string FirstPath = "Assets/Scenes/07_Bolum1.unity";
        private const string SecondPath = "Assets/Scenes/08_Bolum2.unity";

        [MenuItem("Cernobil/Adim 7 - Iki Bolum Kutu ve Tufek")]
        public static void CreateScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Oyun modunu durdur", "Önce üstteki Stop düğmesine bas.", "Tamam");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(Source))
            {
                EditorUtility.DisplayDialog("Önce Adım 6", "Önce koşu ve silah sahnesini oluştur.", "Tamam");
                return;
            }

            Sprite crateArt = ImportSprite("SupplyCrate.png", 1.8f, new Vector2(0.5f, 0.5f));
            Sprite rifleArt = ImportSprite("Rifle.png", 1.8f, new Vector2(0.35f, 0.45f));
            Sprite depot = ImportSprite("DepotStreet.png", 31, new Vector2(0.5f, 0.5f));
            Sprite pistolArt = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + "Pistol.png");
            Sprite white = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/White.png");
            if (pistolArt == null || white == null) throw new InvalidOperationException("Tabanca veya temel sprite bulunamadı.");
            WeaponDefinition pistol = Definition("Pistol", WeaponKind.Pistol, "Tabanca", pistolArt,
                12, 0.2f, 1.2f, false, new Vector2(0.65f, 0.28f));
            WeaponDefinition rifle = Definition("Rifle", WeaponKind.Rifle, "Otomatik Tüfek", rifleArt,
                24, 0.09f, 1.6f, true, new Vector2(1.16f, 0.17f));

            CreateFirst(pistol, rifle, white);
            CreateSecond(pistol, rifle, crateArt, rifleArt, depot);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(FirstPath, true),
                new EditorBuildSettingsScene(SecondPath, true)
            };
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(FirstPath);
            Debug.Log("İki bölüm hazır. İlk bölümde sağa ilerle; ikincide kutuyu kırıp E ile tüfeği al.");
        }

        private static void CreateFirst(WeaponDefinition pistol, WeaponDefinition rifle, Sprite white)
        {
            var scene = EditorSceneManager.OpenScene(Source);
            var player = Player();
            player.GetComponent<PlayerWeapon>().ConfigureLoadout(pistol, rifle);
            var flow = new GameObject("Bolum Akisi").AddComponent<StageProgression>();
            flow.Configure(1, "08_Bolum2", player.GetComponent<Health>(), player.GetComponent<PlayerWeapon>(), null);
            UnityEngine.Object.FindFirstObjectByType<PrototypeOverlay>().ConfigureStage(flow);
            var exit = new GameObject("Sag Cikis - Bolum 2");
            exit.transform.position = new Vector3(13.9f, 0, 0);
            var trigger = exit.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.2f, 14);
            exit.AddComponent<LevelExit>().Configure(flow);
            for (int i = -3; i <= 3; i++)
            {
                var marker = new GameObject("Cikis isareti");
                marker.transform.SetParent(exit.transform, false);
                marker.transform.localPosition = new Vector3(0, i * 1.5f, 0);
                marker.transform.localScale = new Vector3(0.15f, 0.8f, 1);
                var visual = marker.AddComponent<SpriteRenderer>();
                visual.sprite = white;
                visual.color = new Color(0.25f, 0.85f, 0.72f, 0.75f);
                visual.sortingOrder = -900;
            }
            EditorSceneManager.SaveScene(scene, FirstPath);
        }

        private static void CreateSecond(WeaponDefinition pistol, WeaponDefinition rifle, Sprite crateArt,
            Sprite rifleArt, Sprite depot)
        {
            var scene = EditorSceneManager.OpenScene(Source);
            var player = Player();
            player.transform.position = new Vector3(-11, -2, 0);
            var weapon = player.GetComponent<PlayerWeapon>();
            var health = player.GetComponent<Health>();
            weapon.ConfigureLoadout(pistol, rifle);
            Camera.main.transform.position = new Vector3(-3.2f, -2, -10);

            var environment = GameObject.Find("Terk Edilmis Sokak");
            environment.name = "Ikmal Deposu Sokagi";
            environment.GetComponent<SpriteRenderer>().sprite = depot;
            environment.transform.localScale = new Vector3(31 / depot.bounds.size.x, 21 / depot.bounds.size.y, 1);
            var carCollision = GameObject.Find("Park halindeki arac");
            if (carCollision != null) UnityEngine.Object.DestroyImmediate(carCollision);

            var zombies = UnityEngine.Object.FindObjectsByType<ZombieChase>(FindObjectsSortMode.InstanceID);
            if (zombies.Length != 3) throw new InvalidOperationException("Kaynak sahnede üç zombi bekleniyor.");
            var positions = new[] { new Vector2(-1, -2), new Vector2(3, 1), new Vector2(6, -3),
                new Vector2(8, 3), new Vector2(-1, -5) };
            for (int i = 0; i < 5; i++)
            {
                var zombie = i < 3 ? zombies[i].gameObject : UnityEngine.Object.Instantiate(zombies[i - 3].gameObject);
                zombie.name = "Bolum 2 Zombi " + (i + 1);
                zombie.transform.position = positions[i];
                zombie.GetComponent<ZombieChase>().SetTarget(player.transform);
                zombie.GetComponent<ZombieAttack>().SetTarget(health);
            }

            var pickup = new GameObject("Kutudan Cikan Tufek");
            pickup.transform.position = new Vector3(-6, -1.7f, 0);
            var dropVisual = pickup.AddComponent<SpriteRenderer>();
            dropVisual.sprite = rifleArt;
            dropVisual.sortingOrder = 100;
            pickup.AddComponent<WeaponPickup>().Configure(weapon, health);
            pickup.SetActive(false);

            var crate = new GameObject("Kirilabilir Ikmal Kutusu");
            crate.transform.position = new Vector3(-6, -2, 0);
            var crateVisual = crate.AddComponent<SpriteRenderer>();
            crateVisual.sprite = crateArt;
            crateVisual.sortingOrder = 120;
            crate.AddComponent<BoxCollider2D>().size = new Vector2(1.5f, 1);
            crate.AddComponent<Health>().Configure(3, true);
            crate.AddComponent<TargetFeedback>();
            var supply = crate.AddComponent<WeaponCrate>();
            supply.Configure(pickup);

            var flow = new GameObject("Bolum Akisi").AddComponent<StageProgression>();
            flow.Configure(2, null, health, weapon, supply);
            UnityEngine.Object.FindFirstObjectByType<PrototypeOverlay>().ConfigureStage(flow);
            if (UnityEngine.Object.FindObjectsByType<ZombieChase>(FindObjectsSortMode.None).Length != 5)
                throw new InvalidOperationException("İkinci bölümde tam beş zombi oluşturulamadı.");
            EditorSceneManager.SaveScene(scene, SecondPath);
        }

        private static PlayerMovement Player()
        {
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            if (player == null) throw new InvalidOperationException("Oyuncu bulunamadı.");
            // Reuse the repaired, individually persisted animation frames in the source scene.
            foreach (var renderer in player.GetComponentsInChildren<SpriteRenderer>())
                if (renderer.enabled && renderer.sprite == null)
                    throw new InvalidOperationException("Önce Cernobil menüsündeki Karakter Kaybolmasini Duzelt seçeneğini kullan.");
            return player;
        }

        private static WeaponDefinition Definition(string file, WeaponKind kind, string label, Sprite sprite,
            int capacity, float interval, float reload, bool automatic, Vector2 barrel)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data")) AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/Weapons")) AssetDatabase.CreateFolder("Assets/Data", "Weapons");
            string path = "Assets/Data/Weapons/" + file + ".asset";
            var definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<WeaponDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }
            definition.Configure(kind, label, sprite, capacity, interval, reload, automatic, barrel);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static Sprite ImportSprite(string file, float worldWidth, Vector2 pivot)
        {
            string path = ArtRoot + file;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Görsel bulunamadı: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            importer.spritePixelsPerUnit = width / worldWidth;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 4096;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = 9;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException("Sprite yüklenemedi: " + path);
            return sprite;
        }
    }
}
