using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RotatingHazard : MonoBehaviour
{
    [Header("Rotation")]
    public Vector3 rotationAxis = Vector3.up;
    [Tooltip("Degrees per second. Use a negative number to spin the other way.")]
    public float rotationSpeed = 90f;
    [Tooltip("Optional. Leave empty to spin around the rectangle's own center. Assign an empty object to spin around that point instead (like a clock hand).")]
    public Transform pivot;

    [Header("Danger")]
    [Tooltip("Extra reach around the rectangle so brushing against it counts as touching.")]
    public float touchPadding = 0.05f;
    [Tooltip("Makes the collider a trigger so the player can't stand on or be blocked by it.")]
    public bool makeTriggerCollider = true;

    [Header("Look")]
    public bool applyColor = true;
    public Color hazardColor = new Color(0.85f, 0.1f, 0.1f);

    private BoxCollider box;
    private SpawnAtCheckpoint player;

    void Start()
    {
        box = GetComponent<BoxCollider>();
        player = FindFirstObjectByType<SpawnAtCheckpoint>();

        if (makeTriggerCollider) box.isTrigger = true;

        if (applyColor)
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null) r.material.color = hazardColor;
        }
    }

    void Update()
    {
        // Spin
        float step = rotationSpeed * Time.deltaTime;
        if (pivot != null)
            transform.RotateAround(pivot.position, rotationAxis.normalized, step);
        else
            transform.Rotate(rotationAxis.normalized, step, Space.Self);

        CheckPlayerTouch();
    }

    void CheckPlayerTouch()
    {
        if (player == null) return;

        Vector3 center = transform.TransformPoint(box.center);
        Vector3 scale = transform.lossyScale;
        Vector3 halfExtents =
            Vector3.Scale(box.size, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z))) * 0.5f
            + Vector3.one * touchPadding;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation, ~0, QueryTriggerInteraction.Collide);
        foreach (Collider c in hits)
        {
            if (c.transform.IsChildOf(player.transform))
            {
                player.Respawn();
                break;
            }
        }
    }
}