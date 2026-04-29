/// <summary>
/// Shared damage contract for players, enemies, and destructibles.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
}
