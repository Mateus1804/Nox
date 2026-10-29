using UnityEngine;
using UnityEngine.UI;

// Operações comuns dos dois adaptadores legados, sem mover os dados serializados das cenas.
public static class NoxPlayerControllerSupport
{
    public const float NormalSpeed = 6f;
    public const float PushSpeed = 3f;
    public const float DefaultJumpHeight = 3.5f;
    public const float ImpulseForce = 1700f;

    public static Vector3 ReadMovement()
    {
        return Vector3.ClampMagnitude(new Vector3(
            Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")), 1f);
    }

    public static bool CanReceiveContactDamage(Collider source, int playerId)
    {
        if (source == null)
            return false;
        NoxEnemyStrike strike = source.GetComponentInParent<NoxEnemyStrike>();
        if (strike != null)
            return strike.TryDamage(source, playerId);
        return source.CompareTag("Fogo") || source.CompareTag("Enemy") || source.CompareTag("Boss");
    }

    public static void UpdateHealthBar(Image bar, float health, float maximum)
    {
        if (bar == null)
            return;
        float ratio = NoxRules.HealthRatio(health, maximum);
        Color color = ratio > 2f / 3f ? Color.green : ratio > 1f / 3f ? Color.yellow : Color.red;
        if (bar.fillAmount != ratio)
            bar.fillAmount = ratio;
        if (bar.color != color)
            bar.color = color;
    }
}
