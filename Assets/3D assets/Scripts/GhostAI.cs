using UnityEngine;
using System.Collections.Generic;
public class GhostAI : MonoBehaviour
{
    [Header("Patrol (perimeter checking)")]
    [Tooltip("Optional. Drag empty GameObjects here to make your own patrol route. Leave empty to patrol a rectangle around the ghost's start position.")]
    public Transform[] patrolPoints;
    public float patrolWidth = 10f;
    public float patrolDepth = 10f;
    public float patrolSpeed = 2f;
    public float waitAtPoint = 1.5f;
    public float lookAroundSpeed = 90f;

    [Header("Radar")]
    public float detectionRadius = 8f;  // player closer than this gets noticed
    [Tooltip("Player must get farther than this to be lost. Keep it larger than Detection Radius.")]
    public float loseRadius = 12f;

    [Header("Chase")]
    [Tooltip("Keep this lower than the player's Move Speed so the player can escape.")]
    public float chaseSpeed = 4f;
    public float chaseHeight = 1f;
    public float catchDistance = 1.2f;
    public float catchHeight = 3f;
    public float stunAfterCatch = 2f;

    [Header("Movement Feel")]
    public float turnSpeed = 8f;
    public float heightSmoothing = 3f;
    public float bobAmount = 0.15f;     // gentle floating up and down
    public float bobSpeed = 2f;
    [Tooltip("If the ghost moves backwards or sideways, try 180, 90 or -90.")]
    public float modelYawOffset = 0f;

    private enum State { Patrol, Chase }
    private State state = State.Patrol;

    private SpawnAtCheckpoint player;
    private List<Vector3> route = new List<Vector3>();
    private int currentIndex;
    private bool waiting;
    private float waitTimer;
    private float startY;
    private Vector3 startPos;
    private float ignorePlayerUntil;

    void OnValidate()
    {
        if (loseRadius < detectionRadius) loseRadius = detectionRadius;
    }

    void Start()
    {
        player = FindFirstObjectByType<SpawnAtCheckpoint>();
        if (player == null)
            Debug.LogWarning("GhostAI: no player found. Add SpawnAtCheckpoint to walkcycle.");

        startPos = transform.position;
        startY = startPos.y;
        BuildRoute();
    }

    void BuildRoute()
    {
        route.Clear();

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            foreach (Transform p in patrolPoints)
                if (p != null) route.Add(p.position);
        }

        if (route.Count == 0)
        {
            // Automatic rectangle around the start position
            float hx = patrolWidth * 0.5f;
            float hz = patrolDepth * 0.5f;
            route.Add(startPos + new Vector3(-hx, 0f, -hz));
            route.Add(startPos + new Vector3(hx, 0f, -hz));
            route.Add(startPos + new Vector3(hx, 0f, hz));
            route.Add(startPos + new Vector3(-hx, 0f, hz));
        }
    }

    void Update()
    {
        if (player == null) return;

        Vector3 playerPos = player.transform.position;
        float dist = HorizontalDistance(transform.position, playerPos);
        bool ignoring = Time.time < ignorePlayerUntil;

        if (state == State.Patrol)
        {
            if (!ignoring && dist <= detectionRadius)
                state = State.Chase;
            else
                Patrol();
        }

        if (state == State.Chase)
        {
            if (ignoring || dist > loseRadius)
            {
                // Player escaped the radar - go back to patrolling from the nearest point
                state = State.Patrol;
                currentIndex = ClosestPointIndex();
                waiting = false;
            }
            else
            {
                Chase(playerPos);
            }
        }
    }

    // ---------------- Patrol ----------------

    void Patrol()
    {
        ApplyHeight(startY);
        if (route.Count == 0) return;

        Vector3 target = route[currentIndex];

        if (waiting)
        {
            // Look around at the corner
            transform.Rotate(0f, lookAroundSpeed * Time.deltaTime, 0f, Space.World);
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                currentIndex = (currentIndex + 1) % route.Count;
            }
            return;
        }

        MoveHorizontal(target, patrolSpeed);

        if (HorizontalDistance(transform.position, target) < 0.15f)
        {
            waiting = true;
            waitTimer = waitAtPoint;
        }
    }

    int ClosestPointIndex()
    {
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < route.Count; i++)
        {
            float d = HorizontalDistance(transform.position, route[i]);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

    // ---------------- Chase ----------------

    void Chase(Vector3 playerPos)
    {
        MoveHorizontal(playerPos, chaseSpeed);
        ApplyHeight(playerPos.y + chaseHeight);

        float dist = HorizontalDistance(transform.position, playerPos);
        if (dist < catchDistance && Mathf.Abs(transform.position.y - playerPos.y) < catchHeight)
        {
            player.Respawn();
            ignorePlayerUntil = Time.time + stunAfterCatch;
            state = State.Patrol;
            currentIndex = ClosestPointIndex();
            waiting = false;
        }
    }


    void MoveHorizontal(Vector3 target, float speed)
    {
        Vector3 pos = transform.position;
        Vector3 dir = target - pos;
        dir.y = 0f;
        float d = dir.magnitude;

        if (d > 0.001f)
        {
            pos += dir.normalized * Mathf.Min(speed * Time.deltaTime, d);

            Quaternion look = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, modelYawOffset, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }
        transform.position = pos;
    }

    void ApplyHeight(float desiredY)
    {
        desiredY += Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, desiredY, heightSmoothing * Time.deltaTime);
        transform.position = pos;
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPos : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(loseRadius, detectionRadius));

        Gizmos.color = Color.cyan;
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;
                Transform next = patrolPoints[(i + 1) % patrolPoints.Length];
                if (next == null) continue;
                Gizmos.DrawLine(patrolPoints[i].position, next.position);
            }
        }
        else
        {
            float hx = patrolWidth * 0.5f;
            float hz = patrolDepth * 0.5f;
            Vector3 a = center + new Vector3(-hx, 0f, -hz);
            Vector3 b = center + new Vector3(hx, 0f, -hz);
            Vector3 c = center + new Vector3(hx, 0f, hz);
            Vector3 d = center + new Vector3(-hx, 0f, hz);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
    }
}