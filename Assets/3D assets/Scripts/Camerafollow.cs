using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    [Header("Target (leave empty to auto-find the player)")]
    public Transform target;

    [Header("Bird's Eye Settings")]
    public float height = 15f;
    [Range(45f, 90f)]
    public float tiltAngle = 75f;
    public float smoothTime = 0.15f;

    private Vector3 velocity;

    void Start()
    {
        if (target == null)
        {
            var player = FindFirstObjectByType<PlayerMovement>();
            if (player != null) target = player.transform;
            else Debug.LogWarning("CameraFollow: no player found to follow.");
        }

        if (target != null) transform.position = DesiredPosition();
        transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);
        transform.position = Vector3.SmoothDamp(transform.position, DesiredPosition(), ref velocity, smoothTime);
    }

    Vector3 DesiredPosition()
    {
        float back = height / Mathf.Tan(tiltAngle * Mathf.Deg2Rad);
        return target.position + new Vector3(0f, height, -back);
    }
}