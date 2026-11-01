using Cernobil.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace Cernobil.Companion
{
    public enum RobotState { Following, WaitingForCommand, GoingToPanel, ActivatingPanel, Returning, Failed }

    [RequireComponent(typeof(RobotObstacleSensor))]
    public sealed class CompanionRobot : MonoBehaviour
    {
        [SerializeField] private CompanionNavigationWorld navigation;
        [SerializeField] private Transform player;
        [SerializeField] private Health playerHealth;
        [SerializeField] private GateControlPanel panel;
        [SerializeField] private SantralGate gate;
        [SerializeField] private Transform commandPoint;
        [SerializeField] private SpriteRenderer visual;
        private NavMeshAgent agent;
        private GameObject proxy;
        private RobotObstacleSensor sensor;
        private LineRenderer route;
        private Material routeMaterial;
        private float nextFollowRequest;
        private float nextRepath;
        private float activationStarted;
        private float commandStarted;
        private float lastProgress;
        private Vector3 progressPosition;
        private bool activated;
        private int retries;
        public RobotState State { get; private set; }
        public string Error { get; private set; }
        public float LastCompletionSeconds { get; private set; }
        public int SuccessfulCommands { get; private set; }
        public bool CanCommand => agent != null && agent.isOnNavMesh && !navigation.IsUpdating &&
            playerHealth.IsAlive && !gate.IsOpen && !Busy &&
            Vector2.Distance(player.position, commandPoint.position) < 3.5f &&
            Vector2.Distance(player.position, transform.position) < 3.2f;
        public bool Busy => State == RobotState.GoingToPanel || State == RobotState.ActivatingPanel || State == RobotState.Returning;
        public string Status => State == RobotState.Failed ? Error : gate.PassageOpen && !Busy ? "Robot: Kapı açık, seni takip ediyorum."
            : State == RobotState.GoingToPanel ? "Robot: Servis yolundan panele gidiyorum."
            : State == RobotState.ActivatingPanel ? "Robot: Kontrol panelini çalıştırıyorum."
            : State == RobotState.Returning ? "Robot: Yanına dönüyorum."
            : CanCommand ? "E — ROBOTA KAPIYI AÇ EMRİ VER"
            : "Robot: Kapının yanındaki sarı işarete ilerle.";

        public void Configure(CompanionNavigationWorld world, Transform owner, Health health,
            GateControlPanel control, SantralGate targetGate, Transform command, SpriteRenderer renderer)
        {
            navigation = world;
            player = owner;
            playerHealth = health;
            panel = control;
            gate = targetGate;
            commandPoint = command;
            visual = renderer;
        }

        private void Start()
        {
            sensor = GetComponent<RobotObstacleSensor>();
            if (!navigation.Ready) { Fail(navigation.Error ?? "Robotun yol bulma alanı hazır değil."); return; }
            if (!NavMesh.SamplePosition(CompanionNavigationWorld.ToNavigation(transform.position), out var start, 1, NavMesh.AllAreas))
            { Fail("Robot başlangıcı NavMesh üzerinde değil."); return; }
            proxy = new GameObject("Robot - Native NavMeshAgent");
            proxy.SetActive(false);
            proxy.transform.SetParent(navigation.transform, false);
            proxy.transform.position = start.position;
            agent = proxy.AddComponent<NavMeshAgent>();
            agent.agentTypeID = navigation.AgentTypeId;
            agent.radius = CompanionNavigationWorld.AgentRadius;
            agent.height = 0.5f;
            agent.baseOffset = 0;
            agent.speed = 3;
            agent.acceleration = 12;
            agent.angularSpeed = 720;
            agent.updateRotation = false;
            agent.autoRepath = true;
            proxy.SetActive(true);
            if (!agent.Warp(start.position)) { Fail("Robot NavMesh üzerine yerleştirilemedi."); return; }
            route = gameObject.AddComponent<LineRenderer>();
            routeMaterial = new Material(Shader.Find("Sprites/Default"));
            route.sharedMaterial = routeMaterial;
            route.startWidth = route.endWidth = 0.045f;
            route.startColor = route.endColor = new Color(0.2f, 0.95f, 1, 0.65f);
            route.sortingOrder = 500;
            route.enabled = false;
            State = RobotState.Following;
        }

        private void Update()
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
            if (!playerHealth.IsAlive) { agent.isStopped = true; if (route != null) route.enabled = false; return; }
            Vector3 desired = agent.desiredVelocity;
            sensor.Observe(new Vector2(desired.x, desired.z));
            if (navigation.IsUpdating) { agent.isStopped = true; return; }
            if (!Cernobil.Dialogue.NpcDialogue.BlocksPlayerInput && CanCommand && Input.GetKeyDown(KeyCode.E)) CommandOpenGate();

            if (State == RobotState.GoingToPanel)
            {
                if (Time.time - commandStarted > 60) { Fail("Robot yolu tamamlayamadı. E ile yeniden dene."); return; }
                if (sensor.Blocked)
                {
                    agent.isStopped = true;
                    if (Time.time >= nextRepath)
                    {
                        nextRepath = Time.time + 0.5f;
                        navigation.RequestRebuild();
                        if (++retries > 5) Fail("Robotun önü kapalı. Yolu kontrol edip E ile yeniden dene.");
                    }
                    return;
                }
                agent.isStopped = false;
                if (!agent.pathPending && (agent.pathStatus != NavMeshPathStatus.PathComplete || agent.isPathStale))
                {
                    if (!RequestPath(panel.transform.position, 0.16f)) return;
                }
                if (Vector3.Distance(agent.transform.position, progressPosition) > 0.08f)
                { progressPosition = agent.transform.position; lastProgress = Time.time; }
                if (Time.time - lastProgress > 6)
                { Fail("Robot ilerleyemiyor. E ile yeniden yol hesaplat."); return; }
                if (!agent.pathPending && agent.remainingDistance <= 0.25f &&
                    Vector2.Distance(transform.position, panel.transform.position) <= 0.5f)
                {
                    agent.isStopped = true;
                    activationStarted = Time.time;
                    State = RobotState.ActivatingPanel;
                }
            }
            else if (State == RobotState.ActivatingPanel)
            {
                if (!activated && Time.time - activationStarted >= 1.2f)
                {
                    if (!panel.TryActivate(this)) { Fail("Robot panelin etkileşim alanına ulaşamadı."); return; }
                    activated = true;
                }
                if (activated && gate.PassageOpen && !navigation.IsUpdating)
                {
                    SuccessfulCommands++;
                    LastCompletionSeconds = Time.time - commandStarted;
                    Debug.Log($"ROBOT_GATE_SUCCESS: {LastCompletionSeconds:F2}s; sensor={sensor.NearestDistance:F2}m; angle={sensor.NearestAngle:F0}");
                    State = RobotState.Returning;
                    RequestPath(player.position, 1.8f);
                }
            }
            else if (State == RobotState.Returning)
            {
                agent.isStopped = false;
                if (Vector2.Distance(transform.position, player.position) < 2.2f) State = RobotState.Following;
                else if (Time.time >= nextFollowRequest)
                { nextFollowRequest = Time.time + 0.5f; RequestPath(player.position, 1.8f); }
            }
            else if (State != RobotState.Failed)
            {
                if (CanCommand) { State = RobotState.WaitingForCommand; agent.isStopped = true; }
                else
                {
                    State = RobotState.Following;
                    if (Vector2.Distance(transform.position, player.position) <= 2.2f) agent.isStopped = true;
                    else if (Time.time >= nextFollowRequest)
                    { nextFollowRequest = Time.time + 0.4f; RequestPath(player.position, 1.8f); }
                }
            }
        }

        public bool CommandOpenGate()
        {
            if (!CanCommand) return false;
            Error = null;
            activated = false;
            retries = 0;
            commandStarted = lastProgress = Time.time;
            progressPosition = agent.transform.position;
            State = RobotState.GoingToPanel;
            return RequestPath(panel.transform.position, 0.16f);
        }

#if UNITY_EDITOR
        // Used only to choose a test's initial condition, before a command is issued.
        // Task movement after this point is exclusively performed by NavMeshAgent.
        public bool SetValidationStart(Vector2 initialPosition)
        {
            if (agent == null || !agent.isOnNavMesh || Busy || gate.IsOpen) return false;
            if (!NavMesh.SamplePosition(CompanionNavigationWorld.ToNavigation(initialPosition), out var hit, 0.8f, NavMesh.AllAreas))
                return false;
            agent.ResetPath();
            if (!agent.Warp(hit.position)) return false;
            transform.position = CompanionNavigationWorld.ToVisible(agent.transform.position);
            State = RobotState.Following;
            return true;
        }
#endif

        private bool RequestPath(Vector2 visibleTarget, float stoppingDistance)
        {
            if (!NavMesh.SamplePosition(CompanionNavigationWorld.ToNavigation(visibleTarget), out var target, 0.8f, NavMesh.AllAreas))
            { Fail("Hedef için yürünebilir alan bulunamadı. E ile tekrar dene."); return false; }
            var path = new NavMeshPath();
            if (!agent.CalculatePath(target.position, path) || path.status != NavMeshPathStatus.PathComplete)
            { Fail("Panele tam bir yol bulunamadı. E ile tekrar dene."); return false; }
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = false;
            // Let the native agent own its path request, especially just after a NavMesh update.
            // A precomputed path can become stale between a gate rebuild and SetPath.
            if (!agent.SetDestination(target.position)) { Fail("Robot hedef için yol isteği başlatamadı."); return false; }
            return true;
        }

        private void Fail(string error)
        {
            Error = error;
            State = RobotState.Failed;
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            if (route != null) route.enabled = false;
            Debug.LogWarning(error);
        }

        private void LateUpdate()
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
            transform.position = CompanionNavigationWorld.ToVisible(agent.transform.position);
            if (Mathf.Abs(agent.velocity.x) > 0.05f) visual.flipX = agent.velocity.x < 0;
            visual.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10);
            if (route == null) return;
            route.enabled = Busy && State != RobotState.ActivatingPanel && agent.hasPath;
            if (!route.enabled) return;
            var corners = agent.path.corners;
            route.positionCount = corners.Length;
            for (int i = 0; i < corners.Length; i++) route.SetPosition(i, CompanionNavigationWorld.ToVisible(corners[i]));
        }

        private void OnDisable()
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
        }

        private void OnDestroy()
        {
            if (proxy != null) Destroy(proxy);
            if (routeMaterial != null) Destroy(routeMaterial);
        }
    }
}
