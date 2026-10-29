using UnityEngine;

// Uma linha por inimigo, criada uma vez; forma e espessura também comunicam a fase.
public sealed class NoxThreatCue : MonoBehaviour
{
    public Material material;
    private LineRenderer line;
    private static readonly Vector3[] directions = Ring();
    private static Vector3[] Ring()
    {
        var points = new Vector3[33];
        for (int i = 0; i < points.Length; i++)
        {
            float angle = i * Mathf.PI * 2 / (points.Length - 1);
            points[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }
        return points;
    }
    private readonly Vector3[] points = new Vector3[33];
    private void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = false;
        line.positionCount = points.Length;
        line.sharedMaterial = material;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.enabled = false;
    }
    public void Show(Vector3 center, float radius, NoxThreatPhase phase, float progress)
    {
        if (line == null)
            return;
        bool show = NoxSession.CanPlay && (phase == NoxThreatPhase.Windup || phase == NoxThreatPhase.Active);
        line.enabled = show && material != null;
        if (!line.enabled)
            return;
        if (line.sharedMaterial != material)
            line.sharedMaterial = material;
        float size = phase == NoxThreatPhase.Active || NoxPresentation.ReducedMotion ? radius : radius * Mathf.Lerp(0.4f, 1f, progress);
        for (int i = 0; i < points.Length; i++)
            points[i] = center + directions[i] * size;
        line.startWidth = line.endWidth = phase == NoxThreatPhase.Active ? 0.12f : 0.055f;
        line.startColor = line.endColor = phase == NoxThreatPhase.Active ? new Color(1f, 0.45f, 0.3f) : new Color(1f, 0.85f, 0.4f);
        line.SetPositions(points);
    }
    private void OnDisable()
    {
        if (line != null)
            line.enabled = false;
    }
}
