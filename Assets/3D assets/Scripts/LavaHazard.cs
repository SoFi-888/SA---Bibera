using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LavaHazard : MonoBehaviour
{
    public float touchPadding = 0.3f;

    private SpawnAtCheckpoint player;
    private Collider col;

    void Start()
    {
        player = FindFirstObjectByType<SpawnAtCheckpoint>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if (player == null || col == null) return;

        Bounds zone = col.bounds;
        zone.Expand(new Vector3(0f, touchPadding, 0f));

        if (zone.Intersects(player.GetBounds()))
            player.Respawn();
    }
}