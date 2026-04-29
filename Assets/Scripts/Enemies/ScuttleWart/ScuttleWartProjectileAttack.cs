using UnityEngine;

/// <summary>
/// ScuttleWart ranged attack: animation events call <see cref="Fire"/> (or legacy <see cref="FireLaser"/>) on this component.
/// </summary>
public class ScuttleWartProjectileAttack : MonoBehaviour
{
    [SerializeField]
    GameObject projectilePrefab;

    [SerializeField]
    Transform firePoint;

    [SerializeField]
    float projectileSpeed = 10f;

    /// <summary>Primary hook for animation events (Option A).</summary>
    public void Fire()
    {
        HorizontalProjectileSpawn.Spawn(projectilePrefab, firePoint, projectileSpeed, transform);
    }

    /// <summary>Legacy animation event name; forwards to <see cref="Fire"/>.</summary>
    public void FireLaser()
    {
        Fire();
    }
}
