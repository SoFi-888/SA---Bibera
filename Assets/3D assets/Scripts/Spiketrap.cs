using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 moveDirection = Vector3.up;

    [Tooltip("How high the spikes rise above their starting position (in Unity units). Change this to make the trap taller or shorter.")]
    [Min(0f)]
    public float moveDistance = 3f;

    [Tooltip("Higher = faster (full cycle takes about 6.28 / speed seconds)")]
    public float speed = 1.5f;

    [Tooltip("Give each trap a different value so they don't move in sync")]
    public float timeOffset = 0f;

    [Header("Danger")]
    [Tooltip("If on, spikes are harmless while mostly lowered")]
    public bool onlyDeadlyWhenRaised = false;
    [Range(0f, 1f)]
    [Tooltip("How high they must be to hurt (0 = down, 1 = fully up)")]
    public float raisedThreshold = 0.3f;
    [Tooltip("Makes the danger zone slightly smaller than the model")]
    public float hitboxShrink = 0.1f;

    private Vector3 startPos;
    private SpawnAtCheckpoint player;
    private Renderer[] renderers;

    void Start()
    {
        startPos = transform.position;
        player = FindFirstObjectByType<SpawnAtCheckpoint>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        float t = (1f - Mathf.Cos((Time.time + timeOffset) * speed)) * 0.5f;
        transform.position = startPos + moveDirection.normalized * moveDistance * t;

        if (player == null) return;
        if (onlyDeadlyWhenRaised && t < raisedThreshold) return;

        if (player.GetBounds().Intersects(GetHazardBounds()))
            player.Respawn();
    }

    Bounds GetHazardBounds()
    {
        if (renderers == null || renderers.Length == 0)
            return new Bounds(transform.position, Vector3.one);

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        b.Expand(-hitboxShrink);
        return b;
    }
}