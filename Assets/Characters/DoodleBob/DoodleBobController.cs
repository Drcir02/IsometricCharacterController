using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
public class DoodleBobController : MonoBehaviour
{
    [Header("References")]
    public Transform player; // pokud není nastaveno v inspectoru, pokusí se najít objekt s tagem "Player"

    [Header("Movement")]
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float rotateSpeed = 8f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float terminalVelocity = -20f;

    [Header("Pathfinding")]
    [SerializeField] private float pathRecalcInterval = 0.35f; // jak často přepočítat cestu
    [SerializeField] private float reachCornerDistance = 0.4f; // kdy považovat corner za dosažený

    [Header("Stuck detection")]
    [SerializeField] private float stuckCheckInterval = 0.5f;
    [SerializeField] private float stuckMoveThreshold = 0.05f; // pokud se za interval nepohneme víc než tohle, považovat za stuck
    [SerializeField] private float stuckRecoveryMaxAttempts = 3;

    [Header("Fallback obstacle avoidance")]
    [SerializeField] private float obstacleDetectDistance = 1.2f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float fallbackAvoidStrength = 0.8f;

    private CharacterController controller;
    private NavMeshAgent agent;

    private float verticalVelocity;
    private NavMeshPath currentPath;
    private int currentCornerIndex;
    private float pathRecalcTimer;

    // stuck detection
    private Vector3 lastStuckCheckPos;
    private float stuckCheckTimer;
    private int stuckAttempts;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        // agent bude pouze počítat path, pohyb provádí CharacterController
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = speed;
        agent.angularSpeed = 720f;
        agent.acceleration = 8f;
        agent.radius = Mathf.Max(0.3f, controller.radius);
        agent.height = controller.height;

