using UnityEngine;

/// <summary>
/// Enemy-agnostic horizontal projectile spawn (prefab + RB velocity + optional flip). No weapon-specific naming.
/// </summary>
public static class HorizontalProjectileSpawn
{
    public static GameObject Spawn(
        GameObject prefab,
        Transform firePoint,
        float speed,
        Transform facingTransform)
    {
        if (prefab == null || firePoint == null || facingTransform == null)
            return null;

        GameObject spawned = Object.Instantiate(prefab, firePoint.position, Quaternion.identity);
        float facingDirection = facingTransform.localScale.x > 0f ? 1f : -1f;

        Rigidbody2D rb = spawned.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = new Vector2(facingDirection * speed, 0f);

        if (facingDirection < 0f)
        {
            Vector3 scale = spawned.transform.localScale;
            scale.x *= -1f;
            spawned.transform.localScale = scale;
        }

        return spawned;
    }
}
