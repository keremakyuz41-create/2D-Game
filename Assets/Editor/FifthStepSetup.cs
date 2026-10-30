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
    public static class FifthStepSetup
    {
        private const string ArtRoot = "Assets/Art/Street/";

        [MenuItem("Cernobil/Adim 5 - Piksel Sokagi Olustur")]
        public static void CreateScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Oyun modunu durdur", "Önce üstteki Stop düğmesine bas.", "Tamam");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string source = "Assets/Scenes/04_HayattaKalma.unity";
            if (!File.Exists(source))
            {
                EditorUtility.DisplayDialog("Önce Adım 4", "Önce saldırı ve can sahnesini oluştur.", "Tamam");
                return;
            }
            Sprite street = ImportSprite("Street.png", 1);
            Sprite survivor = ImportSprite("Survivor.png", 1.45f);
            Sprite zombieArt = ImportSprite("Zombie.png", 1.5f);
            var scene = EditorSceneManager.OpenScene(source);
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            if (player == null) throw new InvalidOperationException("Oyuncu bulunamadı.");

            // Keep physical boundary walls, replace prototype floor art and central barriers.
            var floor = GameObject.Find("Zemin");
            if (floor != null) UnityEngine.Object.DestroyImmediate(floor);
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (renderer.name == "Yol cizgisi" || renderer.name == "Bina kapisi" ||
                    renderer.name == "Harabe bina" || renderer.name.StartsWith("Beton bariyer"))
                    UnityEngine.Object.DestroyImmediate(renderer.gameObject);
                else if (renderer.name.EndsWith("siniri")) renderer.enabled = false;
            }
            var environment = new GameObject("Terk Edilmis Sokak");
            var background = environment.AddComponent<SpriteRenderer>();
            background.sprite = street;
            background.sortingOrder = -1000;
            environment.transform.localScale = new Vector3(31f / street.bounds.size.x,
                21f / street.bounds.size.y, 1);
            Solid("Bina cephesi", new Vector2(0, 9), new Vector2(31, 4));
            Solid("Alt bahce citi", new Vector2(0, -9.4f), new Vector2(31, 2.8f));
            Solid("Park halindeki arac", new Vector2(-12.6f, 6.6f), new Vector2(4.7f, 1.4f));

            ReplacePlayerArt(player.gameObject, survivor);
            foreach (var zombie in UnityEngine.Object.FindObjectsByType<ZombieChase>(FindObjectsSortMode.None))
                ReplaceZombieArt(zombie.gameObject, zombieArt);

            Camera.main.orthographicSize = 7.7f;
            Camera.main.GetComponent<CameraFollow>().ConfigureBounds(new Vector2(-1.7f, 0.8f),
                new Vector2(1.7f, 0.8f));
            Camera.main.transform.position = new Vector3(-1.7f, 0.8f, -10);
            Camera.main.backgroundColor = new Color(0.12f, 0.14f, 0.11f);
            UnityEngine.Object.FindFirstObjectByType<PrototypeOverlay>().ConfigureStreet();
            const string destination = "Assets/Scenes/05_PikselSokak.unity";
            EditorSceneManager.SaveScene(scene, destination);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(destination, true) };
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player.gameObject;
            Debug.Log("Piksel sokak hazır. Hareket, atış, saldırı ve yeniden başlama korunur.");
        }

        private static Sprite ImportSprite(string fileName, float worldHeight)
        {
            string path = ArtRoot + fileName;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Görsel bulunamadı: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 4096;
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            importer.spritePixelsPerUnit = height / worldHeight;
            importer.SaveAndReimport();
            var result = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (result == null) throw new InvalidOperationException("Sprite yüklenemedi: " + path);
            return result;
        }

        private static void ReplacePlayerArt(GameObject player, Sprite sprite)
        {
            foreach (var name in new[] { "Oyuncu", "Bas" })
            {
                var old = player.transform.Find(name);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            var renderer = NewVisual(player, sprite);
            player.AddComponent<PixelActorPresentation>().Configure(renderer, true);
            var gun = player.transform.Find("Silah yonu/Tabanca");
            if (gun != null)
            {
                gun.localScale = new Vector3(0.42f, 0.12f, 1);
                gun.GetComponent<SpriteRenderer>().sortingOrder = 400;
                gun.GetComponent<SpriteRenderer>().color = new Color(0.65f, 0.62f, 0.48f);
            }
        }

        private static void ReplaceZombieArt(GameObject zombie, Sprite sprite)
        {
            foreach (var name in new[] { "Bas", "Sol goz", "Sag goz" })
            {
                var old = zombie.transform.Find(name);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            zombie.GetComponent<SpriteRenderer>().enabled = false;
            var renderer = NewVisual(zombie, sprite);
            zombie.GetComponent<TargetFeedback>().ConfigureRenderer(renderer);
            zombie.AddComponent<PixelActorPresentation>().Configure(renderer, false);
        }

        private static SpriteRenderer NewVisual(GameObject parent, Sprite sprite)
        {
            var visual = new GameObject("Piksel Gorunum");
            visual.transform.SetParent(parent.transform, false);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 100;
            return renderer;
        }

        private static void Solid(string name, Vector2 position, Vector2 size)
        {
            var solid = new GameObject(name);
            solid.transform.position = position;
            solid.AddComponent<BoxCollider2D>().size = size;
        }
    }
}
