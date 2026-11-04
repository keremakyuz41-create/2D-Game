using System;
using System.IO;
using Cernobil.Companion;
using Cernobil.Dialogue;
using Cernobil.Player;
using Cernobil.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Cernobil.Editor
{
    public static class EleventhVisualPolish
    {
        private const string Second = "Assets/Scenes/14_FinalBolum2.unity";
        private const string Third = "Assets/Scenes/15_SonTahliye.unity";
        [MenuItem("Cernobil/Adim 11 - Gercekci Kapi ve Gorevli Boyutu")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { EditorUtility.DisplayDialog("Önce oyunu durdur", "Önce üstteki Stop düğmesine bas.", "Tamam"); return; }
            if (!File.Exists(Second) || !File.Exists(Third))
            { EditorUtility.DisplayDialog("Final sahneleri gerekli", "Önce Adım 10 sahnelerini oluştur.", "Tamam"); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string original = SceneManager.GetActiveScene().path;
            Sprite metal = ImportPanel();
            var second = EditorSceneManager.OpenScene(Second);
            PolishGate(metal);
            EditorSceneManager.MarkSceneDirty(second); EditorSceneManager.SaveScene(second);
            var third = EditorSceneManager.OpenScene(Third);
            ResizeGuard();
            EditorSceneManager.MarkSceneDirty(third); EditorSceneManager.SaveScene(third);
            AssetDatabase.SaveAssets();
            if (!string.IsNullOrEmpty(original) && original != Third) EditorSceneManager.OpenScene(original);
            Debug.Log("Adım 11 uygulandı: ray içine çekilen çelik santral kapısı, sabit kasa, durum lambası ve oyuncuyla aynı görünür boydaki görevli.");
        }

        private static Sprite ImportPanel()
        {
            const string path = "Assets/Art/Companion/SantralSteelGate.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Çelik kapı görseli bulunamadı.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1942 / 3.2f;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
            const string spritePath = "Assets/Art/Companion/SantralSteelPanel.asset";
            var panel = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (panel != null) return panel;
            // Persist the actual panel region without transparent canvas margins.
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            panel = Sprite.Create(texture, new Rect(153, 4, 504, 1934), new Vector2(0.5f, 0.5f), 1934 / 3.1f);
            panel.name = "Santral Steel Sliding Panel";
            AssetDatabase.CreateAsset(panel, spritePath);
            return panel;
        }

        private static void PolishGate(Sprite panel)
        {
            var gate = UnityEngine.Object.FindFirstObjectByType<SantralGate>();
            if (gate == null) throw new InvalidOperationException("Santral kapısı bulunamadı.");
            var data = new SerializedObject(gate);
            var shutter = data.FindProperty("shutter").objectReferenceValue as Transform;
            var collider = data.FindProperty("barrier").objectReferenceValue as BoxCollider2D;
            if (shutter == null || collider == null) throw new InvalidOperationException("Kapı bağlantısı eksik.");
            var renderer = shutter.GetComponent<SpriteRenderer>();
            renderer.sprite = panel;
            renderer.sortingOrder = 190;
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            shutter.localPosition = Vector3.zero;
            shutter.localRotation = Quaternion.identity;
            shutter.localScale = new Vector3(1.05f / panel.bounds.size.x, 3.1f / panel.bounds.size.y, 1);
            var white = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/White.png");
            var root = gate.transform.Find("Celik Kapi Kasasi");
            if (root == null)
            { root = new GameObject("Celik Kapi Kasasi").transform; root.SetParent(gate.transform, false); }
            var maskObject = root.Find("Panel gorunur aciklik");
            if (maskObject == null)
            { maskObject = new GameObject("Panel gorunur aciklik").transform; maskObject.SetParent(root, false); }
            var mask = maskObject.GetComponent<SpriteMask>();
            if (mask == null) mask = maskObject.gameObject.AddComponent<SpriteMask>();
            mask.sprite = white;
            mask.isCustomRangeActive = true;
            mask.backSortingOrder = 185; mask.frontSortingOrder = 195;
            maskObject.localScale = new Vector3(1.08f / white.bounds.size.x, 3.12f / white.bounds.size.y, 1);
            // Rails and end housings stay fixed while the moving panel disappears into its slot.
            Part(root, white, "Sol ray", new Vector2(-0.59f, 0), new Vector2(0.13f, 3.36f), new Color(0.12f, 0.15f, 0.15f));
            Part(root, white, "Sag ray", new Vector2(0.59f, 0), new Vector2(0.13f, 3.36f), new Color(0.12f, 0.15f, 0.15f));
            Part(root, white, "Ust motor kasasi", new Vector2(0, 1.72f), new Vector2(1.38f, 0.30f), new Color(0.22f, 0.26f, 0.24f));
            Part(root, white, "Alt esik", new Vector2(0, -1.65f), new Vector2(1.32f, 0.16f), new Color(0.29f, 0.28f, 0.21f));
            for (int i = 0; i < 5; i++)
            {
                Part(root, white, "Motor uyari " + i, new Vector2(-0.48f + i * 0.24f, 1.73f), new Vector2(0.11f, 0.2f), new Color(0.65f, 0.49f, 0.18f));
                Part(root, white, "Sol civata " + i, new Vector2(-0.59f, -1.35f + i * 0.65f), new Vector2(0.07f, 0.07f), new Color(0.54f, 0.56f, 0.52f));
                Part(root, white, "Sag civata " + i, new Vector2(0.59f, -1.35f + i * 0.65f), new Vector2(0.07f, 0.07f), new Color(0.54f, 0.56f, 0.52f));
            }
            Part(root, white, "Lamba yuvasi", new Vector2(0.86f, 1.35f), new Vector2(0.27f, 0.38f), new Color(0.1f, 0.12f, 0.12f));
            var lamp = Part(root, white, "Durum lambasi", new Vector2(0.86f, 1.35f), new Vector2(0.15f, 0.23f), new Color(0.95f, 0.2f, 0.12f));
            gate.ConfigureAppearance(lamp, 1.15f, 3.25f);
            // The physical barrier, service gap and robot navigation stay at their established dimensions.
            if (Mathf.Abs(collider.size.x - 0.7f) > 0.01f || Mathf.Abs(collider.size.y - 3) > 0.01f)
                throw new InvalidOperationException("Kapının fiziksel açıklığı beklenenden farklı; otomatik değiştirilmedi.");
        }

        private static SpriteRenderer Part(Transform root, Sprite sprite, string name, Vector2 position, Vector2 size, Color color)
        {
            var item = root.Find(name);
            if (item == null) { item = new GameObject(name).transform; item.SetParent(root, false); }
            item.localPosition = new Vector3(position.x, position.y, 0);
            item.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1);
            var renderer = item.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = item.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.color = color;
            renderer.sortingOrder = name == "Durum lambasi" ? 207 : 205;
            return renderer;
        }

        private static void ResizeGuard()
        {
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            var guard = UnityEngine.Object.FindFirstObjectByType<NpcDialogue>();
            if (player == null || guard == null) throw new InvalidOperationException("Oyuncu veya görevli bulunamadı.");
            var presentation = new SerializedObject(player.GetComponent<PixelActorPresentation>());
            var body = presentation.FindProperty("visual").objectReferenceValue as SpriteRenderer;
            var visual = guard.GetComponent<SpriteRenderer>();
            if (body == null || visual == null) throw new InvalidOperationException("Karakter görselleri bulunamadı.");
            float targetHeight = VisibleHeight(body);
            float originalHeight = VisibleHeight(visual);
            visual.transform.localScale *= targetHeight / originalHeight;
            Debug.Log("Görevli görünür boyu " + originalHeight.ToString("F2") + " → " + VisibleHeight(visual).ToString("F2") + " birim. Oyuncu=" + targetHeight.ToString("F2"));
        }

        private static float VisibleHeight(SpriteRenderer renderer)
        {
            var sprite = renderer.sprite;
            var texture = new Texture2D(2, 2);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture)) ))
                    throw new InvalidOperationException("Karakterin alfa alanı okunamadı.");
                var pixels = texture.GetPixels32();
                var rect = sprite.rect;
                int low = texture.height, high = -1;
                for (int y = Mathf.FloorToInt(rect.y); y < Mathf.CeilToInt(rect.yMax); y++)
                for (int x = Mathf.FloorToInt(rect.x); x < Mathf.CeilToInt(rect.xMax); x++)
                    if (pixels[y * texture.width + x].a > 153) { low = Mathf.Min(low, y); high = Mathf.Max(high, y); }
                if (high < low) throw new InvalidOperationException("Karakter görseli boş.");
                return (high - low + 1) / sprite.pixelsPerUnit * Mathf.Abs(renderer.transform.lossyScale.y);
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }
    }
}
