using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCombatConfig", menuName = "Isotope/Enemy/Combat Config")]
public class EnemyCombatConfig : ScriptableObject
{
    public float maxHealth = 3f;
    public float knockbackForceToPlayer = 18f;
}
