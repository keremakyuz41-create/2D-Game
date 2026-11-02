using System;
using System.Collections.Generic;
using System.IO;
using Cernobil.Companion;
using Cernobil.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Cernobil.Editor
{
    public static class CompanionNavMeshValidation
    {
        [Serializable] private class Results
        {
            public string status;
            public string scene;
            public string timestampUtc;
            public float buildMilliseconds;
            public List<string> passed = new List<string>();
            public List<float> closedPathLengths = new List<float>();
            public float openedPathLength;
            public string error;
        }

        [MenuItem("Cernobil/Robot - NavMesh Yol Testlerini Calistir")]
        public static void ValidateScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Önce Play modunu durdur.");
            var result = new Results { timestampUtc = DateTime.UtcNow.ToString("O") };
            var world = UnityEngine.Object.FindFirstObjectByType<CompanionNavigationWorld>();
            var gate = UnityEngine.Object.FindFirstObjectByType<SantralGate>();
            if (world == null || gate == null) throw new InvalidOperationException("Robotun ikinci bölüm sahnesini aç.");
            result.scene = world.gameObject.scene.name;
            var barrier = gate.GetComponent<BoxCollider2D>();
            bool wasEnabled = barrier.enabled;
            GameObject sensorObject = null;
            try
            {
                barrier.enabled = true;
                Physics2D.SyncTransforms();
                Check(world.BuildInitial(), "Native NavMesh build succeeds", result);
                result.buildMilliseconds = world.LastBuildMilliseconds;
                Vector2 target = new Vector2(12.5f, 0);
                var starts = new[] { new Vector2(-12.2f, -3.2f), new Vector2(-4, 0), new Vector2(6, -2), new Vector2(7, 3) };
                foreach (var start in starts)
                {
                    var path = Path(start, target);
                    result.closedPathLengths.Add(Length(path));
                    bool usesGap = false;
                    var corners = path.corners;
                    for (int i = 1; i < corners.Length; i++)
                    {
                        var a = corners[i - 1];
                        var b = corners[i];
                        if ((a.x <= 9.5f && b.x >= 9.5f) || (b.x <= 9.5f && a.x >= 9.5f))
                        {
                            float t = Mathf.Abs(b.x - a.x) < 0.0001f ? 0 : (9.5f - a.x) / (b.x - a.x);
                            float y = Mathf.Lerp(a.z, b.z, t);
                            if (y >= 4.4f && y <= 5.4f) usesGap = true;
                        }
                    }
                    Check(usesGap, "Complete native path uses service gap from " + start, result);
                }
                Check(NavMesh.Raycast(CompanionNavigationWorld.ToNavigation(new Vector2(7, -1)),
                    CompanionNavigationWorld.ToNavigation(target), out _, NavMesh.AllAreas),
                    "Closed gate blocks direct native navigation", result);

                Vector2 gap = new Vector2(9.5f, 4.9f);
                var player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
                bool playerBlocked = false;
                foreach (var collider in Physics2D.OverlapBoxAll(gap, player.GetComponent<BoxCollider2D>().size, 0))
                    if (collider.GetComponent<NavigationObstacle2D>() != null) playerBlocked = true;
                Check(playerBlocked, "Player cannot fit through the service gap", result);
                bool robotBlocked = false;
                foreach (var collider in Physics2D.OverlapCircleAll(gap, CompanionNavigationWorld.AgentRadius))
                    if (collider.GetComponent<NavigationObstacle2D>() != null) robotBlocked = true;
                Check(!robotBlocked, "Robot fits through the service gap", result);

                sensorObject = new GameObject("Temporary sensor test");
                sensorObject.transform.position = new Vector3(9, -1, 0);
                var sensor = sensorObject.AddComponent<RobotObstacleSensor>();
                sensor.Observe(Vector2.right);
                Check(sensor.NearestDistance > 0.05f && sensor.NearestDistance < 0.3f,
                    "Raycast sensor measures the gate distance", result);
                Check(!UnityEngine.Object.FindFirstObjectByType<GateControlPanel>().TryActivate(null),
                    "Control panel rejects an unauthorized actor", result);

                barrier.enabled = false;
                Check(world.RebuildForValidation(), "Native NavMesh updates when gate opens", result);
                result.openedPathLength = Length(Path(new Vector2(7, -1), target));
                Check(result.openedPathLength < result.closedPathLengths[2], "Open gate provides a shorter route", result);
                Check(!NavMesh.Raycast(CompanionNavigationWorld.ToNavigation(new Vector2(7, -1)),
                    CompanionNavigationWorld.ToNavigation(target), out _, NavMesh.AllAreas),
                    "Open gate permits direct native navigation", result);
                result.status = "passed";
                Debug.Log("ROBOT_NAVMESH_TESTS_OK: " + result.passed.Count + " checks passed; 4/4 complete service paths.");
            }
            catch (Exception error)
            {
                result.status = "failed";
                result.error = error.ToString();
                throw;
            }
            finally
            {
                barrier.enabled = wasEnabled;
                if (sensorObject != null) UnityEngine.Object.DestroyImmediate(sensorObject);
                world.ReleaseValidationData();
                File.WriteAllText("NavMesh-Test-Sonuclari.json", JsonUtility.ToJson(result, true));
            }
        }

        private static NavMeshPath Path(Vector2 start, Vector2 target)
        {
            if (!NavMesh.SamplePosition(CompanionNavigationWorld.ToNavigation(start), out var a, 0.8f, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(CompanionNavigationWorld.ToNavigation(target), out var b, 0.8f, NavMesh.AllAreas))
                throw new Exception("Path endpoints are not on the native NavMesh: " + start);
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
                throw new Exception("Incomplete native path from " + start);
            return path;
        }

        private static float Length(NavMeshPath path)
        {
            float length = 0;
            var corners = path.corners;
            for (int i = 1; i < corners.Length; i++) length += Vector3.Distance(corners[i - 1], corners[i]);
            return length;
        }

        private static void Check(bool condition, string label, Results results)
        {
            if (!condition) throw new Exception(label);
            results.passed.Add(label);
        }
    }
}
