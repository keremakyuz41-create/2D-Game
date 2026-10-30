using System;
using System.IO;
using Cernobil.Combat;
using Cernobil.Player;
using Cernobil.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cernobil.Editor
{
    public static class SecondStepSetup
    {
        [MenuItem("Cernobil/Adim 2 - Atis Sahnesini Olustur")]
        public static void CreateScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Oyun modunu durdur", "Önce üstteki Stop düğmesine bas.", "Tamam");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string firstScene = "Assets/Scenes/01_HareketDenemesi.unity";
            if (!File.Exists(firstScene))
            {
                EditorUtility.DisplayDialog("İlk sahne gerekli", "Önce Cernobil menüsünden Adim 1 sahnesini oluştur.", "Tamam");
                return;
            }
            var tile = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/White.png");
            if (tile == null) throw new InvalidOperationException("Önce Adim 1 sahnesini yeniden oluştur.");
            var scene = EditorSceneManager.OpenScene(firstScene);
            const string scenePath = "Assets/Scenes/02_AtisDenemesi.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            if (player == null) throw new InvalidOperationException("Player bulunamadı.");
            player.transform.position = new Vector3(-5, -2, 0);
            var pivot = new GameObject("Silah yonu").transform;
            pivot.SetParent(player.transform, false);
            var gun = new GameObject("Tabanca");
            gun.transform.SetParent(pivot, false);
            gun.transform.localPosition = new Vector3(0.5f, 0, 0);
            gun.transform.localScale = new Vector3(0.55f, 0.14f, 1);
            var gunVisual = gun.AddComponent<SpriteRenderer>();
            gunVisual.sprite = tile;
            gunVisual.color = new Color(0.78f, 0.77f, 0.62f);
            gunVisual.sortingOrder = 12;
            var weapon = player.gameObject.AddComponent<PlayerWeapon>();
            weapon.SetGunPivot(pivot);

            Target(tile, "Hedef 1", new Vector2(-1, -2));
            Target(tile, "Hedef 2", new Vector2(2, -4));
            Target(tile, "Hedef 3 - bariyerin arkasinda", new Vector2(7, -2));
            UnityEngine.Object.FindFirstObjectByType<PrototypeOverlay>().ConfigureCombat(weapon);
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player.gameObject;
            Debug.Log("Adim 2 hazır: 3 hedef, her hedef 3 isabette yok olur. R ile doldur.");
        }

        private static void Target(Sprite tile, string name, Vector2 position)
        {
            var target = new GameObject(name);
            target.transform.position = position;
            var visual = target.AddComponent<SpriteRenderer>();
            visual.sprite = tile;
            visual.color = new Color(0.9f, 0.4f, 0.15f);
            visual.sortingOrder = 10;
            var collider = target.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.85f, 0.85f);
            target.AddComponent<Health>();
            target.AddComponent<TargetFeedback>();
        }
    }
}
