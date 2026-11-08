
using UnityEngine;
using UnityEngine.AI;

public class MomChase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform playerSightPoint;
    [SerializeField] private NavMeshAgent agent;

    [Header("Vision")]
    [SerializeField] private float visionDistance = 20f;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float updateInterval = 0.15f;
    [SerializeField, Range(10f, 360f)]
    private float visionAngle = 60f;

    [Header("Movement")]
    [SerializeField] private float visibleSpeed = 6f;
    [SerializeField] private float hiddenSpeed = 2f;
    [SerializeField] private float wanderingSpeed = 2f;
    [SerializeField] private float currentSpeed;

    [Header("Wandering")]
    [SerializeField] private float timeBeforeWandering = 5f;

    [SerializeField] private float wanderCountdown = 5f;

    [SerializeField] private bool isWandering = false;

    [Header("Player Stealth")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHiding playerHiding;

    [SerializeField, Range(0.1f, 1f)]
    private float standingSightHeightPercent = 0.8f;

    [SerializeField, Range(0.1f, 1f)]
    private float crouchingSightHeightPercent = 0.55f;

    private CharacterController playerCharacterController;

    private bool canSeePlayer;
    private float nextVisionCheckTime;
    private float nextWanderTargetTime;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        wanderCountdown = timeBeforeWandering;
        isWandering = false;

        if (player != null)
        {
            if (playerController == null)
                playerController = player.GetComponent<PlayerController>();

            playerCharacterController =
                player.GetComponent<CharacterController>();
        }

        if (player != null)
        {
            if (playerHiding == null)
                playerHiding = player.GetComponent<PlayerHiding>();
        }
    }



    private void Update()
    {
        if (player == null || agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        bool playerIsHiding =
            playerHiding != null && playerHiding.IsHiding;

        // Görüş kontrolü
        if (Time.time >= nextVisionCheckTime)
        {
            nextVisionCheckTime = Time.time + updateInterval;

            canSeePlayer = playerIsHiding
                ? false
                : CanSeePlayer();
        }

        // Saklanıyorsa eski görüş bilgisini de geçersiz kıl.
        if (playerIsHiding)
            canSeePlayer = false;

        // 1. Oyuncuyu gerçekten görüyorsa hızlı kovala.
        if (canSeePlayer)
        {
            wanderCountdown = timeBeforeWandering;
            isWandering = false;

            SetAgentSpeed(visibleSpeed);
            agent.SetDestination(player.position);

            return;
        }

        // 2. Oyuncuyu göremiyorsa geri sayım.
        if (!isWandering)
        {
            wanderCountdown = Mathf.Max(
                0f,
                wanderCountdown - Time.deltaTime
            );

            if (wanderCountdown > 0f)
            {
                SetAgentSpeed(hiddenSpeed);

                // Oyuncu saklanmıyorsa yavaşça takip et.
                // Saklanıyorsa son mevcut rotasını koru.
                if (!playerIsHiding)
                {
                    agent.SetDestination(player.position);
                }

                return;
            }

            // 3. Süre bitti, rastgele dolaşmaya geç.
            isWandering = true;

            agent.ResetPath();
            nextWanderTargetTime = 0f;
        }

        // 4. Wandering
        SetAgentSpeed(wanderingSpeed);

        bool reachedDestination =
            !agent.pathPending &&
            (!agent.hasPath ||
             agent.remainingDistance <=
             agent.stoppingDistance + 0.3f);

        if (reachedDestination &&
            Time.time >= nextWanderTargetTime)
        {
            bool foundDestination =
                TryChooseRandomDestination();

            nextWanderTargetTime = Time.time +
                (foundDestination ? 0.5f : 1f);
        }
    }



    private bool CanSeePlayer()
    {
        if (playerHiding != null && playerHiding.IsHiding)
            return false;

        Vector3 origin = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * 1.6f;

        Vector3 targetPoint;

        if (playerCharacterController != null)
        {
            Bounds playerBounds = playerCharacterController.bounds;

            bool crouching =
                playerController != null &&
                playerController.IsCrouching;

            float heightPercent = crouching
                ? crouchingSightHeightPercent
                : standingSightHeightPercent;

            targetPoint = new Vector3(
                playerBounds.center.x,
                playerBounds.min.y +
                    playerBounds.size.y * heightPercent,
                playerBounds.center.z
            );
        }
        else
        {
            targetPoint = playerSightPoint != null
                ? playerSightPoint.position
                : player.position + Vector3.up * 1.2f;
        }

        Vector3 direction = targetPoint - origin;

        if (direction.sqrMagnitude > visionDistance * visionDistance)
            return false;

        Vector3 forward = eyePoint != null
            ? eyePoint.forward
            : transform.forward;

        Vector3 horizontalForward =
            Vector3.ProjectOnPlane(forward, Vector3.up).normalized;

        Vector3 horizontalDirection =
            Vector3.ProjectOnPlane(direction, Vector3.up).normalized;

        if (horizontalDirection.sqrMagnitude > 0.001f)
        {
            float angle = Vector3.Angle(
                horizontalForward,
                horizontalDirection
            );

            if (angle > visionAngle * 0.5f)
                return false;
        }

        bool blocked = Physics.Linecast(
            origin,
            targetPoint,
            obstructionMask,
            QueryTriggerInteraction.Ignore
        );

        return !blocked;
    }

    private void SetAgentSpeed(float speed)
    {
        agent.speed = speed;
        currentSpeed = agent.speed;
    }

    private bool TryChooseRandomDestination()
    {
        NavMeshTriangulation triangulation =
            NavMesh.CalculateTriangulation();

        if (triangulation.vertices == null ||
            triangulation.indices == null ||
            triangulation.indices.Length < 3)
        {
            return false;
        }

        int triangleCount = triangulation.indices.Length / 3;

        NavMeshPath path = new NavMeshPath();

        for (int attempt = 0; attempt < 30; attempt++)
        {
            int triangleStart = Random.Range(0, triangleCount) * 3;

            Vector3 a = triangulation.vertices[
                triangulation.indices[triangleStart]
            ];

            Vector3 b = triangulation.vertices[
                triangulation.indices[triangleStart + 1]
            ];

            Vector3 c = triangulation.vertices[
                triangulation.indices[triangleStart + 2]
            ];

            float u = Random.value;
            float v = Random.value;

            if (u + v > 1f)
            {
                u = 1f - u;
                v = 1f - v;
            }

            Vector3 randomPoint =
                a + u * (b - a) + v * (c - a);

            if (!NavMesh.SamplePosition(
                randomPoint,
                out NavMeshHit hit,
                2f,
                agent.areaMask))
            {
                continue;
            }

            if (!agent.CalculatePath(hit.position, path))
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            return agent.SetPath(path);
        }

        return false;
    }


    private void OnDrawGizmosSelected()
    {
        Vector3 origin = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * 1.6f;

        Vector3 forward = eyePoint != null
            ? eyePoint.forward
            : transform.forward;

        Gizmos.color = Color.cyan;

        Vector3 endPoint = origin + forward * visionDistance;

        Gizmos.DrawLine(origin, endPoint);
        Gizmos.DrawSphere(endPoint, 0.08f);

        Gizmos.DrawWireSphere(origin, visionDistance);
    }

}
