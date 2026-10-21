using UnityEngine;
public class Bomb : MonoBehaviour
{
    private SpawnAtCheckpoint player;
    private Collider bombCollider;
    private GameObject marker;
    private GameObject explosionPrefab;
    private Vector3 target;
    private float flightTime;
    private float spawnTime;
    private float explosionRadius;
    private bool exploded;

    public void Init(SpawnAtCheckpoint playerRef, Vector3 landingPoint, float totalFlightTime,
                     float radius, GameObject fxPrefab)
    {
        player = playerRef;
        target = landingPoint;
        flightTime = totalFlightTime;
        explosionRadius = radius;
        explosionPrefab = fxPrefab;
        spawnTime = Time.time;
        bombCollider = GetComponentInChildren<Collider>();
        CreateMarker();
    }

    void Update()
    {
        if (exploded) return;

        if (marker != null)
        {
            float progress = Mathf.Clamp01((Time.time - spawnTime) / Mathf.Max(flightTime, 0.01f));
            float size = explosionRadius * 2f * Mathf.Lerp(0.3f, 1f, progress);
            size *= 1f + 0.08f * Mathf.Sin(Time.time * 12f);
            marker.transform.localScale = new Vector3(size, 0.01f, size);
        }

        if (player != null && bombCollider != null &&
            player.GetBounds().Intersects(bombCollider.bounds))
        {
            Explode();
            return;
        }

        if (Time.time - spawnTime > 20f || transform.position.y < -100f)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        Vector3 pos = transform.position;

        if (player != null)
        {
            Vector3 closest = player.GetBounds().ClosestPoint(pos);
            if ((closest - pos).sqrMagnitude <= explosionRadius * explosionRadius)
                player.Respawn();
        }

        SpawnExplosion(pos);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (marker != null) Destroy(marker);
    }

    void CreateMarker()
    {
        marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "BombLandingMarker";
        Destroy(marker.GetComponent<Collider>());
        marker.transform.position = target + Vector3.up * 0.05f;

        Renderer rend = marker.GetComponent<Renderer>();
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Shader sh = FindShader("Universal Render Pipeline/Unlit", "Unlit/Color", "Sprites/Default");
        if (sh != null)
        {
            Material m = new Material(sh);
            m.color = new Color(1f, 0.15f, 0.1f);
            rend.material = m;
        }
    }

    void SpawnExplosion(Vector3 pos)
    {
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, pos, Quaternion.identity);
            Destroy(fx, 5f);
            return;
        }
        GameObject go = new GameObject("Explosion");
        go.transform.position = pos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // must stop before changing settings

        var main = ps.main;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = 0.9f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 10f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 1.3f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.7f, 0.15f), new Color(1f, 0.25f, 0f));
        main.gravityModifier = 0.3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 70) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.85f, 0.3f), 0f),
                new GradientColorKey(new Color(0.25f, 0.1f, 0.05f), 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f) });
        colorOverLife.color = grad;

        ParticleSystemRenderer pr = go.GetComponent<ParticleSystemRenderer>();
        Shader sh = FindShader("Universal Render Pipeline/Particles/Unlit", "Sprites/Default", "Particles/Standard Unlit");
        if (sh != null) pr.material = new Material(sh);

        ps.Play();
        Destroy(go, 2.5f);
    }

    static Shader FindShader(params string[] names)
    {
        foreach (string n in names)
        {
            Shader s = Shader.Find(n);
            if (s != null) return s;
        }
        return null;
    }
}