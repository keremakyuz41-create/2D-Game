using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Cernobil.Companion
{
    [DefaultExecutionOrder(-200)]
    public sealed class CompanionNavigationWorld : MonoBehaviour
    {
        // Unity's genuine NavMesh lives on an XZ plane. The visible game remains in XY.
        // Only coordinates are projected; all routes and movement are owned by NavMeshAgent.
        public const float NavigationHeight = -50;
        public const float AgentRadius = 0.2f;
        private NavMeshData data;
        private NavMeshDataInstance instance;
        private NavMeshBuildSettings settings;
        private AsyncOperation update;
        private int signature;
        private float nextGeometryCheck;
        private bool rebuildRequested;
        public bool Ready => data != null && instance.valid;
        public bool IsUpdating => rebuildRequested || (update != null && !update.isDone);
        public int AgentTypeId => settings.agentTypeID;
        public string Error { get; private set; }
        public float LastBuildMilliseconds { get; private set; }

        public static Vector3 ToNavigation(Vector2 visible) => new Vector3(visible.x, NavigationHeight, visible.y);
        public static Vector3 ToVisible(Vector3 navigation) => new Vector3(navigation.x, navigation.z, 0);

        private void Awake() => BuildInitial();

        public bool BuildInitial()
        {
            if (Ready) return true;
            settings = NavMesh.GetSettingsCount() > 0 ? NavMesh.GetSettingsByIndex(0) : NavMesh.CreateSettings();
            settings.agentRadius = AgentRadius;
            settings.agentHeight = 0.5f;
            settings.agentClimb = 0.1f;
            settings.agentSlope = 45;
            settings.overrideVoxelSize = true;
            settings.voxelSize = 0.06f;
            settings.minRegionArea = 0.05f;
            var started = Time.realtimeSinceStartup;
            data = NavMeshBuilder.BuildNavMeshData(settings, Sources(), BuildBounds(), Vector3.zero, Quaternion.identity);
            LastBuildMilliseconds = (Time.realtimeSinceStartup - started) * 1000;
            if (data == null)
            {
                Error = "Robotun NavMesh alanı oluşturulamadı.";
                Debug.LogError(Error);
                return false;
            }
            data.name = "Companion - Real Unity NavMesh";
            instance = NavMesh.AddNavMeshData(data);
            signature = GeometrySignature();
            return Ready;
        }

        public void RequestRebuild() => rebuildRequested = true;

#if UNITY_EDITOR
        public bool RebuildForValidation()
        {
            if (!Ready) return false;
            return NavMeshBuilder.UpdateNavMeshData(data, settings, Sources(), BuildBounds());
        }

        public void ReleaseValidationData()
        {
            if (data == null) return;
            NavMeshBuilder.Cancel(data);
            if (instance.valid) instance.Remove();
            DestroyImmediate(data);
            data = null;
        }
#endif

        private void Update()
        {
            if (!Ready || (update != null && !update.isDone)) return;
            if (Time.unscaledTime >= nextGeometryCheck)
            {
                nextGeometryCheck = Time.unscaledTime + 0.5f;
                if (signature != GeometrySignature()) rebuildRequested = true;
            }
            if (!rebuildRequested) return;
            rebuildRequested = false;
            signature = GeometrySignature();
            update = NavMeshBuilder.UpdateNavMeshDataAsync(data, settings, Sources(), BuildBounds());
        }

        private static Bounds BuildBounds() => new Bounds(new Vector3(0, NavigationHeight + 1, -0.5f),
            new Vector3(32, 6, 16));

        private static List<NavMeshBuildSource> Sources()
        {
            Physics2D.SyncTransforms();
            var sources = new List<NavMeshBuildSource>
            {
                new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Box,
                    transform = Matrix4x4.TRS(new Vector3(0, NavigationHeight - 0.1f, -0.5f), Quaternion.identity, Vector3.one),
                    size = new Vector3(31, 0.2f, 15),
                    area = 0
                }
            };
            foreach (var marker in FindObjectsByType<NavigationObstacle2D>(FindObjectsSortMode.InstanceID))
            {
                var collider = marker.Collider;
                if (!collider.enabled || collider.isTrigger) continue;
                var bounds = collider.bounds;
                sources.Add(new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Box,
                    transform = Matrix4x4.TRS(new Vector3(bounds.center.x, NavigationHeight + 1, bounds.center.y),
                        Quaternion.identity, Vector3.one),
                    size = new Vector3(bounds.size.x, 2, bounds.size.y),
                    area = 1 // Unity's built-in Not Walkable area.
                });
            }
            return sources;
        }

        private static int GeometrySignature()
        {
            unchecked
            {
                int hash = 17;
                foreach (var marker in FindObjectsByType<NavigationObstacle2D>(FindObjectsSortMode.InstanceID))
                {
                    var collider = marker.Collider;
                    if (!collider.enabled || collider.isTrigger) continue;
                    hash = hash * 31 + marker.GetInstanceID();
                    hash = hash * 31 + collider.bounds.GetHashCode();
                }
                return hash;
            }
        }

        private void OnDestroy()
        {
            if (data == null) return;
            NavMeshBuilder.Cancel(data);
            if (instance.valid) instance.Remove();
            Destroy(data);
        }
    }
}
