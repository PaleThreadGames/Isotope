using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPerceptionConfig", menuName = "Isotope/Enemy/Perception Config")]
public class EnemyPerceptionConfig : ScriptableObject
{
    public float detectionRadius = 8f;
    public float attackRadius = 1.5f;
    public float hearingRadius = 3f;
    [Range(0f, 360f)]
    public float visionAngle = 120f;
    public float memoryDuration = 1.5f;
    public float retreatDistance = 2.5f;
    public LayerMask visionBlockers;
}
