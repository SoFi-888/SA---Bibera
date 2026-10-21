using UnityEngine;
public class LavaBall : MonoBehaviour
{
    [Header("Eruption")]
    [Tooltip("How high the ball flies above its starting position.")]
    public float launchHeight = 6f;
    [Tooltip("Seconds from launch until the ball is back down.")]
    public float airTime = 2f;
    [Tooltip("Seconds the ball waits between eruptions.")]
    public float restTime = 1.5f;
    [Tooltip("Adds a random extra wait (0 to this value) so eruptions feel natural.")]
    public float restRandomness = 0.5f;

    [Header("Timing")]
    [Tooltip("Seconds before the first eruption.")]
    public float startDelay = 0f;
    [Tooltip("Adds a random delay to the first eruption so balls don't all launch together.")]
    public bool randomizeStartDelay = true;

    [Header("Look")]
    public bool hideWhileResting = true;
    public Vector3 spin = new Vector3(60f, 90f, 30f);

    [Header("Danger")]
    [Tooltip("How close counts as touching. 0 = work it out automatically from the model's size.")]
    public float hitRadius = 0f;

    private enum State { Resting, Flying }
    private State state = State.Resting;

    private SpawnAtCheckpoint player;
    private Renderer[] renderers;
    private Vector3 startPos;
    private float timer;
    private float currentRest;
    private float gravity;
    private float launchSpeed;

    void Start()
    {
        player = FindFirstObjectByType<SpawnAtCheckpoint>();
        renderers = GetComponentsInChildren<Renderer>();
        startPos = transform.position;

        // Work out the hit radius from the model's size
        if (hitRadius <= 0f && renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            hitRadius = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z)) * 0.9f;
        }
        if (hitRadius <= 0f) hitRadius = 0.5f;

        airTime = Mathf.Max(0.1f, airTime);
        gravity = 8f * launchHeight / (airTime * airTime);
        launchSpeed = gravity * airTime * 0.5f;

        currentRest = startDelay + (randomizeStartDelay ? Random.Range(0f, restTime + restRandomness) : 0f);
        SetVisible(!hideWhileResting);
    }

    void Update()
    {
        transform.Rotate(spin * Time.deltaTime, Space.Self);
        timer += Time.deltaTime;

        if (state == State.Resting)
        {
            if (timer >= currentRest)
            {
                state = State.Flying;
                timer = 0f;
                SetVisible(true);
            }
            return;
        }

        float t = timer;
        float height = launchSpeed * t - 0.5f * gravity * t * t;
        transform.position = startPos + Vector3.up * Mathf.Max(0f, height);

        CheckPlayerHit();

        if (t >= airTime)
        {
            transform.position = startPos;
            state = State.Resting;
            timer = 0f;
            currentRest = restTime + Random.Range(0f, restRandomness);
            SetVisible(!hideWhileResting);
        }
    }

    void CheckPlayerHit()
    {
        if (player == null) return;

        Vector3 closest = player.GetBounds().ClosestPoint(transform.position);
        if ((closest - transform.position).sqrMagnitude <= hitRadius * hitRadius)
            player.Respawn();
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers) r.enabled = visible;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 basePos = Application.isPlaying ? startPos : transform.position;
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawLine(basePos, basePos + Vector3.up * launchHeight);
        Gizmos.DrawWireSphere(basePos + Vector3.up * launchHeight, Mathf.Max(hitRadius, 0.3f));
    }
}