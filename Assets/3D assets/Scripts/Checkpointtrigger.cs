using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    public Color activatedColor = new Color(0.2f, 0.8f, 1f);

    private SpawnAtCheckpoint player;
    private Collider col;
    private bool activated;

    void Start()
    {
        player = FindFirstObjectByType<SpawnAtCheckpoint>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if (activated || player == null || col == null) return;

        Bounds zone = col.bounds;
        zone.Expand(new Vector3(0.5f, 2f, 0.5f));

        if (zone.Intersects(player.GetBounds()))
        {
            player.SetCheckpoint(transform);
            activated = true;

            Renderer r = GetComponent<Renderer>();
            if (r != null) r.material.color = activatedColor;
        }
    }
}