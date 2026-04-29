using UnityEngine;

/// <summary>
/// Computes sight/hearing/memory only; does not move or animate.
/// </summary>
public class EnemyPerception : MonoBehaviour
{
    [SerializeField]
    EnemyPerceptionConfig perceptionConfig;

    [SerializeField]
    Transform eyePoint;

    [SerializeField]
    PlayerReferenceSO playerReference;

    [Header("Legacy (used when perceptionConfig is null)")]
    [SerializeField]
    float retreatDistance = 2.5f;

    [SerializeField]
    float detectionRadius = 8f;

    [SerializeField]
    float attackRadius = 1.5f;

    [SerializeField]
    float hearingRadius = 3f;

    [SerializeField, Range(0f, 360f)]
    float visionAngle = 120f;

    [SerializeField]
    float memoryDuration = 1.5f;

    [SerializeField]
    LayerMask visionBlockers;

    float _timeSinceSawPlayer;

    public Transform EyePoint => eyePoint;

    public void ApplyBindings(
        EnemyPerceptionConfig config,
        Transform eye,
        PlayerReferenceSO playerRef,
        float legacyRetreat,
        float legacyDetection,
        float legacyAttack,
        float legacyHearing,
        float legacyVisionAngle,
        float legacyMemory,
        LayerMask legacyBlockers)
    {
        perceptionConfig = config;
        eyePoint = eye;
        playerReference = playerRef;
        if (perceptionConfig == null)
        {
            retreatDistance = legacyRetreat;
            detectionRadius = legacyDetection;
            attackRadius = legacyAttack;
            hearingRadius = legacyHearing;
            visionAngle = legacyVisionAngle;
            memoryDuration = legacyMemory;
            visionBlockers = legacyBlockers;
        }

        _timeSinceSawPlayer = EffectiveMemoryDuration;
    }

    void Awake()
    {
        _timeSinceSawPlayer = EffectiveMemoryDuration;
    }

    float EffectiveDetectionRadius => perceptionConfig != null ? perceptionConfig.detectionRadius : detectionRadius;
    float EffectiveAttackRadius => perceptionConfig != null ? perceptionConfig.attackRadius : attackRadius;
    float EffectiveHearingRadius => perceptionConfig != null ? perceptionConfig.hearingRadius : hearingRadius;
    float EffectiveVisionAngle => perceptionConfig != null ? perceptionConfig.visionAngle : visionAngle;
    float EffectiveMemoryDuration => perceptionConfig != null ? perceptionConfig.memoryDuration : memoryDuration;
    LayerMask EffectiveVisionBlockers => perceptionConfig != null ? perceptionConfig.visionBlockers : visionBlockers;

    public PerceptionSnapshot Tick(Transform player)
    {
        var snapshot = new PerceptionSnapshot();

        if (player == null)
            return snapshot;

        Vector3 sightOrigin = eyePoint != null ? eyePoint.position : transform.position;
        snapshot.DistanceToPlayer = Vector2.Distance(sightOrigin, player.position);

        Vector2 dirToPlayer = (player.position - sightOrigin).normalized;
        RaycastHit2D hit = Physics2D.Raycast(sightOrigin, dirToPlayer, snapshot.DistanceToPlayer, EffectiveVisionBlockers);
        snapshot.HasClearLineOfSight = hit.collider == null;

        Vector2 bugForward = transform.localScale.x > 0f ? Vector2.right : Vector2.left;
        float angleToPlayer = Vector2.Angle(bugForward, dirToPlayer);

        snapshot.InVisionCone = snapshot.DistanceToPlayer < EffectiveDetectionRadius
            && angleToPlayer < EffectiveVisionAngle / 2f
            && snapshot.HasClearLineOfSight;

        snapshot.InHearingRange = Vector2.Distance(transform.position, player.position) < EffectiveHearingRadius;

        if (snapshot.InVisionCone || snapshot.InHearingRange)
            _timeSinceSawPlayer = 0f;
        else
            _timeSinceSawPlayer += Time.fixedDeltaTime;

        snapshot.RemembersPlayer = _timeSinceSawPlayer < EffectiveMemoryDuration;

        return snapshot;
    }

    public float GetDistanceToPlayer(Transform player)
    {
        if (player == null)
            return float.MaxValue;

        Vector3 sightOrigin = eyePoint != null ? eyePoint.position : transform.position;
        return Vector2.Distance(sightOrigin, player.position);
    }

    public float GetRetreatDistance()
    {
        return perceptionConfig != null ? perceptionConfig.retreatDistance : retreatDistance;
    }

    public float GetAttackRadius()
    {
        return EffectiveAttackRadius;
    }

    void OnDrawGizmosSelected()
    {
        float detectR = perceptionConfig != null ? perceptionConfig.detectionRadius : detectionRadius;
        float retreatR = perceptionConfig != null ? perceptionConfig.retreatDistance : retreatDistance;
        float atkR = perceptionConfig != null ? perceptionConfig.attackRadius : attackRadius;
        float visAngle = perceptionConfig != null ? perceptionConfig.visionAngle : visionAngle;

        Vector3 sightOrigin = eyePoint != null ? eyePoint.position : transform.position;
        Vector3 forward = transform.localScale.x > 0f ? Vector3.right : Vector3.left;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(sightOrigin, retreatR);

        Vector3 upperLimit = Quaternion.Euler(0f, 0f, visAngle / 2f) * forward;
        Vector3 lowerLimit = Quaternion.Euler(0f, 0f, -visAngle / 2f) * forward;
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawRay(sightOrigin, upperLimit * detectR);
        Gizmos.DrawRay(sightOrigin, lowerLimit * detectR);

        Transform playerTransform = null;
        if (playerReference != null)
            playerTransform = playerReference.Target;

        if (playerTransform != null)
        {
            float d = Vector2.Distance(sightOrigin, playerTransform.position);
            LayerMask blockers = perceptionConfig != null ? perceptionConfig.visionBlockers : visionBlockers;
            RaycastHit2D losHit = Physics2D.Raycast(
                sightOrigin,
                (playerTransform.position - sightOrigin).normalized,
                d,
                blockers);
            Gizmos.color = losHit.collider == null ? Color.green : Color.red;
            Gizmos.DrawLine(sightOrigin, playerTransform.position);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(sightOrigin, atkR);
    }
}