        currentPath = new NavMeshPath();
        currentCornerIndex = 0;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        lastStuckCheckPos = transform.position;
    }

    private void Update()
    {
        if (player == null) return;

        bool onNavMesh = agent.isOnNavMesh;
        Vector3 horizontalMove = Vector3.zero;

        // Periodicky přepočítat path (úspora oproti každému snímku)
        pathRecalcTimer -= Time.deltaTime;
        if (onNavMesh && pathRecalcTimer <= 0f)
        {
            RecalculatePath();
            pathRecalcTimer = pathRecalcInterval;
        }

        if (onNavMesh && currentPath != null && currentPath.corners.Length > 0)
        {
            // zajisti, že index ukazuje na první "další" corner (0 == pozice agenta)
            currentCornerIndex = Mathf.Clamp(currentCornerIndex, 1, currentPath.corners.Length - 1);

            Vector3 targetCorner = currentPath.corners[currentCornerIndex];
            Vector3 dirToCorner = targetCorner - transform.position;
            dirToCorner.y = 0f;

            // pokud jsme dost blízko corneru -> posun na další
            if (dirToCorner.magnitude <= reachCornerDistance)
            {
                if (currentCornerIndex < currentPath.corners.Length - 1)
                {
                    currentCornerIndex++;
                }
                else
                {
                    // poslední corner (cílová pozice) -> jdi přímo k hráči
                    dirToCorner = (player.position - transform.position);
                    dirToCorner.y = 0f;
                }
            }

            // pokud nějaký dir existuje -> použij ho
            if (dirToCorner.sqrMagnitude > 0.0001f)
            {
                horizontalMove = dirToCorner.normalized * speed;
            }
            else
            {
                horizontalMove = Vector3.zero;
            }
        }
        else
        {
            // fallback: jednoduché obejití překážky pokud není navmesh nebo cesta není dostupná
            horizontalMove = SimpleChaseWithAvoid();
        }

        // Gravity
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -1f;
        }
        verticalVelocity += gravity * Time.deltaTime;
        verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);

        Vector3 finalMove = (horizontalMove + Vector3.up * verticalVelocity) * Time.deltaTime;
        controller.Move(finalMove);

        // synchronizace pozice agenta (důležité pro správné path výpočty)
        if (onNavMesh)
            agent.nextPosition = transform.position;

        // rotace směrem k pohybu (jen horizontálně)
        Vector3 lookDir = new Vector3(horizontalMove.x, 0f, horizontalMove.z);
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotateSpeed * Time.deltaTime);
        }

        // Stuck detection: kontroluj pohyb v intervalech
        stuckCheckTimer -= Time.deltaTime;
        if (stuckCheckTimer <= 0f)
        {
            float moved = Vector3.Distance(transform.position, lastStuckCheckPos);
            if (moved < stuckMoveThreshold)
            {
                stuckAttempts++;
                TryRecoverFromStuck();
            }
            else
            {
                stuckAttempts = 0;
            }

            lastStuckCheckPos = transform.position;
            stuckCheckTimer = stuckCheckInterval;
        }
    }

    private void RecalculatePath()
    {
        if (!agent.isOnNavMesh)
            return;

        // vypočti path směrem k hráči
        NavMeshPath newPath = new NavMeshPath();
        agent.CalculatePath(player.position, newPath);

        // pokud není path validní, zkus znovu s menším radiusem (během runtime to pomůže v některých edge-casech)
        if (newPath.status == NavMeshPathStatus.PathInvalid)
        {
            // pokus o nalezení nejbližší pozice na NavMeshi k cíli
            NavMeshHit hit;
            if (NavMesh.SamplePosition(player.position, out hit, 2.0f, agent.areaMask))
            {
                agent.CalculatePath(hit.position, newPath);
            }
        }

        if (newPath.status == NavMeshPathStatus.PathComplete || newPath.corners.Length > 0)
        {
            currentPath = newPath;
            // nastav index na první corner za pozicí (pokud existuje)
            currentCornerIndex = Mathf.Clamp(1, 1, currentPath.corners.Length - 1);
        }
        else
        {
            // pokud cesta není použitelná, nech currentPath null -> fallback bude použít SimpleChaseWithAvoid
            currentPath = null;
        }
    }

    // jednoduchý fallback: pokud mezi DoodleBobem a hráčem je překážka -> snažíme se obejít použitím tangensu kolizní normály
    private Vector3 SimpleChaseWithAvoid()
    {
        Vector3 dirToPlayer = (player.position - transform.position);
        dirToPlayer.y = 0f;
        Vector3 dir = dirToPlayer.normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, dir, out hit, obstacleDetectDistance, obstacleMask))
        {
            // máme překážku — vytvoř odklon podél překážky
            Vector3 tangent = Vector3.Cross(hit.normal, Vector3.up).normalized;
            if (Vector3.Dot(tangent, dir) < 0) tangent = -tangent;
            Vector3 steer = (dir * (1f - fallbackAvoidStrength) + tangent * fallbackAvoidStrength).normalized;
            return steer * speed;
        }
        else
        {
            return dir * speed;
        }
    }

    private void TryRecoverFromStuck()
    {
        if (stuckAttempts <= 0) return;

        // 1) Pokud máme path, zkus přeskočit na další corner (pokud existuje)
        if (currentPath != null && currentPath.corners.Length > 0)
        {
            if (currentCornerIndex < currentPath.corners.Length - 1)
            {
                currentCornerIndex++;
                return;
            }
        }

        // 2) Zkus přepočítat path s malým offsetem kolem hráče (pokud je cíl obklíčený)
        if (agent.isOnNavMesh)
        {
            // zkus najít náhradní pozici poblíž hráče
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (1.0f + stuckAttempts * 0.5f);
                NavMeshHit hit;
                if (NavMesh.SamplePosition(player.position + offset, out hit, 1.0f + stuckAttempts * 0.5f, agent.areaMask))
                {
                    NavMeshPath altPath = new NavMeshPath();
                    agent.CalculatePath(hit.position, altPath);
                    if (altPath.status == NavMeshPathStatus.PathComplete && altPath.corners.Length > 0)
                    {
                        currentPath = altPath;
                        currentCornerIndex = Mathf.Clamp(1, 1, currentPath.corners.Length - 1);
                        return;
                    }
                }
            }
        }

        // 3) Pokud recovery pokusy překročily limit, použij fallback nudging (malý náhodný pohyb)
        if (stuckAttempts >= stuckRecoveryMaxAttempts)
        {
            Vector3 nudge = (Random.insideUnitSphere * 0.6f);
            nudge.y = 0f;
            controller.Move(nudge * Time.deltaTime); // jednorázový malý nudge
            stuckAttempts = 0;
        }
    }
}