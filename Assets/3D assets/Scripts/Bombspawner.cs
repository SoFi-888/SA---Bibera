using UnityEngine;
public class BombSpawner : MonoBehaviour
{
    [Header("Required - drag your Bomb model/prefab here")]
    public GameObject bombPrefab;

    [Header("Timing")]
    public float interval = 10f;
    public float firstBombDelay = 10f;

    [Header("Throw")]
    public float dropHeight = 12f;
    public float throwDistance = 14f;
    public float scatter = 3f;
    public float tumbleSpeed = 5f;
    public float bombScale = 1f;
    public float bombMass = 5f;

    [Header("Explosion")]
    public float explosionRadius = 2f;
    public GameObject explosionEffectPrefab;

    private SpawnAtCheckpoint player;
    private float nextThrowTime;

    void Start()
    {
        player = FindFirstObjectByType<SpawnAtCheckpoint>();
        nextThrowTime = Time.time + firstBombDelay;

        if (bombPrefab == null)
            Debug.LogWarning("BombSpawner: assign the Bomb model to 'Bomb Prefab'.");
    }

    void Update()
    {
        if (player == null || bombPrefab == null) return;

        if (Time.time >= nextThrowTime)
        {
            ThrowBomb();
            nextThrowTime += interval;
        }
    }

    void ThrowBomb()
    {
        Vector3 playerPos = player.transform.position;
        Vector2 offset = Random.insideUnitCircle * scatter;
        Vector3 target = playerPos + new Vector3(offset.x, 0f, offset.y);
        target.y = FindGroundY(target, playerPos.y);

        // 2. Pick a random start point off to the side and above
        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.up;
        Vector3 start = target + new Vector3(dir.x, 0f, dir.y) * throwDistance + Vector3.up * dropHeight;

        float g = Mathf.Abs(Physics.gravity.y);
        float flightTime = Mathf.Sqrt(2f * dropHeight / g);
        Vector3 horizontal = target - start;
        horizontal.y = 0f;
        Vector3 launchVelocity = horizontal / flightTime;

        // 4. Create the bomb
        GameObject go = Instantiate(bombPrefab, start, Quaternion.identity);
        go.SetActive(true);
        go.transform.localScale *= bombScale;
        PrepareBomb(go);
        go.transform.rotation = Random.rotation;

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = bombMass;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = launchVelocity;
        rb.angularVelocity = Random.onUnitSphere * tumbleSpeed;

        Bomb bomb = go.GetComponent<Bomb>();
        if (bomb == null) bomb = go.AddComponent<Bomb>();
        bomb.Init(player, target, flightTime, explosionRadius, explosionEffectPrefab);
    }

    void PrepareBomb(GameObject go)
    {
        foreach (MeshCollider mc in go.GetComponentsInChildren<MeshCollider>())
            mc.convex = true;

        if (go.GetComponentInChildren<Collider>() != null) return;

        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        SphereCollider sc = go.AddComponent<SphereCollider>();
        if (rends.Length == 0) return;

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

        Vector3 s = go.transform.lossyScale;
        float maxScale = Mathf.Max(s.x, Mathf.Max(s.y, s.z));
        sc.center = go.transform.InverseTransformPoint(b.center);
        sc.radius = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z)) / maxScale;
    }

    float FindGroundY(Vector3 point, float fallbackY)
    {
        RaycastHit[] hits = Physics.RaycastAll(point + Vector3.up * 100f, Vector3.down, 200f,
                                               ~0, QueryTriggerInteraction.Ignore);
        float bestDistance = float.MaxValue;
        float groundY = fallbackY;

        foreach (RaycastHit h in hits)
        {
            if (h.transform.IsChildOf(player.transform)) continue;
            if (h.distance < bestDistance)
            {
                bestDistance = h.distance;
                groundY = h.point.y;
            }
        }
        return groundY;
    }
}