using System;
using System.IO;
using Cernobil.Companion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cernobil.Editor
{
    public static class ThirteenthHudFix
    {
        [MenuItem("Cernobil/Adim 13 - Bos Paneli Kaldir")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { EditorUtility.DisplayDialog("Önce oyunu durdur", "Önce Stop düğmesine bas.", "Tamam"); return; }
            const string path = "Assets/Scenes/14_FinalBolum2.unity";
            if (!File.Exists(path) || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string original = SceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.OpenScene(path);
            int removed = 0;
            foreach (var item in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (item == null) continue;
                if (item.name == "Metal servis dolabi" || item.name.StartsWith("Servis pompa blogu", StringComparison.Ordinal))
                { UnityEngine.Object.DestroyImmediate(item.gameObject); removed++; }
            }
            var panels = UnityEngine.Object.FindObjectsByType<GateControlPanel>(FindObjectsSortMode.None);
            if (panels.Length != 1) throw new InvalidOperationException("Kapıyı açan gerçek kontrol paneli korunmalı.");
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            if (!string.IsNullOrEmpty(original) && original != path) EditorSceneManager.OpenScene(original);
            Debug.Log("Boş panel ve ona ait engel kaldırıldı. Tek işlevsel robot kontrol paneli korundu. Kaldırılan nesne: " + removed);
        }
    }
}
