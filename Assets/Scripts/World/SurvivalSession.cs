using Cernobil.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cernobil.World
{
    public sealed class SurvivalSession : MonoBehaviour
    {
        [SerializeField] private Health playerHealth;
        private float flashUntil;
        private bool restarting;
        public Health PlayerHealth => playerHealth;
        public float DamageFlashStrength => Mathf.Clamp01((flashUntil - Time.time) / 0.2f);
        public bool IsGameOver => playerHealth != null && !playerHealth.IsAlive;

        public void SetPlayer(Health value) => playerHealth = value;

        private void OnEnable()
        {
            if (playerHealth != null) playerHealth.Changed += OnHealthChanged;
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.Changed -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int maximum) => flashUntil = Time.time + 0.2f;

        private void Update()
        {
            if (IsGameOver && Input.GetKeyDown(KeyCode.Return)) RestartLevel();
        }

        public void RestartLevel()
        {
            if (restarting) return;
            restarting = true;
            LevelTransfer.Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void RestartGame()
        {
            if (restarting) return;
            if (SceneManager.sceneCountInBuildSettings == 0)
            { Debug.LogError("Başlangıç bölümü Build Settings listesinde bulunamadı."); return; }
            restarting = true;
            LevelTransfer.Clear();
            // The first enabled build scene is the start of the complete game.
            SceneManager.LoadScene(0);
        }

    }
}
