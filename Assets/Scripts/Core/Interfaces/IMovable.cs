using UnityEngine;

/// <summary>
/// Motor abstraction: ground enemies, flyers, etc. Brains depend on this, not concrete movement classes.
/// </summary>
public interface IMovable
{
    Transform Transform { get; }

    bool IsPatrolWaiting { get; }

    void PatrolFixedUpdate();
    bool ChaseFixedUpdate(Transform target, bool retreating);
    void StopHorizontal();
    void PrepareAttackFacing(Transform target);

    bool IsPatrolWaiting { get; }
}
