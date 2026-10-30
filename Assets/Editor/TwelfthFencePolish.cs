using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cernobil.Editor
{
    public static class TwelfthFencePolish
    {
        private const string ScenePath = "Assets/Scenes/14_FinalBolum2.unity";
        [MenuItem("Cernobil/Adim 12 - Santral Tel Cit ve Servis Gecidi")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { EditorUtility.DisplayDialog("Önce oyunu durdur", "Önce üstteki Stop düğmesine bas.", "Tamam"); return; }
            if (!File.Exists(ScenePath)) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string original = SceneManager.GetActiveScene().path;
            Sprite fence = ImportFence();
            var white = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/White.png");
            var scene = EditorSceneManager.OpenScene(ScenePath);
            int collidersBefore = UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            // Hide only the placeholder renderers. Keep every collider/navigation marker active.
            foreach (var visual in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                string name = visual.gameObject.name;
                if (name.StartsWith("Santral alt duvar", StringComparison.Ordinal) ||
                    name.StartsWith("Santral orta duvar", StringComparison.Ordinal) ||
                    name.StartsWith("Santral ust duvar", StringComparison.Ordinal) ||
                    name.StartsWith("Servis pompa blogu", StringComparison.Ordinal) ||
                    name == "Robot servis gecidi" || name == "Servis zemin isareti") visual.enabled = false;
            }
            var root = GameObject.Find("Santral Tel Cit Gorunumu");
            if (root == null) root = new GameObject("Santral Tel Cit Gorunumu");
            Fence(root.transform, fence, white, "Alt tel cit", -8, -2.5f);
            Fence(root.transform, fence, white, "Orta tel cit", 0.5f, 4.4f);
            Fence(root.transform, fence, white, "Ust tel cit", 5.4f, 7.2f);
            var dark = new Color(0.13f, 0.15f, 0.15f);
            var steel = new Color(0.34f, 0.35f, 0.33f);
            // An open, robot-sized maintenance slot replaces the cyan debug strip.
            Draw(root.transform, white, "Servis aciklik sol kasa", new Vector2(9.03f, 4.9f), new Vector2(0.10f, 1.05f), dark, 105);
            Draw(root.transform, white, "Servis aciklik sag kasa", new Vector2(9.97f, 4.9f), new Vector2(0.10f, 1.05f), dark, 105);
            Draw(root.transform, white, "Servis aciklik alt esik", new Vector2(9.5f, 4.43f), new Vector2(1.04f, 0.08f), steel, 108);
            Draw(root.transform, white, "Servis aciklik ust esik", new Vector2(9.5f, 5.37f), new Vector2(1.04f, 0.08f), steel, 108);
            for (int i = 0; i < 3; i++)
                Draw(root.transform, white, "Servis zemini metal iz " + i, new Vector2(9.5f, 4.63f + i * 0.26f),
                    new Vector2(0.58f, 0.025f), new Color(0.35f, 0.37f, 0.34f, 0.65f), -700);
            // The functional robot control panel is the only panel in the final level.

            int collidersAfter = UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            if (collidersBefore != collidersAfter) throw new InvalidOperationException("Görsel düzenleme çarpışma nesnesi eklememeli.");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            if (!string.IsNullOrEmpty(original) && original != ScenePath) EditorSceneManager.OpenScene(original);
            Debug.Log("Adım 12 kaydedildi: yeşil bloklar tel çit ve metal servis dolabına dönüştü. Robot geçidi ve " + collidersAfter + " çarpışma nesnesi korundu.");
        }

        private static Sprite ImportFence()
        {
            const string png = "Assets/Art/Companion/SantralWireFence.png";
            AssetDatabase.ImportAsset(png, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(png) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Tel çit görseli bulunamadı.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
            const string path = "Assets/Art/Companion/SantralWireFenceTile.asset";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            sprite = Sprite.Create(AssetDatabase.LoadAssetAtPath<Texture2D>(png), new Rect(197, 4, 400, 2037),
                new Vector2(0.5f, 0.5f), 400 / 0.9f, 0, SpriteMeshType.FullRect);
            sprite.name = "Santral Wire Fence Tile";
            AssetDatabase.CreateAsset(sprite, path);
            return sprite;
        }

        private static void Fence(Transform root, Sprite fence, Sprite white, string name, float bottom, float top)
        {
            float length = top - bottom;
            var renderer = Draw(root, fence, name, new Vector2(9.5f, (bottom + top) / 2), new Vector2(0.9f, length), Color.white, 90);
            renderer.transform.localScale = Vector3.one;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(0.9f, length);
            // Small steel anchor plates make the fence terminate cleanly against the gate casing.
            foreach (float y in new[] { bottom + 0.05f, top - 0.05f })
            {
                Draw(root, white, name + " ankraj " + y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    new Vector2(9.5f, y), new Vector2(1.0f, 0.10f), new Color(0.23f, 0.25f, 0.24f), 103);
            }
        }

        private static SpriteRenderer Draw(Transform root, Sprite sprite, string name, Vector2 position, Vector2 size, Color color, int order)
        {
            var item = root.Find(name);
            if (item == null) { item = new GameObject(name).transform; item.SetParent(root, false); }
            item.position = new Vector3(position.x, position.y, 0);
            item.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1);
            var renderer = item.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = item.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order; renderer.enabled = true;
            return renderer;
        }
    }
}
