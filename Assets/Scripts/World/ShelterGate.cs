using System.Collections;
using Cernobil.Combat;
using Cernobil.Dialogue;
using Cernobil.Player;
using Cernobil.UI;
using UnityEngine;

namespace Cernobil.World
{
    public sealed class ShelterGate : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D barrier;
        [SerializeField] private SpriteRenderer shutter;
        [SerializeField] private NpcDialogue guard;
        [SerializeField] private Health playerHealth;
        public bool Authorized { get; private set; }
        public bool PassageOpen { get; private set; }
        public bool Completed { get; private set; }

        public void Configure(BoxCollider2D collider, SpriteRenderer door, NpcDialogue npc, Health health)
        { barrier = collider; shutter = door; guard = npc; playerHealth = health; }

        public bool TryAuthorize(NpcDialogue actor)
        {
            if (actor == null || actor != guard || !actor.WithinRange || Authorized ||
                playerHealth == null || !playerHealth.IsAlive) return false;
            Authorized = true;
            StartCoroutine(Open());
            return true;
        }

        private IEnumerator Open()
        {
            Vector3 size = shutter.transform.localScale;
            Vector3 position = shutter.transform.localPosition;
            float elapsed = 0;
            while (elapsed < 0.85f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / 0.85f);
                shutter.transform.localScale = new Vector3(size.x * (1 - t * 0.97f), size.y, size.z);
                shutter.transform.localPosition = position + Vector3.left * (t * 1.9f);
                yield return null;
            }
            shutter.enabled = false;
            barrier.enabled = false;
            PassageOpen = true;
        }

        public void TryFinish(Collider2D other)
        {
            if (!PassageOpen || Completed || NpcDialogue.BlocksPlayerInput || !playerHealth.IsAlive ||
                other.GetComponentInParent<Health>() != playerHealth) return;
            Completed = true;
            playerHealth.GetComponent<PlayerMovement>().enabled = false;
            playerHealth.GetComponent<PlayerWeapon>().enabled = false;
        }

        private void OnGUI()
        {
            if (!Completed) return;
            Matrix4x4 previous = GUI.matrix;
            int depth = GUI.depth;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            if (scale <= 0) return;
            try
            {
                GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
                GUI.depth = -200;
                float w = Screen.width / scale, h = Screen.height / scale;
                HudTheme.Fill(new Rect(0, 0, w, h), new Color(0.015f, 0.025f, 0.025f, 0.95f));
                HudTheme.Card(new Rect(w / 2 - 270, h / 2 - 150, 540, 300));
                HudTheme.Label(new Rect(w / 2 - 240, h / 2 - 112, 480, 32), "SON TAHLİYE", 16, HudTheme.Amber, true, TextAnchor.MiddleCenter);
                HudTheme.Label(new Rect(w / 2 - 240, h / 2 - 62, 480, 50), "SIĞINAĞA ULAŞTIN", 30, HudTheme.Mint, true, TextAnchor.MiddleCenter);
                HudTheme.Label(new Rect(w / 2 - 230, h / 2, 460, 60), "Görevli girişini onayladı.\nÇernobil'in sokakları artık geride kaldı.", 18, HudTheme.Text, false, TextAnchor.MiddleCenter, true);
                if (HudTheme.Button(new Rect(w / 2 - 130, h / 2 + 88, 260, 40), "BAŞTAN OYNA"))
                    FindFirstObjectByType<SurvivalSession>().RestartGame();
            }
            finally { GUI.matrix = previous; GUI.depth = depth; }
        }
    }
}
