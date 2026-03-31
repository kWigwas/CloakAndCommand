using UnityEngine;

/// <summary>2D orthographic camera: world points just outside the visible frustum (not at diagonal distance).</summary>
public static class OffScreenSpawn2D
{
    /// <summary>
    /// Pick a point around <paramref name="center"/> within a radius band, preferring points outside current view.
    /// Falls back to nearest edge shell near center if sampling fails.
    /// </summary>
    public static Vector2 RandomNearPointOutsideView(
        Vector2 center,
        Camera cam,
        float minRadius,
        float maxRadius,
        float marginWorld,
        int maxTries = 24)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return center;

        minRadius = Mathf.Max(0.05f, minRadius);
        maxRadius = Mathf.Max(minRadius + 0.05f, maxRadius);
        marginWorld = Mathf.Max(0.05f, marginWorld);
        maxTries = Mathf.Max(4, maxTries);

        for (int i = 0; i < maxTries; i++)
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float rad = Random.Range(minRadius, maxRadius);
            Vector2 p = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * rad;
            if (IsBeyondView(p, cam, marginWorld))
                return p;
        }

        // Fallback: push from center toward nearest outside-view shell so distance stays bounded.
        Vector2 c = cam.transform.position;
        float halfH = cam.orthographicSize + marginWorld;
        float halfW = cam.orthographicSize * cam.aspect + marginWorld;
        Vector2 toCenter = center - c;
        Vector2 dir = toCenter.sqrMagnitude > 1e-6f ? toCenter.normalized : Vector2.right;
        float tX = Mathf.Abs(dir.x) > 1e-5f ? halfW / Mathf.Abs(dir.x) : float.PositiveInfinity;
        float tY = Mathf.Abs(dir.y) > 1e-5f ? halfH / Mathf.Abs(dir.y) : float.PositiveInfinity;
        float t = Mathf.Min(tX, tY) + marginWorld * 0.5f;
        return c + dir * t;
    }

    /// <summary>
    /// Random point slightly past one viewport edge — stays near the playable band instead of megaworlds away.
    /// </summary>
    public static Vector2 RandomBeyondView(Camera cam, float marginWorld)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return Vector2.zero;

        marginWorld = Mathf.Max(0.05f, marginWorld);
        Vector2 c = cam.transform.position;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // how far past the edge (world units) — keeps spawns in a thin shell
        float past = Random.Range(marginWorld * 0.35f, marginWorld * 1.1f);
        // small stretch along parallel axis so not always middle-of-edge
        float alongPad = marginWorld * 0.35f;

        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: // right
                return new Vector2(c.x + halfW + past,
                    c.y + Random.Range(-halfH - alongPad, halfH + alongPad));
            case 1: // left
                return new Vector2(c.x - halfW - past,
                    c.y + Random.Range(-halfH - alongPad, halfH + alongPad));
            case 2: // top
                return new Vector2(c.x + Random.Range(-halfW - alongPad, halfW + alongPad),
                    c.y + halfH + past);
            default: // bottom
                return new Vector2(c.x + Random.Range(-halfW - alongPad, halfW + alongPad),
                    c.y - halfH - past);
        }
    }

    public static bool IsBeyondView(Vector2 world, Camera cam, float marginWorld)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return true;

        marginWorld = Mathf.Max(0f, marginWorld);
        Vector2 c = cam.transform.position;
        float halfH = cam.orthographicSize + marginWorld;
        float halfW = cam.orthographicSize * cam.aspect + marginWorld;
        return Mathf.Abs(world.x - c.x) > halfW || Mathf.Abs(world.y - c.y) > halfH;
    }
}
