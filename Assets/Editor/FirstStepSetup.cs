using System;
using System.IO;
using Cernobil.Player;
using Cernobil.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cernobil.Editor
{
    public static class FirstStepSetup
    {
        private const string ScenePath = "Assets/Scenes/01_HareketDenemesi.unity";
        private static Sprite tile;

        [MenuItem("Cernobil/Adim 1 - Deneme Sahnesini Olustur")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            GenerateScene();
        }

        public static void CreateForBatch()
        {
            GenerateScene();
            ValidateScene();
            Debug.Log("CERNOBIL_STEP_1_OK: Scene created and configuration validated.");
        }

        private static void GenerateScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Art/Prototype");
            CreateTile();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PlayerSettings.companyName = "YazLab";
            PlayerSettings.productName = "Cernobil - Son Tahliye";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(
                "ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input != null) { input.intValue = 0; settings.ApplyModifiedPropertiesWithoutUndo(); }

            var floor = new GameObject("Zemin").transform;
            for (int x = -15; x <= 15; x++)
            for (int y = -10; y <= 10; y++)
            {
                float shade = ((x + y) & 1) == 0 ? 0.19f : 0.21f;
                Color color = Mathf.Abs(y) < 4
                    ? new Color(shade, shade + 0.015f, shade + 0.005f)
                    : new Color(shade + 0.07f, shade + 0.08f, shade + 0.045f);
                var ground = Draw("Zemin tasi", new Vector2(x, y), Vector2.one, color, -10);
                ground.transform.SetParent(floor);
            }
            for (int x = -14; x <= 14; x += 3)
                Draw("Yol cizgisi", new Vector2(x, 0), new Vector2(1.1f, 0.09f),
                    new Color(0.58f, 0.53f, 0.32f), -9);

            Wall("Kuzey siniri", new Vector2(0, 10), new Vector2(31, 1));
            Wall("Guney siniri", new Vector2(0, -10), new Vector2(31, 1));
            Wall("Bati siniri", new Vector2(-15, 0), new Vector2(1, 20));
            Wall("Dogu siniri", new Vector2(15, 0), new Vector2(1, 20));
            Wall("Beton bariyer 1", new Vector2(0, 2), new Vector2(4, 0.8f));
            Wall("Beton bariyer 2", new Vector2(5, -2), new Vector2(0.8f, 3));
            Wall("Harabe bina", new Vector2(-6, 6), new Vector2(7, 4));
            Draw("Bina kapisi", new Vector2(-6, 4.03f), new Vector2(0.8f, 0.25f),
                new Color(0.12f, 0.13f, 0.1f), 1);

            var player = Draw("Oyuncu", new Vector2(-5, -2), new Vector2(0.65f, 0.85f),
                new Color(0.44f, 0.53f, 0.25f), 10);
            // Keep the physics root at unit scale; child artwork defines visible size.
            var root = new GameObject("Player");
            root.transform.position = player.transform.position;
            player.transform.SetParent(root.transform, true);
            var collider = root.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.6f, 0.75f);
            var body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 0;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            root.AddComponent<KeyboardMovementInput>();
            root.AddComponent<PlayerMovement>();
            var head = Draw("Bas", (Vector2)root.transform.position + new Vector2(0, 0.2f),
                new Vector2(0.32f, 0.32f), new Color(0.73f, 0.58f, 0.4f), 11);
            head.transform.SetParent(root.transform, true);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(-4, -2, -10);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6;
            camera.backgroundColor = new Color(0.08f, 0.1f, 0.08f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<CameraFollow>().SetTarget(root.transform);
            new GameObject("Kontroller").AddComponent<PrototypeOverlay>();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
        }

        private static void CreateTile()
        {
            const string path = "Assets/Art/Prototype/White.png";
            var texture = new Texture2D(8, 8);
            var colors = new Color[64];
            Array.Fill(colors, Color.white);
            texture.SetPixels(colors);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 8;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            tile = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (tile == null)
                throw new InvalidOperationException("White.png could not be imported as a single sprite. Scene creation stopped.");
        }

        private static GameObject Draw(string name, Vector2 position, Vector2 scale,
            Color color, int order)
        {
            var item = new GameObject(name);
            item.transform.position = position;
            item.transform.localScale = new Vector3(scale.x, scale.y, 1);
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = tile;
            renderer.color = color;
            renderer.sortingOrder = order;
            return item;
        }

        private static void Wall(string name, Vector2 position, Vector2 scale)
        {
            var wall = Draw(name, position, scale, new Color(0.36f, 0.38f, 0.31f), 0);
            wall.AddComponent<BoxCollider2D>();
        }

        private static void ValidateScene()
        {
            var renderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            if (renderers.Length == 0) throw new Exception("Scene has no visible sprite objects.");
            foreach (var renderer in renderers)
                if (renderer.sprite == null)
                    throw new Exception("Missing sprite on scene object: " + renderer.name);
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            if (player == null || player.GetComponent<Rigidbody2D>().gravityScale != 0)
                throw new Exception("Player or top-down physics configuration is invalid.");
            if (Camera.main == null || !Camera.main.orthographic)
                throw new Exception("Orthographic camera missing.");
            if (UnityEngine.Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None).Length < 8)
                throw new Exception("Collision geometry missing.");
            if (!File.Exists(ScenePath)) throw new Exception("Scene was not saved.");
        }
    }
}
