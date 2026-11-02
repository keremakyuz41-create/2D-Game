using System;
using System.Collections.Generic;
using System.IO;
using Cernobil.Combat;
using Cernobil.Companion;
using Cernobil.Enemies;
using Cernobil.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cernobil.Editor
{
    [InitializeOnLoad]
    public static class CompanionPlayValidation
    {
        [Serializable] private class Case
        {
            public Vector2 start;
            public bool issuedCommand;
            public bool usedServiceGap;
            public bool returnedToPlayer;
            public int movementSamples;
            public float commandSeconds;
            public float totalSeconds;
        }
        [Serializable] private class Results
        {
            public string status;
            public string timestampUtc;
            public string error;
            public List<Case> cases = new List<Case>();
        }
        private const string RunningKey = "Cernobil.Robot.LiveValidation";
        private const string OriginalKey = "Cernobil.Robot.ValidationOriginal";
        private static Results results;
        private static Case current;
        private static CompanionRobot robot;
        private static SantralGate gate;
        private static PlayerMovement player;
        private static Vector3 previous;
        private static double started;
        private static int previousRobotId;
        private static bool waitingForReload;
        private static readonly Vector2[] Starts = { new Vector2(-12.2f, -3.2f), new Vector2(-4, 0),
            new Vector2(6, -2), new Vector2(7, 3) };

        static CompanionPlayValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    string original = SessionState.GetString(OriginalKey, "");
                    SessionState.EraseString(OriginalKey);
                    if (!string.IsNullOrEmpty(original) && File.Exists(original)) EditorSceneManager.OpenScene(original);
                }
            };
        }

        [MenuItem("Cernobil/Robot - Canli Gorev Testlerini Calistir")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Önce Play modunu durdur.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            results = null;
            current = null;
            robot = null;
            waitingForReload = false;
            previousRobotId = 0;
            SessionState.SetString(OriginalKey, EditorSceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/10_RobotBolum2.unity");
            SessionState.SetBool(RunningKey, true);
            EditorApplication.EnterPlaymode();
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || !SessionState.GetBool(RunningKey, false)) return;
            try
            {
                if (results == null)
                {
                    results = new Results { timestampUtc = DateTime.UtcNow.ToString("O") };
                    started = EditorApplication.timeSinceStartup;
                }
                if (current == null)
                {
                    robot = UnityEngine.Object.FindFirstObjectByType<CompanionRobot>();
                    if (robot == null || (waitingForReload && robot.GetInstanceID() == previousRobotId)) return;
                    player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
                    gate = UnityEngine.Object.FindFirstObjectByType<SantralGate>();
                    if (player == null || gate == null) return;
                    player.enabled = false;
                    player.GetComponent<PlayerWeapon>().enabled = false;
                    player.transform.position = new Vector3(7, -1, 0);
                    foreach (var chase in UnityEngine.Object.FindObjectsByType<ZombieChase>(FindObjectsSortMode.None)) chase.enabled = false;
                    foreach (var attack in UnityEngine.Object.FindObjectsByType<ZombieAttack>(FindObjectsSortMode.None)) attack.enabled = false;
                    if (!robot.SetValidationStart(Starts[results.cases.Count])) return;
                    waitingForReload = false;
                    current = new Case { start = Starts[results.cases.Count] };
                    started = EditorApplication.timeSinceStartup;
                    previous = robot.transform.position;
                }
                if (EditorApplication.timeSinceStartup - started > 45) throw new Exception("Robot live task exceeded 45 seconds: " + robot.Status);
                Vector3 position = robot.transform.position;
                if (Vector3.Distance(previous, position) > 0.001f) current.movementSamples++;
                if (previous.x < 9.5f && position.x >= 9.5f && !gate.IsOpen)
                {
                    float t = (9.5f - previous.x) / Mathf.Max(0.0001f, position.x - previous.x);
                    float y = Mathf.Lerp(previous.y, position.y, t);
                    current.usedServiceGap = y >= 4.4f && y <= 5.4f;
                    if (!current.usedServiceGap) throw new Exception("Robot crossed the closed barrier outside the service gap.");
                }
                previous = position;
                if (!current.issuedCommand && robot.CanCommand) current.issuedCommand = robot.CommandOpenGate();
                if (robot.State == RobotState.Failed) throw new Exception(robot.Error);
                if (!gate.PassageOpen || robot.SuccessfulCommands == 0 || robot.Busy ||
                    Vector2.Distance(position, player.transform.position) > 2.3f) return;
                current.returnedToPlayer = true;
                current.commandSeconds = robot.LastCompletionSeconds;
                current.totalSeconds = (float)(EditorApplication.timeSinceStartup - started);
                if (!current.usedServiceGap || !current.issuedCommand || current.movementSamples < 5)
                    throw new Exception("Live navigation did not provide the required movement evidence.");
                results.cases.Add(current);
                Debug.Log("ROBOT_LIVE_CASE_OK: " + results.cases.Count + "/4; " + current.commandSeconds.ToString("F2") + "s");
                current = null;
                if (results.cases.Count == Starts.Length) { Finish("passed", null); return; }
                previousRobotId = robot.GetInstanceID();
                waitingForReload = true;
                SceneManager.LoadScene("10_RobotBolum2");
            }
            catch (Exception error) { Finish("failed", error.ToString()); }
        }

        private static void Finish(string status, string error)
        {
            if (results == null) results = new Results();
            results.status = status;
            results.error = error;
            File.WriteAllText("Robot-Canli-Test-Sonuclari.json", JsonUtility.ToJson(results, true));
            SessionState.SetBool(RunningKey, false);
            if (status == "passed") Debug.Log("ROBOT_LIVE_TESTS_OK: 4/4 completed and returned to player.");
            else Debug.LogError("ROBOT_LIVE_TEST_FAILED: " + error);
            EditorApplication.ExitPlaymode();
        }
    }
}
