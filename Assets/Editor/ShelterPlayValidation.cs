using System;
using System.IO;
using Cernobil.Dialogue;
using Cernobil.Player;
using Cernobil.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cernobil.Editor
{
    // This test uses the real local model and real door/trigger; it never fabricates an NPC reply.
    [InitializeOnLoad]
    public static class ShelterPlayValidation
    {
        private const string Armed = "Cernobil.ShelterLiveValidation";
        private const string Question = "Yaralandım, sığınmaya ihtiyacım var. Size yardım da edebilirim. Beni içeri alabilir misin?";
        private static NpcDialogue npc;
        private static ShelterGate gate;
        private static Rigidbody2D body;
        private static double started, phaseStarted;
        private static int phase;
        [Serializable] private sealed class Result
        {
            public string status;
            public string detail;
            public string question;
            public string reply;
            public double seconds;
            public bool liveModel = true;
            public bool closedGateRejectedExit;
            public bool unauthorizedActorRejected;
        }
        private static Result result;
        static ShelterPlayValidation()
        {
            EditorApplication.playModeStateChanged += OnMode;
            EditorApplication.update += Tick;
        }

        [MenuItem("Cernobil/Final - Canli Siginak Testi")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists("Assets/Scenes/15_SonTahliye.unity")) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/Scenes/15_SonTahliye.unity");
            SessionState.SetBool(Armed, true);
            EditorApplication.isPlaying = true;
        }

        private static void OnMode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Armed, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                started = phaseStarted = EditorApplication.timeSinceStartup;
                result = new Result { question = Question };
                phase = 1;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode && phase != 0) Finish(false, "Oynatma testi tamamlanmadan durduruldu.");
        }

        private static void Tick()
        {
            if (phase == 0 || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            try
            {
                double now = EditorApplication.timeSinceStartup;
                if (now - started > 100) { Finish(false, "Canlı test zaman aşımına uğradı."); return; }
                if (phase == 1 && now - phaseStarted > 1)
                {
                    npc = UnityEngine.Object.FindFirstObjectByType<NpcDialogue>();
                    gate = UnityEngine.Object.FindFirstObjectByType<ShelterGate>();
                    var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
                    if (npc == null || gate == null || player == null) throw new InvalidOperationException("Sahne bileşenleri bulunamadı.");
                    body = player.GetComponent<Rigidbody2D>();
                    gate.TryFinish(player.GetComponent<Collider2D>());
                    result.closedGateRejectedExit = !gate.Completed && !gate.PassageOpen;
                    result.unauthorizedActorRejected = !gate.TryAuthorize(null);
                    if (!result.closedGateRejectedExit || !result.unauthorizedActorRejected) throw new InvalidOperationException("Kilit/yetki kontrolü başarısız.");
                    body.position = new Vector2(0, 1.1f);
                    Physics2D.SyncTransforms();
                    phase = 2; phaseStarted = now;
                }
                else if (phase == 2 && now - phaseStarted > 0.5)
                {
                    if (!npc.OpenConversation() || !npc.SubmitText(Question)) throw new InvalidOperationException("Serbest metin gönderilemedi.");
                    phase = 3; phaseStarted = now;
                }
                else if (phase == 3 && !npc.IsBusy)
                {
                    result.reply = npc.LastReply;
                    if (!gate.Authorized) { Finish(false, npc.LastError ?? "Gerçek model bu cümlede giriş izni vermedi."); return; }
                    if (!gate.PassageOpen) return;
                    npc.Dismiss();
                    body.position = new Vector2(0, 6.2f);
                    Physics2D.SyncTransforms();
                    phase = 4; phaseStarted = now;
                }
                else if (phase == 4 && now - phaseStarted > 1)
                    Finish(gate.Completed, gate.Completed ? "Gerçek model izni, kapı animasyonu ve sığınak çıkış tetikleyicisi çalıştı." : "İzin sonrası çıkış tetiklenmedi.");
            }
            catch (Exception ex) { Finish(false, ex.Message); }
        }

        private static void Finish(bool success, string detail)
        {
            if (result == null) result = new Result();
            result.status = success ? "passed" : "failed";
            result.detail = detail;
            result.seconds = EditorApplication.timeSinceStartup - started;
            File.WriteAllText("Siginak-Canli-Test-Sonuclari.json", JsonUtility.ToJson(result, true));
            phase = 0;
            SessionState.SetBool(Armed, false);
            if (success) Debug.Log("Sığınak canlı testi geçti: " + detail);
            else Debug.LogWarning("Sığınak canlı testi tamamlanamadı: " + detail);
        }
    }
}
