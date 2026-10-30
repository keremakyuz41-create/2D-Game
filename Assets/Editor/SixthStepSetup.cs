using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Cernobil.Combat;
using Cernobil.Enemies;
using Cernobil.Player;
using Cernobil.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cernobil.Editor
{
    public static class SixthStepSetup
    {
        private const string ArtRoot = "Assets/Art/Street/";

        [MenuItem("Cernobil/Adim 6 - Buyuk Karakter Kosu ve Silah")]
        public static void CreateScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Oyun modunu durdur", "Önce üstteki Stop düğmesine bas.", "Tamam");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string source = "Assets/Scenes/05_PikselSokak.unity";
            if (!File.Exists(source))
            {
                EditorUtility.DisplayDialog("Önce Adım 5", "Önce piksel sokak sahnesini oluştur.", "Tamam");
                return;
            }
            Sprite[] runFrames = ImportRunSheet();
            Sprite pistol = ImportSingle("Pistol.png", 0.95f, new Vector2(0.28f, 0.35f));
            Sprite flash = CreateFlash();
            var scene = EditorSceneManager.OpenScene(source);
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            if (player == null) throw new InvalidOperationException("Oyuncu bulunamadı.");
            player.EnableSprint();
            var presentation = player.GetComponent<PixelActorPresentation>();
            presentation.ConfigureAnimation(runFrames);
            var playerVisual = player.transform.Find("Piksel Gorunum");
            playerVisual.localScale = Vector3.one * 1.8f;
            playerVisual.GetComponent<SpriteRenderer>().sprite = runFrames[0];
            player.GetComponent<BoxCollider2D>().size = new Vector2(0.9f, 1.4f);

            foreach (var zombie in UnityEngine.Object.FindObjectsByType<ZombieChase>(FindObjectsSortMode.None))
            {
                zombie.transform.Find("Piksel Gorunum").localScale = Vector3.one * 1.7f;
                zombie.GetComponent<BoxCollider2D>().size = new Vector2(0.9f, 1.4f);
                zombie.SetStopDistance(1.45f);
                zombie.GetComponent<ZombieAttack>().SetRange(1.75f);
            }

            var pivot = player.transform.Find("Silah yonu");
            var gun = pivot.Find("Tabanca");
            gun.localPosition = Vector3.zero;
            gun.localScale = Vector3.one;
            var gunVisual = gun.GetComponent<SpriteRenderer>();
            gunVisual.sprite = pistol;
            gunVisual.color = Color.white;
            gunVisual.sortingOrder = 400;

            var muzzle = new GameObject("Namlu ucu").transform;
            muzzle.SetParent(pivot, false);
            muzzle.localPosition = new Vector3(0.65f, 0.28f, 0);
            var flare = new GameObject("Namlu parlamasi");
            flare.transform.SetParent(muzzle, false);
            flare.transform.localScale = Vector3.one * 0.38f;
            var flashVisual = flare.AddComponent<SpriteRenderer>();
            flashVisual.sprite = flash;
            flashVisual.sortingOrder = 450;
            flashVisual.enabled = false;
            player.GetComponent<PlayerWeapon>().ConfigureHandheld(gunVisual, muzzle, flashVisual);

            Camera.main.orthographicSize = 6.8f;
            Camera.main.GetComponent<CameraFollow>().ConfigureBounds(new Vector2(-3.2f, -2.5f),
                new Vector2(3.2f, 2.5f));
            Camera.main.transform.position = new Vector3(-3.2f, -2, -10);
            UnityEngine.Object.FindFirstObjectByType<PrototypeOverlay>().ConfigureRunning();
            const string destination = "Assets/Scenes/06_KosuVeSilah.unity";
            EditorSceneManager.SaveScene(scene, destination);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(destination, true) };
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player.gameObject;
            Debug.Log("Adim 6 hazır: büyük karakterler, dört kare animasyon, Shift ile koşma ve elde tabanca.");
        }

        [MenuItem("Cernobil/Karakter Kaybolmasini Duzelt")]
        public static void RepairAnimation()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Oyun modunu durdur", "Önce üstteki Stop düğmesine bas.", "Tamam");
                return;
            }
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            var presentation = player != null ? player.GetComponent<PixelActorPresentation>() : null;
            if (presentation == null)
            {
                EditorUtility.DisplayDialog("Sahne gerekli", "Önce 06_KosuVeSilah sahnesini aç.", "Tamam");
                return;
            }
            var frames = ImportRunSheet();
            presentation.RepairAnimation(frames);
            EditorUtility.SetDirty(presentation);
            EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
            EditorSceneManager.SaveScene(player.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Animasyon onarıldı: dört kalıcı ve farklı sprite kaydı doğrulandı.");
        }

        private static Sprite[] ImportRunSheet()
        {
            const string path = ArtRoot + "SurvivorRun.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Koşu görseli bulunamadı.");
            // Native Sprite assets have independent persistent IDs. The legacy multi-sprite
            // metadata API can assign the same fileID to every frame in this Unity version.
            importer.textureType = TextureImporterType.Default;
            importer.spriteImportMode = SpriteImportMode.None;
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            // The authored atlas has a 40px top margin at 1254px, with the row gap at 647px.
            // Slice inside the transparent row gap so no boot is cut at the mathematical midpoint.
            float cellHeight = Mathf.Round(height * (607f / 1254f));
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            const string folder = ArtRoot + "RunFrames";
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            var frames = new Sprite[4];
            float cellWidth = width / 2f;
            for (int i = 0; i < 4; i++)
            {
                var rect = new Rect((i % 2) * cellWidth, i < 2 ? cellHeight : 0, cellWidth, cellHeight);
                var pivot = new Vector2(i % 2 == 0 ? 0.56f : 0.44f, 0.5f);
                var sprite = Sprite.Create(texture, rect, pivot, cellHeight / 1.45f,
                    0, SpriteMeshType.FullRect, Vector4.zero, false);
                sprite.name = "Run_" + i;
                string framePath = folder + "/Run_" + i + ".asset";
                var existing = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
                if (existing == null) AssetDatabase.CreateAsset(sprite, framePath);
                else
                {
                    EditorUtility.CopySerialized(sprite, existing);
                    EditorUtility.SetDirty(existing);
                    UnityEngine.Object.DestroyImmediate(sprite);
                }
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
            }
            AssetDatabase.SaveAssets();
            var identities = new HashSet<string>();
            foreach (var frame in frames)
            {
                if (frame == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(frame, out string guid, out long id) ||
                    id == 0 || !identities.Add(guid + ":" + id))
                    throw new InvalidOperationException("Koşu karesinin kalıcı ve benzersiz kaydı oluşturulamadı.");
            }
            return frames;
        }

        private static Sprite ImportSingle(string fileName, float worldWidth, Vector2 pivot)
        {
            string path = ArtRoot + fileName;
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
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = 9;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateFlash()
        {
            const string path = ArtRoot + "MuzzleFlash.png";
            var texture = new Texture2D(16, 16);
            var pixels = new Color[256];
            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float dx = Mathf.Abs(x - 7.5f);
                float dy = Mathf.Abs(y - 7.5f);
                if ((dx < 2 && dy < 7) || (dy < 2 && dx < 7) || dx + dy < 6)
                    pixels[y * 16 + x] = dx + dy < 4 ? new Color(1, 1, 0.7f) : new Color(1, 0.65f, 0.12f);
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            return ImportSingle("MuzzleFlash.png", 1, new Vector2(0.5f, 0.5f));
        }
    }
}
