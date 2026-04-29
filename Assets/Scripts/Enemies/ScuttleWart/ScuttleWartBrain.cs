using UnityEngine;

/// <summary>
/// ScuttleWart-specific AI: reads perception, selects state, drives IMovable + animator bridge.
/// </summary>
public class ScuttleWartBrain
{
    public enum State
    {
        Patrolling,
        Chasing,
        Attacking,
        Retreating,
    }

    readonly EnemyPerception _perception;
    readonly IMovable _motor;
    readonly EnemyAnimatorBridge _anim;
    readonly EnemyHealth _health;
    readonly PlayerReferenceSO _playerRef;

    public State CurrentState { get; private set; } = State.Patrolling;

    public ScuttleWartBrain(
        EnemyPerception perception,
        IMovable motor,
        EnemyAnimatorBridge anim,
        EnemyHealth health,
        PlayerReferenceSO playerRef)
    {
        _perception = perception;
        _motor = motor;
        _anim = anim;
        _health = health;
        _playerRef = playerRef;

        if (motor is EnemyGroundMovement ground)
            ground.IsPatrollingStateActive = () => CurrentState == State.Patrolling;
    }

    public void TickFixed()
    {
        if (_health.IsDead)
            return;

        if (_anim.IsMovementLockedByAttackAnimator)
        {
            _motor.StopHorizontal();
            return;
        }

        Transform player = _playerRef != null ? _playerRef.Target : null;
        if (player == null)
            return;

        PerceptionSnapshot snap = _perception.Tick(player);
        float dist = snap.DistanceToPlayer;
        float retreatDist = _perception.GetRetreatDistance();
        float attackRadius = _perception.GetAttackRadius();

        bool remembers = snap.RemembersPlayer;

        if (remembers && dist < retreatDist)
            CurrentState = State.Retreating;
        else if (dist < attackRadius && (snap.InVisionCone || snap.InHearingRange))
            CurrentState = State.Attacking;
        else if (remembers)
            CurrentState = State.Chasing;
        else
            CurrentState = State.Patrolling;

        switch (CurrentState)
        {
            case State.Patrolling:
                _motor.PatrolFixedUpdate();
                _anim.SetLocomotionSpeed(_motor.IsPatrolWaiting ? 0f : 1f);
                break;

            case State.Chasing:
                bool chaseMoved = _motor.ChaseFixedUpdate(player, false);
                _anim.SetLocomotionSpeed(chaseMoved ? 1f : 0f);
                break;

            case State.Retreating:
                bool retreatMoved = _motor.ChaseFixedUpdate(player, true);
                _anim.SetLocomotionSpeed(retreatMoved ? 1f : 0f);
                break;

            case State.Attacking:
                _motor.StopHorizontal();
                _anim.SetLocomotionSpeed(0f);
                _motor.PrepareAttackFacing(player);
                _anim.TriggerAttack();
                break;
        }
    }
}
