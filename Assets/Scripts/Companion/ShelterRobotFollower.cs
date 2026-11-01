using Cernobil.Combat;
using Cernobil.World;
using UnityEngine;
using UnityEngine.AI;

namespace Cernobil.Companion
{
    public sealed class ShelterRobotFollower : MonoBehaviour
    {
        [SerializeField] private CompanionNavigationWorld world;
        [SerializeField] private Transform player;
        [SerializeField] private Health health;
        [SerializeField] private ShelterGate gate;
        private NavMeshAgent agent;
        private GameObject proxy;
        private float nextPath;
        public void Configure(CompanionNavigationWorld navigation, Transform owner, Health life, ShelterGate door)
        { world = navigation; player = owner; health = life; gate = door; }
        private void Start()
        {
            if (!world.Ready || !NavMesh.SamplePosition(CompanionNavigationWorld.ToNavigation(transform.position), out var hit, 1, NavMesh.AllAreas)) return;
            proxy = new GameObject("Siginak Robot - Native NavMeshAgent");
            proxy.SetActive(false);
            proxy.transform.position = hit.position;
            agent = proxy.AddComponent<NavMeshAgent>();
            agent.agentTypeID = world.AgentTypeId;
            agent.radius = CompanionNavigationWorld.AgentRadius;
            agent.height = 0.5f;
            agent.speed = 3.6f;
            agent.acceleration = 14;
            agent.stoppingDistance = 1.4f;
            agent.updateRotation = false;
            proxy.SetActive(true);
        }
        private void Update()
        {
            if (agent == null || !agent.isOnNavMesh) return;
            if (!health.IsAlive || gate.Completed || world.IsUpdating) { agent.isStopped = true; return; }
            agent.isStopped = false;
            if (Time.time < nextPath) return;
            nextPath = Time.time + 0.3f;
            if (NavMesh.SamplePosition(CompanionNavigationWorld.ToNavigation(player.position), out var target, 1, NavMesh.AllAreas))
                agent.SetDestination(target.position);
        }
        private void LateUpdate()
        {
            if (agent == null || !agent.isOnNavMesh) return;
            transform.position = CompanionNavigationWorld.ToVisible(agent.transform.position);
            var visual = GetComponent<SpriteRenderer>();
            visual.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10);
            if (Mathf.Abs(agent.velocity.x) > 0.05f) visual.flipX = agent.velocity.x < 0;
        }
        private void OnDestroy() { if (proxy != null) Destroy(proxy); }
    }
}
