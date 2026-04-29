using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMovementConfig", menuName = "Isotope/Enemy/Movement Config")]
public class EnemyMovementConfig : ScriptableObject
{
    public float moveSpeed = 2f;
    [Tooltip("Patrol uses moveSpeed * this")]
    public float patrolSpeedMultiplier = 0.5f;
    [Tooltip("Retreat uses moveSpeed * this")]
    public float retreatSpeedMultiplier = 0.75f;
    public float flipDeadzone = 0.5f;
    public float groundCheckDistance = 0.5f;
    public float patrolWaitTime = 2f;
    public float minRigidbodyMass = 100f;
}
